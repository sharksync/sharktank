'use strict';

const http = require('http');

module.exports = {

    server: "127.0.0.1",
    //server: "192.168.42.21",
    //server: "104.236.121.61",
    port: 5000,

    send: function (payload) {

        return new Promise(function (resolve, reject) {

            var requestBody = JSON.stringify(payload);

            var options = {
                host: module.exports.server,
                port: module.exports.port,
                path: '/api',
                method: 'POST',
                headers: {
                    'Content-length': requestBody.length,
                    'Content-type': 'application/json'
                }
            };

            // Fiddler debugging configuration
            //var options = {
            //    host: "localhost",
            //    port: "8888",
            //    path: 'http://192.168.42.21:5000/api',
            //    method: 'POST',
            //    headers: {
            //        'Content-length': requestBody.length,
            //        'Content-type': 'application/json',
            //        Host: "192.168.42.21:5000"
            //    }
            //};

            console.log("Starting Scale.send with payload: " + payload);

            var callback = function (response) {
                var responseString = ''
                response.on('data', function (chunk) {
                    responseString += chunk;
                });

                response.on('end', function () {
                    //console.log("Scale.send onData callback responded: " + responseString);

                    var response = JSON.parse(responseString);

                    //console.log("Scale.send onData callback json parsed");

                    if (response.error != undefined)
                        return reject(response.error);
                    else
                        return resolve(response);
                });

                response.on('error', function (err) {
                    return reject('error from server: ' + err);
                });
            }

            var req = http.request(options, callback);

            req.write(requestBody);
            req.end();
        })

    },

    createKeyspace: function (keyspaceName, replication) {
        var payload = { type: 'keyspace', payload: { keyspace: keyspaceName, command: 'create', replication: replication } };
        return module.exports.send(payload);
    },

    createTable: function (keyspaceName, tablename, pkname) {
        var statement = 'CREATE TABLE IF NOT EXISTS ' + tablename + ' (' + pkname + ' TEXT PRIMARY KEY, __clustertime INTEGER);';
        var payload = { type: 'keyspace', payload: { keyspace: keyspaceName, command: 'update', update: statement } };
        return module.exports.send(payload);
    },

    addColumn: function (keyspaceName, tablename, columnname, columntype) {
        var statement = 'ALTER TABLE ' + tablename + ' ADD COLUMN ' + columnname + ' ' + columntype + ';';
        var payload = { type: 'keyspace', payload: { keyspace: keyspaceName, command: 'update', update: statement } };
        return module.exports.send(payload);
    },

    addIndex: function (keyspaceName, tablename, indexname, columnname) {
        var statement = 'CREATE INDEX ' + indexname + ' ON ' + tablename + ' (' + columnname + ');';
        var payload = { type: 'keyspace', payload: { keyspace: keyspaceName, command: 'update', update: statement } };
        return module.exports.send(payload);
    },

    upsert: function (keyspaceName, partition, table, values) {
        // you must always provide the PK value in the values dictionary, then an INSERT OR REPLACE INTO is called.
        var payload = { type: 'client_write', payload: { keyspace: keyspaceName, partition: partition, table: table, values: values } };
        return module.exports.send(payload);
        // performance testing
        //return new Promise(function (resolve, reject) {
        //    resolve({});
        //});
    },

    query: function (keyspaceName, partition, query, params) {
        var payload = { type: 'client_read', payload: { keyspace: keyspaceName, partition: partition, query: query, params: params } };
        return module.exports.send(payload);
        // performance testing
        //return new Promise(function (resolve, reject) {
        //    resolve({
        //        results: [
        //            {
        //                rec_id: "",
        //                path: "",
        //                value: "",
        //                modified: "",
        //                tidemark: ""
        //            }]
        //    });
        //});
    }

}
