package com.qc.rest.service;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import org.apache.commons.codec.digest.HmacAlgorithms;
import org.apache.commons.codec.digest.HmacUtils;

import java.io.IOException;
import java.net.URLEncoder;
import java.util.*;

public class OAuth2Signature {

    public static String signatureGenerator(String requestHttpMethod,
                                            Object payload,
                                            String apiURL,
                                            String clientSecret) throws IOException {
        String signature;
        if (payload == null || payload == "") {
            // Sort the query params
            String endPoint;

            if (apiURL.contains("?")) {
                // If API url has query parameters
                String[] queryArray = apiURL.substring(apiURL.indexOf("?") + 1).split("&");
                Arrays.sort(queryArray);
                String queryParams = String.join("&", queryArray);
                String[] splitArray = apiURL.split("\\?");
                String finalUrl = splitArray[0] + "?" + queryParams;
                endPoint = URLEncoder.encode(finalUrl, "UTF-8");
            } else {
                endPoint = URLEncoder.encode(apiURL, "UTF-8");
            }

            // Generate the query string
            // httpMethod = GET, DELETE
            String baseString = requestHttpMethod + "&" + endPoint;
            signature = new HmacUtils(HmacAlgorithms.HMAC_SHA_512, clientSecret).hmacHex(baseString);
        } else {
            // Convert the payload to string
            ObjectMapper mapper = new ObjectMapper();
//            String jsonString = mapper.writeValueAsString(payload);

            // Sort the payload
            mapper.configure(SerializationFeature.ORDER_MAP_ENTRIES_BY_KEYS, true);
            HashMap map = mapper.readValue((String) payload, HashMap.class);
            String json = mapper.writeValueAsString(map);

            // Generate the query string
            String requestData = URLEncoder.encode(json, "UTF-8").replace("+", "%20"); // Replacing '+' with '%20'
            String endPoint = URLEncoder.encode(apiURL, "UTF-8");

            // httpMethod = POST, PUT
            String baseString = requestHttpMethod + "&" + endPoint + "&" + requestData;
            signature = new HmacUtils(HmacAlgorithms.HMAC_SHA_512, clientSecret).hmacHex(baseString);
        }
        return signature;
    }
    
    public static void main(String[] args) throws IOException {
        // POST, GET, PUT, DELETE
        String requestHttpMethod = "POST";

        // In case of GET request requestBody=""
        String requestBody = "{\"q\":[{\"f\":1,\"d\":2}],\"a\":\"something\"}";

        // Absolute API endpoint URL. In-case if you are passing query parameter this should be in filled with query parameters as well
        //Make sure you configure this endpoint in database, because the endpoint will change for production environment
        String apiURL = "https://<This will be shared as part of provisioning the partner>/rest/v3/orders";

        // This will be shared as part of provisioning the partner
        String clientSecret = "secret string";

        String oauth2Signature = signatureGenerator(requestHttpMethod, requestBody, apiURL, clientSecret);
        System.out.println("Signature - "+oauth2Signature);

        // Signature - c7a40d9d05597bb236ab62f821901c58819a6dd6abf90fb8287c8ef8e3d8ab44f6cd0033c8c6970c36d86c628a59c2718993e0bc7becc12746cda755affa39f0
    }
}
