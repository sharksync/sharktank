'use strict';

const Calibrate = require('calibrate');
const Joi = require('joi');
const async = require('async');

const Cassandra = require('../common/cassandra');

const syncController = {

    post: function (request, reply) {

        const appId = request.payload.app_id;
        const tasks = [];

        for (var tidemark of request.payload.tidemarks) {
            tasks.push(function (callback) {
                syncController.queryChangesForGroup(appId, tidemark.group, tidemark.tidemark, callback);
            });
        }

        tasks.push(function (callback) {
            syncController.queryDevice(appId, request.payload.device_id, callback);
        });

        async.parallel(tasks, function (err, results) {

            if (err) {
                return reply(Calibrate.error(err));
            }

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

            return reply(Calibrate.response(response));

        });

    },

    queryChangesForGroup: function (appId, group, tidemark, callback) {

        const client = Cassandra.getClient();

        var query = 'SELECT * FROM change WHERE app_id = ? AND group = ?';
        var params = [appId, group];

        if (tidemark != "" && tidemark != undefined) {

            query = query + ' AND modified > ?';
            params.push(tidemark);
        }

        client.execute(query, params, { prepare: true }, function (err, result) {

            if (err) {
                return callback(err, null);
            }

            callback(null, { group: group, changes: result.rows });
        });

    },

    queryDevice: function (appId, deviceId, callback) {

        const client = Cassandra.getClient();

        const query = 'SELECT * FROM device WHERE device_id = ?';
        const params = [deviceId];

        client.execute(query, params, { prepare: true }, function (err, result) {

            if (err) {
                return callback(err, null);
            }

            if (result.rows.count == 0) {
                return callback(new Error("Device record missing"), null);
            }

            const updateStatement = 'UPDATE device SET last_seen = dateof(now()) WHERE device_id = ?';
            const params = [deviceId];
            const deviceRecord = result.rows[0];

            client.execute(updateStatement, params, { prepare: true }, function (err, result) {

                callback(null, { device: deviceRecord });

            });

        });

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
                    app_key: Joi.string().guid().required(),
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