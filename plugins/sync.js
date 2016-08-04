'use strict';

const Calibrate = require('calibrate');
const Joi = require('joi');
const async = require('async');

const Cassandra = require('../common/cassandra');

const syncController = {

    post: function (request, reply) {

        const queries = syncController.getQueries(request);

        // First check its a valid app id / app key
        syncController.checkAccount(request)
            .then(function () {
                // Then fire all the group queries and update the device table
                return Promise.all(queries);
            })
            .then(results => {
                // If all that worked, format a return response
                return reply(syncController.formatResponse(results));
            })
            .catch(err => {
                return reply(Calibrate.error(err));
            });

    },

    checkAccount: function (request) {
        return new Promise(function (resolve, reject) {
            const client = Cassandra.getClient();

            const query = 'SELECT * FROM application WHERE app_id = ? AND app_api_access_key = ?';
            const params = [request.payload.app_id, request.payload.app_api_access_key];
            
            client.execute(query, params, { prepare: true }, function (err, result) {

                if (err) {
                    return reject(err);
                }

                if (result.rows.count == 0) {
                    return reject(new Error("App record missing with app id or access key"));
                }
                
                resolve(result.rows[0]);
            });

        });
    },
    
    getQueries: function (request) {

        const appId = request.payload.app_id;
        const queries = [];

        for (var tidemark of request.payload.tidemarks) {
            queries.push(syncController.queryChangesForGroup(appId, tidemark.group, tidemark.tidemark));
        }

        queries.push(syncController.queryDevice(appId, request.payload.device_id));

        return queries;
    },

    queryChangesForGroup: function (appId, group, tidemark) {
        return new Promise(function (resolve, reject) {
            const client = Cassandra.getClient();

            var query = 'SELECT * FROM change WHERE app_id = ? AND group = ?';
            var params = [appId, group];

            if (tidemark != "" && tidemark != undefined) {

                query = query + ' AND modified > ?';
                params.push(tidemark);
            }

            client.execute(query, params, { prepare: true }, function (err, result) {

                if (err) {
                    return reject(err);
                }

                resolve({ group: group, changes: result.rows });
            });
        });
    },

    queryDevice: function (appId, deviceId, callback) {
        return new Promise(function (resolve, reject) {
            const client = Cassandra.getClient();

            const query = 'SELECT * FROM device WHERE device_id = ?';
            const params = [deviceId];

            client.execute(query, params, { prepare: true }, function (err, result) {

                if (err) {
                    return reject(err);
                }

                if (result.rows.count == 0) {
                    return reject(new Error("Device record missing"));
                }

                const updateStatement = 'UPDATE device SET last_seen = dateof(now()) WHERE device_id = ?';
                const params = [deviceId];
                const deviceRecord = result.rows[0];

                client.execute(updateStatement, params, { prepare: true }, function (err, result) {

                    if (err) {
                        return reject(err);
                    }

                    resolve({ device: deviceRecord });

                });

            });

        });
    },
    
    formatResponse: function (results) {

        var response = {
            groups: []
        }

        for (var result of results) {

            if (result.group != undefined) {

                var groupResponse = {
                    group: result.group,
                    changes: []
                }

                for (var change of result.changes) {
                    groupResponse.changes.push({
                        path: change.path,
                        value: change.value,
                        timestamp: change.modified
                    });
                }

                response.groups.push(groupResponse);
            }
            else if (result.device != undefined) {
                response.sync_id = result.device.sync_id
            }
        }

        return Calibrate.response(response);
    }

};

exports.register = function (server, options, next) {

    server.route({
        method: 'POST',
        path: '/sync',
        handler: syncController.post,
        config: {
            validate: {
                payload: {
                    app_id: Joi.string().guid().required(),
                    app_api_access_key: Joi.string().guid().required(),
                    device_id: Joi.string().guid().required(),
                    client_time: Joi.number().required(),
                    changes: Joi.array().items(Joi.object({
                        path: Joi.string().required(),
                        value: Joi.string().required(),
                        timestamp: Joi.date().required(),
                        action: Joi.string().required(),
                        group: Joi.string().required(),
                    })),
                    tidemarks: Joi.array().items(Joi.object({
                        group: Joi.string().required(),
                        tidemark: [Joi.string().guid(), Joi.string().empty('')]
                    })),
                }
            }
        }
    });

    next();
};

exports.register.attributes = {
    pkg: {
        "name": "users",
        "version": "0.0.1",
        "description": "",
        "main": "index.js"
    }
}