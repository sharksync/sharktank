'use strict';
exports.handler = function (event, context, callback) {
    const response = event.Records[0].cf.response;
    const headers = response.headers;

    // Set some security headers
    headers['X-Frame-Options'] = [{ key: 'X-Frame-Options', value: 'Deny' }];
    headers['X-XSS-Protection'] = [{ key: 'X-XSS-Protection', value: '1; mode=block' }];
    headers['X-Content-Type-Options'] = [{ key: 'X-Content-Type-Options', value: 'nosniff' }];
    headers['Referrer-Policy'] = [{ key: 'Referrer-Policy', value: 'no-referrer' }];
    headers['Strict-Transport-Security'] = [{ key: 'Strict-Transport-Security', value: 'max-age=31536000; includeSubDomains; preload' }];
    headers['Expect-CT'] = [{ key: 'Expect-CT', value: 'enforce; max-age=86400;' }];
    headers['Content-Security-Policy'] = [{ key: 'Content-Security-Policy', value: "default-src 'self' data:;script-src 'self';style-src 'self' 'unsafe-inline';img-src 'self' data:;font-src 'self' data:;connect-src 'self' https://localhost:44325;block-all-mixed-content" }];

    callback(null, response);
};