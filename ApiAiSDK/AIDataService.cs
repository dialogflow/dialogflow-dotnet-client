//
// API.AI .NET SDK - client-side libraries for API.AI
// =================================================
//
// Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
// https://www.api.ai
//
// ***********************************************************************************************************************
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
// an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
//
// ***********************************************************************************************************************

using System;
using System.IO;
using Newtonsoft.Json;
using ApiAiSDK.Model;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiAiSDK
{
    public class AIDataService
    {
        private readonly AIConfiguration config;

        public string SessionId { get; }

        private readonly HttpClient httpClient;

        public AIDataService(AIConfiguration config)
        {
            this.config = config;

            if (string.IsNullOrEmpty(config.SessionId))
            {
                SessionId = Guid.NewGuid().ToString();
            }
            else
            {
                SessionId = config.SessionId;
            }

            httpClient = new HttpClient();
        }

        public async Task<AIResponse> RequestAsync(AIRequest request)
        {
            request.Language = config.Language.code;
            request.Timezone = TimeZoneInfo.Local.StandardName;
            request.SessionId = SessionId;
            
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, config.RequestUrl);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						
                httpRequest.Headers.Add("Authorization", "Bearer " + config.ClientAccessToken);
                		
                var jsonSettings = new JsonSerializerSettings
                { 
                    NullValueHandling = NullValueHandling.Ignore
                };

			
                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, jsonSettings);
                httpRequest.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                if (config.DebugLog)
                {
                    Debug.WriteLine("Request: " + jsonRequest);
                }

                var httpResponse = await httpClient.SendAsync(httpRequest);

                var result = await httpResponse.Content.ReadAsStringAsync();

                var aiResponse = JsonConvert.DeserializeObject<AIResponse>(result);

                CheckForErrors(aiResponse);

                return aiResponse;

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
            request.SessionId = SessionId;

            if (requestExtras != null)
            {
                requestExtras.CopyTo(request);
            }

            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, config.RequestUrl);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequest.Headers.Add("Content-Type", "application/json");

                httpRequest.Headers.Add("Authorization", "Bearer " + config.ClientAccessToken);
                
                var jsonSettings = new JsonSerializerSettings
                { 
                    NullValueHandling = NullValueHandling.Ignore
                };
				
                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, jsonSettings);

                if (config.DebugLog)
                {
                    Debug.WriteLine("Request: " + jsonRequest);
                }

                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(jsonRequest), "request");
                    var voicePart = new StreamContent(voiceStream);
                    voicePart.Headers.ContentDisposition = new ContentDispositionHeaderValue("voiceData")
                    {
                        FileName = "voice.wav"
                    };
                    content.Add(voicePart);

                    httpRequest.Content = content;

                    var response = await httpClient.SendAsync(httpRequest);

                    var responseJsonString = await response.Content.ReadAsStringAsync();

                    if (config.DebugLog)
                    {
                        Debug.WriteLine("Response: " + responseJsonString);
                    }

                    var aiResponse = JsonConvert.DeserializeObject<AIResponse>(responseJsonString);

                    CheckForErrors(aiResponse);

                    return aiResponse;
                }
            }
            catch (Exception e)
            {
                throw new AIServiceException(e);
            }
        }

        public async Task<bool> ResetContextsAsync()
        {
            var cleanRequest = new AIRequest("empty_query_for_resetting_contexts");
            cleanRequest.ResetContexts = true;
            try {
                var response = await RequestAsync(cleanRequest);
                return !response.IsError;
            } catch (AIServiceException e) {
                Debug.WriteLine("Exception while contexts clean." + e);
                return false;
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
