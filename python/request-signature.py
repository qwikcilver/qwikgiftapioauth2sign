import string
import json
import sys
import datetime
import traceback
import uuid
import requests,os
import time
import operator
import hmac
from logHelper import logHelper
from requests_oauthlib import OAuth1
import urllib.parse
import hashlib


'''
JSON encoded API request body parameters. This should be empty string in-case of API HTTP method is GET & DELETE
Json requestBody ""
'''
requestBody = {"q" : [{"f" : 1, "d" : 2}], "a" : "something"}
 
'''
API HTTP method
string requestHttpMethod ""
'''
requestHttpMethod = 'post'
 
'''
Absolute API endpoint URL. In-case if you are passing query parameter this should be in filled with query parameters as well
Make sure you configure this endpoint in database, because the endpoint will change for production environment
string absApiUrl ""
'''

absApiUrl = 'https://<This will be shared as part of provisioning the partner>/rest/v3/orders' 

'''
This will be shared as part of provisioning the partner
string clientSecret ""
'''

clientSecret = 'secret string'

def url_encoder(data):

    encoded_url = urllib.parse.quote_plus(data)
    return encoded_url
    
'''
Sorts the parameters according to the ASCII table.

string payload1

return string
'''

def sort_payload(payload1):
    for i,j in payload1.items():
        type_value = str(type(j))
        if 'list' in type_value :
            if len(j)>1:
                q = []
                for k in j:
                    d = sort_data(k)
                    q.append(d)

                payload1[i] = q
            else:
                l=[]
                payload1[i] = [sort_data(j[0])]
            
        elif 'dict' in type_value:
            payload1[i] = sort_data(j)
        else:
            continue
    
    sorted_payload=sort_data(payload1)
    return sorted_payload

def sort_data(data):
    E= dict(sorted(data.items(),key=operator.itemgetter(0),reverse = False))
    return E

'''
Sort all query parameters in the request according to the parameter name in ASCII table.

string url

return string

'''


def sort_queryparameters(url):
    query_parameter = url.split('?')
    if len(query_parameter) > 1:
        split_queryparameter = query_parameter[1].split('&')
        sorted_queryparameter = sorted(split_queryparameter)
        all_queryparameters = "&".join(sorted_queryparameter)
        url_with_sorted_queryparameters = e[0]+"?"+h
        return url_with_sorted_queryparameters
    elif len(query_parameter) <= 1:
        return url
        
'''
Concat the (request method(upper case), request host, request URL), encoded request parameters and encoded query parameters using & as the separator.

string absApiUrl
string requestHttpMethod
Json requestBody
string clientSecret

returns string
'''

def signature_genarator(requestBody,absApiUrl,requestHttpMethod,clientSecret):

    httpmethod = str(requestHttpMethod).upper()
    sorted_queryparameter = sort_queryparameters(absApiUrl)
    encoded_url = url_encoder(str(sorted_queryparameter))
    get_concat = httpmethod+'&'+encoded_url
    if requestBody != '': 
        sorted_payload = sort_payload(requestBody)
        sorted_payload_json = json.dumps(sorted_payload,separators=(',', ':'))
        modified_payload = sorted_payload_json.replace("'",r'"')
        payload_bytes = bytes(str(modified_payload), 'utf-8')
        encoded_payload = urllib.parse.quote(payload_bytes,safe='')
        post_concat = get_concat + '&' +encoded_payload
        body = bytes(str(post_concat), 'utf-8')
    elif requestBody == '':
        body = bytes(str(D), 'utf-8')
    secret_key = bytes(str(clientSecret), 'utf-8')
    signature = hmac.new(secret_key, body, hashlib.sha512).hexdigest()
    return signature
    
    
'''
function to be called for getting signature
'''
signature = signature_genarator(requestBody, absApiUrl, requestHttpMethod,clientSecret)

'''
In case of GET request pass the requestBody as empty string as below
requestBody = ''
'''
