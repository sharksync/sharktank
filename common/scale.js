'use strict';

const net = require('net');

module.exports = {

    server: "127.0.0.1",
    port: 5000,

    send: function (payload) {

        return new Promise(function (resolve, reject) {

            var client = new net.Socket();
            var responseBuffer = null;

            // settings for the client
            client.setTimeout(30000, function () {
                client.destroy();
                return reject('error connecting to server');
            });

            client.setNoDelay(true);

            client.on('data', function (data) {
                // newmove the c++ \0 termination, if it arrived in time
                var buffer = new Buffer(data);
                if (responseBuffer == null) {
                    responseBuffer = buffer;
                } else {
                    responseBuffer = Buffer.concat([responseBuffer, buffer]);
                }

                if (responseBuffer != null && responseBuffer.length > 0) {
                    if (responseBuffer[responseBuffer.length - 1] == 0) {
                        responseBuffer = responseBuffer.slice(0, responseBuffer.length - 2);

                        // we are complete, lets rock
                        var response = JSON.parse(responseBuffer);
                        client.destroy();

                        if (response.error != undefined)
                            return reject(result.error);
                        else
                            return resolve(response);
                    }
                }
            });

            client.on('error', function (err) {
                return reject('error connecting to server');
            });

            client.connect(module.exports.port, module.exports.server, function () {

                var stringPayload = JSON.stringify(payload);
                var buffer = new Buffer(stringPayload);
                var nullTerminator = new Buffer([0x00, 0x00, 0x00]);

                client.write(Buffer.concat([buffer, nullTerminator]));

            });

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
    },

    query: function (keyspace, partition, query, params) {
        var payload = { type: 'client_read', payload: { keyspace: keyspaceName, partition: partition, query: query, params: params } };
        return module.exports.send(payload);
    }

}