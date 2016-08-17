'use strict';

const Cassandra = require('cassandra-driver');

exports.getClient = () => {

    //const client = new Cassandra.Client({ contactPoints: ['127.0.0.1'], keyspace: 'dev' });
    //const client = new Cassandra.Client({ contactPoints: ['192.168.42.3'], keyspace: 'dev' });
    const client = new Cassandra.Client({ contactPoints: ['db.sharksync'], keyspace: 'dev' });
    client.on('log', function (level, className, message, furtherInfo) {
        if (level == "warning" || level == "error")
            console.log('Cassandra: %s -- %s', level, message);
    });

    return client;
}
