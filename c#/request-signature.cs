        private string GetAuthenticationCode()
        {
            ApiSetupQC oApiSetupQC = db.ApiSetupQC.Where(a =&gt; a.apk_clientId == cny_id &amp;&amp; a.apk_status
== 1).FirstOrDefault();
            AuthenticationCode oAuthenticationCode = new AuthenticationCode();
            oAuthenticationCode.clientId = oApiSetupQC.apk_key;
            oAuthenticationCode.username = oApiSetupQC.apk_username;
            oAuthenticationCode.password = oApiSetupQC.apk_pass;
            string requestData = JsonConvert.SerializeObject(oAuthenticationCode);
            string apiResponse = CallApi(oApiSetupQC.apk_url + &quot;/oauth2/verify&quot;, requestData,
&quot;oauth2/verify&quot;);
 
            WoohoAPIV2.Entities.Response.AuthenticationCode oAuthenticationCodeResp =
JsonConvert.DeserializeObject&lt;WoohoAPIV2.Entities.Response.AuthenticationCode&gt;(apiResponse);
            return oAuthenticationCodeResp.authorizationCode;
        }

 
        private string CreateToken(string authCode)
        {
            ApiSetupQC oApiSetupQC = db.ApiSetupQC.Where(a =&gt; a.apk_clientId == cny_id &amp;&amp; a.apk_status
== 1).FirstOrDefault();
            var request = new RestRequest(Method.POST);
            CreateToken oCreateToken = new CreateToken();
            oCreateToken.authorizationCode = authCode;
            oCreateToken.clientId = oApiSetupQC.apk_key;
            oCreateToken.clientSecret = oApiSetupQC.apk_secret;
            string requestData = JsonConvert.SerializeObject(oCreateToken);
            string apiResponse = CallApi(oApiSetupQC.apk_url + &quot;/oauth2/token&quot;, requestData,
&quot;oauth2/token&quot;);
 
            WoohoAPIV2.Entities.Response.CreateToken oCreateTokenResp =
JsonConvert.DeserializeObject&lt;WoohoAPIV2.Entities.Response.CreateToken&gt;(apiResponse);
            return oCreateTokenResp.token;
        }

 
        private string CallApi(string _url, string _requestData, string _apiName)
        {
            string resp = &quot;&quot;;
            var client = new RestClient(_url);
            var request = new RestRequest(Method.POST);
            request.AddParameter(&quot;application/json&quot;, _requestData, ParameterType.RequestBody);
            int counter = 1;
            IRestResponse response = null;
            client.Timeout = 20000;
            try
            {
                response = client.Execute(request);
                if (response.StatusCode == 0)
                {
                    counter++;
                    response = client.Execute(request);
                    oAPIREQUESTQUICKSELVER.ARQC_STATUS = counter;
                }
            }
            catch
            {
                counter++;
                response = client.Execute(request);
            }
           
            if (response.StatusCode == 0)
            {
                throw new Exception(&quot;Error while calling &quot; + _apiName + &quot; API.&quot;);
            }
            if (Convert.ToString(response.Content) == &quot;&quot;)
            {
                throw new Exception(&quot;Blank response while calling &quot; + _apiName + &quot; API.&quot;);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                throw new Exception(&quot;ServiceUnavailable &quot; + _apiName);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                resp = response.Content.ToString();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception(&quot;Unauthorized&quot;);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)

            {
                throw new Exception(&quot;BadRequest - &quot; + response.Content.ToString());
            }
            return resp;
        }

 
       private string CallApiPost(string _url, string _requestData, string _apiName)
        {
            string resp = &quot;&quot;;
            var client = new RestClient(_url);
            var request = new RestRequest(Method.POST);
            request.AddParameter(&quot;application/json&quot;, _requestData, ParameterType.RequestBody);
            IRestResponse response = null;
            client.Timeout = 20000;
            string _signature = CreateSignature(_requestData, _url, &quot;POST&quot;, _url);
            request.AddHeader(&quot;signature&quot;, _signature);
            string _token = GetToken(&quot;&quot;);
            request.AddHeader(&quot;Authorization&quot;, &quot;Bearer &quot; + _token);
            string dateAtClient = TimeZoneInfo.ConvertTimeToUtc(System.DateTime.Now,
TimeZoneInfo.Local).ToString(&quot;yyyy-MM-ddTHH:mm:ss.fffZ&quot;);
            request.AddHeader(&quot;dateAtClient&quot;, dateAtClient);
 
            try
            {
                response = client.Execute(request);
                if (response.StatusCode == 0)
                {
                    counter++;
                    response = client.Execute(request);
                    oAPIREQUESTQUICKSELVER.ARQC_STATUS = counter;
                }
            }
            catch
            {
                counter++;
                response = client.Execute(request);
            }
 
            if (response.StatusCode == 0)
            {
                throw new Exception(&quot;Error while calling &quot; + _apiName + &quot; API.&quot;);

            }
            if (Convert.ToString(response.Content) == &quot;&quot;)
            {
                throw new Exception(&quot;Blank response while calling &quot; + _apiName + &quot; API.&quot;);
            }
           if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                throw new Exception(&quot;ServiceUnavailable &quot; + _apiName);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                resp = response.Content.ToString();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception(&quot;Unauthorized&quot;);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new Exception(&quot;BadRequest - &quot; + response.Content.ToString());
            }
            return resp;
        }
 

private string CreateSignature(string _requestBody, string _apiUrl, string _method, string
_apiUrlBase)
        {
            string _signature = &quot;&quot;;
            ApiSetupQC oApiSetupQC = db.ApiSetupQC.Where(a =&gt; a.apk_clientId == cny_id &amp;&amp; a.apk_status
== 1).FirstOrDefault();
 
            _apiUrl = GetSortedQueryString(_apiUrl);
            string D = &quot;&quot;;
            if (_apiUrl == &quot;&quot;)
            {
                D = _method + &quot;&amp;&quot; + Uri.EscapeDataString(_apiUrlBase);
            }
            else
            {
                D = _method + &quot;&amp;&quot; + _apiUrlBase + &quot;?&quot; + Uri.EscapeDataString(_apiUrl);
            }
 
            if (_requestBody != &quot;&quot;)
            {
                string newJsonData = StortJson(_requestBody);
               string newJson = Uri.EscapeDataString(newJsonData.Replace(&quot;\\&quot;, &quot;&quot;));
                D = D + &quot;&amp;&quot; + newJson;
            }
            _signature = hmac(D, oApiSetupQC.apk_secret);
            return _signature;
        }
  
private string hmac(string sig, string token)
        {
            string signature = string.Empty;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(token)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(sig));
                signature = BitConverter.ToString(hash).Replace(&quot;-&quot;, &quot;&quot;).ToLower();
            }
            return signature;
        }
 
        public string StortJson(string json)
        {
            var dic = JsonConvert.DeserializeObject&lt;SortedDictionary&lt;string, object&gt;&gt;(json);
            SortedDictionary&lt;string, object&gt; keyValues = new SortedDictionary&lt;string, object&gt;(dic);
            keyValues.OrderBy(m =&gt; m.Key);
            return JsonConvert.SerializeObject(keyValues);
        }
