'use strict';

const Calibrate = require('calibrate');
const Joi = require('joi');
const async = require('async');
const Boom = require('boom');

const moment = require('moment');

const CassandraDriver = require('cassandra-driver');
const Cassandra = require('../common/cassandra');
const TimeUuid = CassandraDriver.types.TimeUuid;

const syncController = {

    post: function (request, reply) {

        request.payload.requestStartMoment = moment();

        // First check its a valid app id / app key
        syncController
            .checkAccount(request)
            .then(function () {
                // Then process all the changes from the client
                return syncController.processClientChanges(request);
            })
            .then(function () {
                // Then fire all the group queries and update the device table
                return Promise.all(syncController.getQueries(request));
            })
            .then(results => {
                // If all that worked, format a return response
                return reply(syncController.formatResponse(results));
            })
            .catch(err => {
                // If any of them died, return an error for it
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

                if (result.rows.length == 0) {
                    return reject(Calibrate(Boom.unauthorized('app_id or app_api_access_key incorrect')));
                }
                
                resolve(result.rows[0]);
            });

        });
    },

    processClientChanges: function (request) {

        const inserts = [];
        const appId = request.payload.app_id;
        const requestStartMoment = request.payload.requestStartMoment;

        for (var group of request.payload.groups) {
                
            for (var change of group.changes) {
                inserts.push(syncController.processClientChange(change, appId, requestStartMoment, group.group));
            }
        }

        return Promise.all(inserts);
    },

    processClientChange: function (change, appId, requestStartMoment, groupName) {
        return new Promise(function (resolve, reject) {
            const client = Cassandra.getClient();

            const recordId = change.path.substring(0, change.path.indexOf("/"));
            const date = requestStartMoment.subtract(change.secondsAgo, 'seconds').toDate();
            const timeuuid = TimeUuid.fromDate(date);

            const insertStatement = 'INSERT INTO change (app_id, rec_id, path, group, modified, value) VALUES (?,?,?,?,?,?)';
            const params = [appId, recordId, change.path, groupName, timeuuid, change.value];

            client.execute(insertStatement, params, { prepare: true }, function (err, result) {

                if (err) {
                    return reject(err);
                }

                resolve();

            });
        });
    },

    getQueries: function (request) {

        const appId = request.payload.app_id;
        const queries = [];

        // Work out changes for each group
        for (var group of request.payload.groups) {
            queries.push(syncController.queryChangesForGroup(appId, group.group, group.tidemark));
        }

        // Grab the device record and update the last seen
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

                if (result.rows.length == 0) {
                    return reject(Boom.notFound('device_id not found'));
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
                    groups: Joi.array().items(Joi.object({
                        group: Joi.string().required(),
                        tidemark: [Joi.string().guid(), Joi.string().empty('')],
                        changes: Joi.array().items(Joi.object({
                            path: Joi.string().required(),
                            value: Joi.string().required(),
                            secondsAgo: Joi.number().required()
                        }))
                    }))
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