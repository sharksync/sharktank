'use strict';

const net = require('net');
const server = "192.168.0.11"
const port = 5000;

const scale = {

    send: function (payload) {

        return new Promise(function (resolve, reject) {

            var client = new net.Socket();
        
            client.on('data', function (data) {
                // newmove the c++ \0 termination, if it arrived in time
                var buffer = new Buffer(data);
                if (buffer[buffer.length - 1] == 0) {
                    buffer = buffer.slice(0, buffer.length - 2);
                }
                var response = JSON.parse(buffer);
                client.destroy;
                return resolve(response);
            });

            client.on('error', function (err) {
                return reject('error connecting to server');
            });

            client.connect(port, server, function () {

                var stringPayload = JSON.stringify(payload);
                var buffer = new Buffer(stringPayload);
                var nullTerminator = new Buffer(0x00);

                client.write(buffer);
                client.write(new Buffer([0x00]));

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

            var statement = 'CREATE TABLE IF NOT EXISTS ' + tablename + ' (' + pkname + ' TEXT PRIMARY KEY, __clustertime INTEGER)';
            var payload = { type: 'keyspace', payload: { "keyspace": keyspaceName, "command": "update", "update": statement } };
            scale.send(payload).then(function (result) {
                return resolve(result);
            }).catch(function (err) {
                return reject('error');
            });

        });

    }

}

/*scale.createKeyspace('test1', 1).then(function (result) {
    if (result.error != null) {
        console.log('create keyspace :' + result.error);
    }
});*/
scale.createTable('test1', 'testtable', 'pk').then(function (result) {
    if (result.error != null) {
        console.log('create table :' + result.error);
    }
});

