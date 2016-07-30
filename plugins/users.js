'use strict';

const Calibrate = require('calibrate');
const Joi = require('joi');

const Cassandra = require('../common/cassandra');

exports.register = function (server, options, next) {
    
    server.route({
        method: 'GET',
        path: '/users',
        handler: function (request, reply) {

            const client = Cassandra.getClient();

            const query = 'SELECT userId, name FROM users';
            client.execute(query, null, { prepare: true }, function (err, result) {

                if (err) {
                    return reply(Calibrate.error(err));
                }
                
                return reply(Calibrate.response(result.rows));
            });
        }
    });

    server.route({
        method: 'GET',
        path: '/users/{userId}',
        handler: function (request, reply) {

            const client = Cassandra.getClient();
            
            const params = [request.params.userId];

            const query = 'SELECT userId, name FROM users WHERE userId=?';
            client.execute(query, params, { prepare: true }, function (err, result) {

                if (err) {
                    return reply(Calibrate.error(err));
                }

                return reply(Calibrate.response(result.rows[0]));
            });
        },
        config: {
            validate: {
                params: {
                    userId: Joi.number().integer().min(1)
                }
            }
        }
    });

    server.route({
        method: 'POST',
        path: '/users',
        handler: function (request, reply) {

            const client = Cassandra.getClient();
            
            const params = [request.payload.userId, request.payload.name];
            console.log(request.payload.name);
            const query = 'INSERT INTO users (userId, name) VALUES (?, ?)';
            client.execute(query, params, { prepare: true }, function (err, result) {

                if (err) {
                    return reply(Calibrate.error(err));
                }

                return reply(Calibrate.response(request.payload.userId));
            });
        },
        config: {
            validate: {
                payload: {
                    userId: Joi.number().integer().min(1),
                    name: Joi.string().min(1)
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