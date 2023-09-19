const cryptoJS = require("crypto-js");

/**
 * JSON encoded API response body parameters.
 */
var responseBody = {"q" : [{"f" : 1, "d" : 2}], "a" : "something"};

/**
 * API HTTP method
 *
 */
var requestHttpMethod = 'post';

/**
 * Absolute API endpoint URL. In-case if you are passing query parameter this should be in filled with query parameters as well
 *
 * Make sure you configure this endpoint in database, because the endpoint will change for production environment
 *
 */
var absApiUrl = 'https://<This will be shared as part of provisioning the partner>/rest/v3/orders';

/**
 * This will be shared as part of provisioning the partner
 *
 */
var clientSecret = 'secret string';

/**
 * Sorts the parameters according to the ASCII table.
 */
let sortObject = (object) => {
    if (object instanceof Array) {
        var sortedObj = [],
            keys = Object.keys(object);
    }
    else {
        sortedObj = {},
            keys = Object.keys(object);
    }

    keys.sort(function (key1, key2) {
        if (key1 < key2) return -1;
        if (key1 > key2) return 1;
        return 0;
    });

    for (var index in keys) {
        var key = keys[index];
        if (typeof object[key] == 'object' && !!object[key]) {
            if ((object[key] instanceof Array)) {
                sortedObj[key] = sortObject(object[key]);
            }
            sortedObj[key] = sortObject(object[key]);
        } else {
            sortedObj[key] = object[key];
        }
    }
    return sortedObj;
}

/**
 * Sort all query parameters in the request according to the parameter name in ASCII table.
 */
let sortQueryParams = () => {
    var url = absApiUrl.split('?'),
        baseUrl = url[0],
        queryParam = url[1].split('&');

    absApiUrl = baseUrl + '?' + queryParam.sort().join('&');

    return fixedEncodeURIComponent(absApiUrl);
}

/**
 * Concat the (request method(upper case), request host, request URL), encoded response parameters and encoded query parameters using & as the separator.
 */
let getConcatenateBaseString = () => {
    var baseArray = [];
    baseArray.push(requestHttpMethod.toUpperCase());

    if (absApiUrl.indexOf('?') >= 0) {
        baseArray.push(sortQueryParams());
    } else {
        baseArray.push(fixedEncodeURIComponent(absApiUrl));
    }
    if (responseBody) {
        baseArray.push(fixedEncodeURIComponent(JSON.stringify(sortObject(responseBody))));
    }

    return baseArray.join('&');
}

let fixedEncodeURIComponent = (str) => {
    return encodeURIComponent(str).replace(/[!'()*]/g, function(c) {
        return '%' + c.charCodeAt(0).toString(16).toUpperCase();
    });
}

console.log(cryptoJS.HmacSHA512(getConcatenateBaseString(), clientSecret).toString());
