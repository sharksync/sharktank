'use strict';

const net = require('net');
const server = "127.0.0.1"
const port = 5000;

const scale = {

    send: function (payload) {

        return new Promise(function (resolve, reject) {

            var client = new net.Socket();
            var responseBuffer = null;

            // settings for the client
            client.setTimeout(30000,function() {
                client.destroy();
                return reject('error connecting to server');
            });

            client.setNoDelay(true);

            client.on('data', function (data) {
                // newmove the c++ \0 termination, if it arrived in time
                var buffer = new Buffer(data);
                if(responseBuffer == null) {
                    responseBuffer = buffer;
                } else {
                    responseBuffer = Buffer.concat([responseBuffer,buffer]);
                }

                if(responseBuffer != null && responseBuffer.length > 0){
                    if(responseBuffer[responseBuffer.length-1] == 0) {
                        responseBuffer = responseBuffer.slice(0, responseBuffer.length - 2);

                        // we are complete, lets rock
                        var response = JSON.parse(responseBuffer);
                        client.destroy;
                        return resolve(response);

                    }
                }

                
            });

            client.on('error', function (err) {
                return reject('error connecting to server');
            });

            client.connect(port, server, function () {

                var stringPayload = JSON.stringify(payload);
                var buffer = new Buffer(stringPayload);
                var nullTerminator = new Buffer([0x00, 0x00, 0x00]);

                client.write(Buffer.concat([buffer, nullTerminator]));

            });

        })

    },

    createKeyspace: function (keyspaceName,replication) {

        return new Promise(function (resolve, reject) {

            var payload = { type: 'keyspace', payload: { "keyspace": keyspaceName, "command": "create", "replication": replication } };
            scale.send(payload).then(function (result) {
                return resolve(result);
            }).catch(function (err) {
                return reject('error');
            });

        });

    },

    createTable: function (keyspaceName, tablename, pkname) {

        return new Promise(function (resolve, reject) {

            var statement = 'CREATE TABLE IF NOT EXISTS ' + tablename + ' (' + pkname + ' TEXT PRIMARY KEY, __clustertime INTEGER);';
            var payload = { type: 'keyspace', payload: { "keyspace": keyspaceName, "command": "update", "update": statement } };
            scale.send(payload).then(function (result) {
                return resolve(result);
            }).catch(function (err) {
                return reject('error');
            });

        });

    },

    addColumn: function (keyspaceName, tablename, columnname, columntype) {

        return new Promise(function (resolve, reject) {

            var statement = 'ALTER TABLE ' + tablename + ' ADD COLUMN ' + columnname +' '+ columntype + ';';
            var payload = { type: 'keyspace', payload: { "keyspace": keyspaceName, "command": "update", "update": statement } };
            scale.send(payload).then(function (result) {
                return resolve(result);
            }).catch(function (err) {
                return reject('error');
            });

        });

    },

    addIndex: function (keyspaceName, tablename, indexname, columnname) {

        return new Promise(function (resolve, reject) {

            var statement = 'CREATE INDEX ' + indexname + ' ON ' + tablename +' ('+ columnname + ');';
            var payload = { type: 'keyspace', payload: { "keyspace": keyspaceName, "command": "update", "update": statement } };
            scale.send(payload).then(function (result) {
                return resolve(result);
            }).catch(function (err) {
                return reject('error');
            });

        });

    }

    upsert: function(keyspace, table, values) {
        // you must always provide the PK value in the values dictionary, then an INSERT OR REPLACE INTO is called.
        var payload = { type: 'keyspace', payload: { "keyspace": keyspaceName, "command": "update", "update": statement } };
    }

    query: function(keyspace, query, params) {
        
    }

}

scale.createKeyspace('test1', 1).then(function (result) {
    if (result.error != null) {
        console.log('create keyspace :' + result.error);
    }
}).then(function(){

    scale.createTable('test1', 'testtable', 'pk').then(function (result) {
    if (result.error != null) {
        console.log('create table :' + result.error);
    }
    }).catch(function(err){
        console.log(err);
    });

}).then(function() {

    scale.addColumn('test1','testtable','poop','TEXT');

})





