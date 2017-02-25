'use strict';

const Calibrate = require('calibrate');
const Joi = require('joi');
const async = require('async');
const Boom = require('boom');
const Q = require("q");

const moment = require('moment');

const Scale = require('../common/scale');

const environment = "dev";
const systemPartition = "shark_sync";

const syncController = {

    post: function (request, reply) {

        // Context is a container for everything needed for this request to be processed
        var context = {
            requestStartMoment: moment(),
            request: request,
            response: {
                groups: []
            }
        };

        // First check its a valid app id / app key
        syncController.checkAccount(context)
            .then(application => {
                // Grab the device record and update the last seen
                return syncController.checkDevice(context)
            })
            .then(device => {
                context.response.sync_id = device.device.sync_id

                // Then process all the changes from the client
                return syncController.processClientChanges(context);
            })
            .then(result => {

                // Then do all the group queries
                return syncController.getQueries(context);
            })
            .then(result => {

                // If all that worked, format a return response
                return reply(syncController.formatResponse(context, result));
            })
            .catch(err => {
                // If any of them died, return an error for it
                return reply(Calibrate.error(err));
            });
    },

    checkAccount: function (context) {
        return new Promise(function (resolve, reject) {

            const query = 'SELECT * FROM application WHERE app_id = ? AND app_api_access_key = ?';
            const params = [context.request.payload.app_id, context.request.payload.app_api_access_key];

            Scale.query(environment, systemPartition, query, params)
                .then(function (data) {

                    if (data.error != null)
                        return reject("Querying application failed with: " + data.error);

                    if (data.results.length == 0) {
                        return reject(Calibrate(Boom.unauthorized('app_id or app_api_access_key incorrect')));
                    }

                    return resolve(data.results[0]);
                })
                .catch(err => {
                    return reject(err);
                });
        });
    },

    checkDevice: function (context) {
        return new Promise(function (resolve, reject) {
            const deviceId = context.request.payload.device_id;

            const query = 'SELECT * FROM device WHERE device_id = ?';
            const params = [deviceId];
            var deviceRecord = null;

            Scale.query(environment, systemPartition, query, params)
                .then(function (data) {

                    if (data.error != null)
                        return reject("Querying device failed with: " + data.error);

                    if (data.results.length == 0) {
                        return reject(Boom.notFound('device_id not found'));
                    }

                    deviceRecord = data.results[0];

                    // Update the last seen column on the device table for this device
                    return Scale.upsert(environment, systemPartition, "device", { device_id: deviceId, last_seen: "%clustertime%" });
                })
                .then(function (data) {
                    if (data.error != null)
                        return reject("Device update failed with: " + data.error);

                    return resolve({ device: deviceRecord });
                })
                .catch(err => {
                    return reject(err);
                });
        });
    },

    processClientChanges: function (context) {

        var upserts = [];
        const appId = context.request.payload.app_id;
        const deviceId = context.request.payload.device_id;
        const requestStartMoment = context.requestStartMoment;

        for (var change of context.request.payload.changes) {
            upserts.push(syncController.processClientChange(context, appId, deviceId, requestStartMoment, change));
        }

        return Promise.all(upserts);
    },

    processClientChange: function (context, appId, deviceId, requestStartMoment, change) {
        return new Promise(function (resolve, reject) {

            var recordId = "";
            var path = "";

            // Path should contain a / in format <guid>/property.name
            if (change.path.indexOf("/") > -1) {
                recordId = change.path.substring(0, change.path.indexOf("/"));
                path = change.path.substring(change.path.indexOf("/") + 1);
            }
            
            const modifiedEpoch = moment(requestStartMoment).subtract(change.secondsAgo, 'seconds').unix();

            Scale.upsert(environment, appId + change.group, "change", {
                change_id: "%uuid%",
                rec_id: recordId,
                path: path,
                device_id: deviceId,
                modified: modifiedEpoch,
                tidemark: "%clustertime%",
                value: change.value
            })
                .then(function (data) {
                    if (data.error != null)
                        return reject("Change upsert failed with: " + data.error);

                    return resolve();
                })
                .catch(err => {
                    return reject(err);
                });
        });
    },

    getQueries: function (context) {

        const appId = context.request.payload.app_id;

        var queries = [];

        for (var group of context.request.payload.groups) {
            queries.push(syncController.queryChangesForGroup(context, appId, group.group, group.tidemark));
        }

        return Promise.all(queries);
    },

    queryChangesForGroup: function (context, appId, group, tidemark) {
        return new Promise(function (resolve, reject) {

            var query = "SELECT * FROM change";
            var params = [];

            if (tidemark != "" && tidemark != undefined) {

                query += " WHERE tidemark > ?";
                params.push(tidemark);
            }

            query += " ORDER BY tidemark LIMIT 20";

            Scale.query(environment, appId + group, query, params)
                .then(function (data) {

                    if (data.error != null)
                        return reject("Querying change table failed with: " + data.error);

                    resolve({ group: group, changes: data.results });
                })
                .catch(err => {
                    return reject(err);
                });
        });
    },

    formatResponse: function (context, results) {

        for (var result of results) {

            if (result.group != undefined) {

                var groupResponse = {
                    group: result.group,
                    tidemark: null,
                    changes: []
                }

                for (var change of result.changes) {
                    if (change != undefined) {
                        groupResponse.changes.push({
                            path: change.rec_id + '/' + change.path,
                            value: change.value,
                            modified: change.modified,
                        });
                        groupResponse.tidemark = change.tidemark;
                    }
                }

                context.response.groups.push(groupResponse);
            }
        }

        return Calibrate.response(context.response);
    }
};

exports.controller = syncController;

exports.register = function (server, options, next) {

    server.route({
        method: "POST",
        path: "/sync",
        handler: syncController.post,
        config: {
            validate: {
                payload: {
                    app_id: Joi.string().guid().required(),
                    app_api_access_key: Joi.string().guid().required(),
                    device_id: Joi.string().guid().required(),
                    changes: Joi.array().items(Joi.object({
                        path: Joi.string().required(),
                        value: Joi.string().required(),
                        secondsAgo: Joi.number().required(),
                        operation: Joi.number().required(),
                        group: Joi.string().required()
                    })),
                    groups: Joi.array().items(Joi.object({
                        group: Joi.string().required(),
                        tidemark: [Joi.string().guid(), Joi.string().empty(''), Joi.allow(null)]
                    }))
                }
            }
        }
    });

    next();
};

exports.register.attributes = {
    pkg: {
        "name": "sync",
        "version": "0.0.1",
        "description": "",
        "main": "sync.js"
    }
}
