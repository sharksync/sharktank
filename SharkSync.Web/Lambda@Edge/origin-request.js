'use strict';
exports.handler = function (event, context, callback) {
    const request = event.Records[0].cf.request;
    const headers = request.headers;

    var requestUrl = request.uri.toLowerCase();

    // Forward all pages to the index.html (pages are any requests without extensions)
    if (requestUrl.indexOf(".") === -1) {
        request.uri = "/index.html"
    }

    callback(null, request);
};