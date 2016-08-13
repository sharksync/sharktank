'use strict';

const Hapi = require('hapi');
const Good = require('good');
const Calibrate = require('calibrate');

const server = new Hapi.Server();
server.connection({ port: 3000 });

server.register([
    { register: require('./plugins/sync') },
    { register: Calibrate.decorate },
    {
        register: Good,
        options: {
            reporters: {
                console: [{
                    module: 'good-squeeze',
                    name: 'Squeeze',
                    args: [{
                        response: '*',
                        log: '*'
                    }]
                }, {
                        module: 'good-console'
                    }, 'stdout']
            }
        }
    }
],
    (err) => {

        if (err) {
            throw err; // something bad happened loading the plugin
        }

        server.start((err) => {

            if (err) {
                throw err;
            }
            server.log('info', 'Server running at: ' + server.info.uri);
        });
    });