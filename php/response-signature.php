<?php
/**
 * JSON encoded API response body parameters.
 *
 * @var string $responseBody
 */
$responseBody = '{"q" : [{"f" : 1, "d" : 2}], "a" : "something"}';

/**
 * API HTTP method
 *
 * @var string $requestHttpMethod
 */
$requestHttpMethod = 'post';

/**
 * Absolute API endpoint URL. In-case if you are passing query parameter this should be in filled with query parameters as well
 *
 * Make sure you configure this endpoint in database, because the endpoint will change for production environment
 *
 * @var string $absApiUrl
 */
$absApiUrl = 'https://<This will be shared as part of provisioning the partner>/rest/v3/orders';

/**
 * This will be shared as part of provisioning the partner
 *
 * @var string $clientSecret
 */
$clientSecret = 'secret string';

/**
 * Sorts the parameters according to the ASCII table.
 *
 * @param array $params
 */
function sortParams(array &$params)
{
    ksort($params);
    foreach ($params as $key => &$value) {
        $value = is_object($value) ? (array) $value : $value;
        if (is_array($value)) {
            sortParams($value);
        }
    }
}

/**
 * Sort all query parameters in the request according to the parameter name in ASCII table.
 *
 * @param string $queryParam
 * @return string
 */
function sortQueryParams($queryParam)
{
    $query = explode('&', $queryParam);
    asort($query, SORT_STRING);
    return implode('&', $query);
}

/**
 * Concat the (request method(upper case), request host, request URL), encoded response parameters and encoded query parameters using & as the separator.
 *
 * @param string $absApiUrl
 * @param string $requestHttpMethod
 * @param string $responseBody
 *
 * @return string
 */
function getConcatenateBaseString($absApiUrl, $requestHttpMethod, $responseBody)
{
    $baseStrings = [];

    $baseStrings[] = strtoupper($requestHttpMethod);
    $url = explode('?', $absApiUrl);
    $apiUrl = $url[0];

    if (isset($url[1])) {
        $baseStrings[] = rawurlencode($apiUrl . '?' . sortQueryParams($url[1]));
    }
    else {
        $baseStrings[] = rawurlencode($apiUrl);
    }

    if ($responseBody) {
        $jsonDecodedResponseBody = json_decode($responseBody, TRUE);
        sortParams($jsonDecodedResponseBody);
        $baseStrings[] = rawurlencode(
            json_encode($jsonDecodedResponseBody, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES)
        );
    }

    return implode('&', $baseStrings);
}

echo hash_hmac('sha512', getConcatenateBaseString($absApiUrl, $requestHttpMethod, $responseBody), $clientSecret);
