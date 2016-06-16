//
//  API.AI .NET SDK - client-side libraries for API.AI
//  =================================================
//
//  Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
//  https://www.api.ai
//
//  ***********************************************************************************************************************
//
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
//  the License. You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
//  an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
//  specific language governing permissions and limitations under the License.
//
//  ***********************************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ApiAiSDK.Http;
using ApiAiSDK.Model;
using Newtonsoft.Json;

namespace ApiAiSDK
{
    public class AIDataService
    {
        private AIConfiguration config;

        private string sessionId;

        public string SessionId
        {
            get
            {
                return sessionId;
            }

            set
            {
                sessionId = value;
            }
        }

        public AIDataService(AIConfiguration config)
        {
            this.config = config;

            if (string.IsNullOrEmpty(config.SessionId))
            {
                sessionId = Guid.NewGuid().ToString();
            }
            else
            {
                sessionId = config.SessionId;
            }
        }

        public async Task<AIResponse> RequestAsync(AIRequest request)
        {

            request.Language = config.Language.code;
            request.Timezone = TimeZoneInfo.Local.StandardName;
            request.SessionId = sessionId;

            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(config.RequestUrl);
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json; charset=utf-8";
                httpRequest.Accept = "application/json";

                httpRequest.Headers["Authorization"] = "Bearer " + config.ClientAccessToken;
                
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, jsonSettings);

                if (config.DebugLog)
                {
                    Debug.WriteLine("Request: " + jsonRequest);
                }

                var requestStream = await httpRequest.GetRequestStreamAsync();
                using (var streamWriter = new StreamWriter(requestStream))
                {
                    streamWriter.Write(jsonRequest);
                    streamWriter.Dispose();
                }

                var httpResponse = await httpRequest.GetResponseAsync() as HttpWebResponse;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    if (config.DebugLog)
                    {
                        Debug.WriteLine("Response: " + result);
                    }

                    var aiResponse = JsonConvert.DeserializeObject<AIResponse>(result);

                    CheckForErrors(aiResponse);

                    return aiResponse;
                }

            }
            catch (Exception e)
            {
                throw new AIServiceException(e);
            }
        }

        public async Task<AIResponse> VoiceRequestAsync(Stream voiceStream, RequestExtras requestExtras = null)
        {
            var request = new AIRequest();
            request.Language = config.Language.code;
            request.Timezone = TimeZoneInfo.Local.StandardName;
            request.SessionId = sessionId;

            if (requestExtras != null)
            {
                if (requestExtras.HasContexts)
                {
                    request.Contexts = requestExtras.Contexts;
                }

                if (requestExtras.HasEntities)
                {
                    request.Entities = requestExtras.Entities;
                }
            }

            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(config.RequestUrl);
                httpRequest.Method = "POST";
                httpRequest.Accept = "application/json";

                httpRequest.Headers["Authorization"] = "Bearer " + config.ClientAccessToken;

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, jsonSettings);

                if (config.DebugLog)
                {
                    Debug.WriteLine("Request: " + jsonRequest);
                }

                var multipartClient = new MultipartHttpClient(httpRequest);
                multipartClient.Connect();

                multipartClient.AddStringPart("request", jsonRequest);
                multipartClient.AddFilePart("voiceData", "voice.wav", voiceStream);

                multipartClient.Finish();

                var responseJsonString = await multipartClient.GetResponse();

                if (config.DebugLog)
                {
                    Debug.WriteLine("Response: " + responseJsonString);
                }

                var aiResponse = JsonConvert.DeserializeObject<AIResponse>(responseJsonString);

                CheckForErrors(aiResponse);

                return aiResponse;

            }
            catch (Exception e)
            {
                throw new AIServiceException(e);
            }
        }

        static void CheckForErrors(AIResponse aiResponse)
        {
            if (aiResponse == null)
            {
                throw new AIServiceException("API.AI response parsed as null. Check debug log for details.");
            }

            if (aiResponse.IsError)
            {
                throw new AIServiceException(aiResponse);
            }
        }
    }
}
