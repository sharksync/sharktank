'use strict';

const proxyquire = require('proxyquire');
const sinon = require('sinon');
const chai = require("chai");
const chaiAsPromised = require("chai-as-promised");
const assert = chai.assert;

chai.use(chaiAsPromised);

describe('Sync.js', function () {

    beforeEach(function () {
    });

    afterEach(function () {
    });

    it('checkAccount', function () {

        var appRecord = {};
        var cassandra = {};
        var sync = proxyquire('../controllers/sync', { '../common/cassandra': cassandra });

        cassandra.getClient = function () {
            return {
                execute: function (query, params, props, done) {

                    assert.equal(query, 'SELECT * FROM application WHERE app_id = ? AND app_api_access_key = ?', "SQL query incorrect");
                    assert.equal(params[0], request.payload.app_id, "First parameter should be app id");
                    assert.equal(params[1], request.payload.app_api_access_key, "Second parameter should be app api access key");

                    assert.ok(props.prepare, "Statement should be prepared");

                    setTimeout(function () {
                        done(null, {
                            rows: [appRecord]
                        });
                    }, 0);
                }
            }
        };

        var request = {
            payload: {
                app_id: "",
                app_api_access_key: ""
            }
        };

        var resultPromise = sync.controller.checkAccount(request);

        return assert.eventually.equal(resultPromise, appRecord, "checkAccount should return the single app");
    });
})
