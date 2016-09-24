'use strict';

const Calibrate = require('calibrate');
const Joi = require('joi');
const async = require('async');
const Boom = require('boom');
const Q = require("q");

const moment = require('moment');

const Scale = require('../common/scale');

const environment = "dev";
const systemPartition = "shark";

const syncController = {

    test: function (request, reply) {

        Scale.query(environment, environment, 'SELECT * FROM change', null)
            .then(function (response) {
                return reply(Calibrate.response(response));
            })
            .catch(err => {
                // If any of them died, return an error for it
                return reply(Calibrate.error(err));
            });

    },

    post: function (request, reply) {

        // Context is a container for everything needed for this request to be processed
        var context = {
            requestStartMoment: moment(),
            request: request,
            response: {
                groups: []
            }
        };

        var queryResults = [];

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

                // Then fire the group queries one at a time
                return syncController.getQueries(context, queryResults);
            })
            .then(lastResult => {

                // As the queries is run one at a time, scoop up the last result
                queryResults.push(lastResult);

                // If all that worked, format a return response
                return reply(syncController.formatResponse(context, queryResults));
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
                .then(function (result) {

                    if (result.rows.length == 0) {
                        return reject(Calibrate(Boom.unauthorized('app_id or app_api_access_key incorrect')));
                    }

                    resolve(result.rows[0]);
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

            Scale.query(environment, systemPartition, query, params)
                .then(function (result) {

                    if (result.rows.length == 0) {
                        return reject(Boom.notFound('device_id not found'));
                    }

                    const deviceRecord = result.rows[0];

                    // Update the last seen column on the device table for this device
                    Scale.upsert(environment, systemPartition, "device", { device_id: deviceId, last_seen: moment().toDate() })
                        .then(function (result) {
                            resolve({ device: deviceRecord });
                        })
                        .catch(err => {
                            return reject(err);
                        });
                })
                .catch(err => {
                    return reject(err);
                });
        });
    },

    processClientChanges: function (context) {
        return new Promise(function (resolve, reject) {

            var inserts = [];
            const appId = context.request.payload.app_id;
            const deviceId = context.request.payload.device_id;
            const requestStartMoment = context.requestStartMoment;

            for (var change of context.request.payload.changes) {

                var recordId = "";
                var path = "";

                // Path should contain a / in format <guid>/property.name
                if (change.path.indexOf("/") > -1) {
                    recordId = change.path.substring(0, change.path.indexOf("/"));
                    path = change.path.substring(change.path.indexOf("/") + 1);
                }

                const date = moment(requestStartMoment).subtract(change.secondsAgo, 'seconds').toDate();

                inserts.push({
                    query: 'INSERT INTO change (app_id, record_id, path, client_modified, group, value, device_id) VALUES (?,?,?,?,?,?,?)',
                    params: [appId, recordId, path, date, change.group, change.value, deviceId]
                });

                inserts.push({
                    query: 'INSERT INTO sync (app_id, group, tidemark, record_id, operation, path) VALUES (?,?,now(),?,?,?)',
                    params: [appId, change.group, recordId, change.operation, path]
                });

                if (inserts.length > 20) {
                    context.client.batch(inserts, { prepare: true }, function (err) {
                        if (err) {
                            return reject(err);
                        }
                    });

                    // Reset the array for the next inserts
                    inserts = [];
                }

            }

            if (inserts.length > 0) {
                context.client.batch(inserts, { prepare: true }, function (err) {
                    if (err) {
                        return reject(err);
                    }

                    return resolve();
                });
            }
            else {
                return resolve();
            }
        });
    },

    getQueries: function (context, results) {

        const appId = context.request.payload.app_id;

        // Work out changes for each group in serial to not hit the DB too hard
        var lastPromise = context.request.payload.groups.reduce(function (promise, group) {
            return promise.then(function (result) {
                if (result != undefined)
                    results.push(result);
                return syncController.queryChangesForGroup(context, appId, group.group, group.tidemark);
            });
        }, Q.resolve())

        return lastPromise;
    },

    queryChangesForGroup: function (context, appId, group, tidemark) {
        return new Promise(function (resolve, reject) {

            const eventualConsistancyBufferDate = moment().subtract(1, "seconds").toDate();
            const eventualConsistancyBufferTimeUUID = TimeUuid.fromDate(eventualConsistancyBufferDate);

            var query = "SELECT * FROM sync WHERE app_id = ? AND group = ? AND tidemark < ?";
            var params = [appId, group, eventualConsistancyBufferTimeUUID];

            if (tidemark != "" && tidemark != undefined) {

                query += " AND tidemark > ?";
                params.push(tidemark);
            }

            query += " LIMIT 20";

            context.client.execute(query, params, { prepare: true }, function (err, result) {

                if (err) {
                    return reject(err);
                }

                var latestTidemark = "";
                var groupedSyncRecords = {};

                // Dedup all the sync records using a dictionary
                for (var syncRecord of result.rows) {
                    var recordIdAndPath = syncRecord.record_id + '/' + syncRecord.path;

                    groupedSyncRecords[recordIdAndPath] = syncRecord;

                    latestTidemark = syncRecord.tidemark;
                }

                // TODO: sync dud app_id,group,tidemark

                syncController.queryChangeRecordsForSyncRecords(context, appId, group, groupedSyncRecords).then(results => {
                    return resolve({ group: group, changes: results, tidemark: latestTidemark });
                })
                    .catch(err => {
                        return reject(err);
                    });

            });
        });
    },

    queryChangeRecordsForSyncRecords: function (context, appId, group, syncRecordsDictionary) {

        const queries = [];

        for (var key in syncRecordsDictionary) {
            queries.push(syncController.queryChangeRecordsForSyncRecord(context, appId, group, syncRecordsDictionary[key]));
        }

        return Promise.all(queries);
    },

    queryChangeRecordsForSyncRecord: function (context, appId, group, syncRecord) {
        return new Promise(function (resolve, reject) {

            var query = "SELECT * FROM change WHERE app_id = ? AND group = ? AND record_id = ? AND path = ? LIMIT 1";
            var params = [appId, group, syncRecord.record_id, syncRecord.path];

            context.client.execute(query, params, { prepare: true }, function (err, result) {

                if (err) {
                    return reject(err);
                }

                resolve({ change: result.rows[0], sync: syncRecord });
            });
        });
    },

    formatResponse: function (context, results) {

        for (var result of results) {

            if (result.group != undefined) {

                var groupResponse = {
                    group: result.group,
                    tidemark: result.tidemark,
                    changes: []
                }

                for (var item of result.changes) {
                    if (item != undefined && item.change != undefined && item.sync != undefined) {
                        groupResponse.changes.push({
                            path: item.change.record_id + '/' + item.change.path,
                            value: item.change.value,
                            client_modified: item.change.client_modified,
                            operation: item.sync.operation
                        });
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

    server.route({
        method: "GET",
        path: "/sync",
        handler: syncController.test
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
