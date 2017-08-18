/**
 * Copyright 2017 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using ApiAiSDK.Model;
using Newtonsoft.Json;

namespace ApiAiSDK
{
    /// <summary>
    /// Class for making simple api.ai requests and performing utility operations. Doesn't use any UI.
    /// </summary>
    public class ApiAi : ApiAiBase
    {
        private AIConfiguration config;
        public AIDataService DataService { get; }
        
        public ApiAi(AIConfiguration config)
        {
            this.config = config;

            DataService = new AIDataService(this.config);
        }

        public async Task<AIResponse> TextRequestAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            return await TextRequestAsync(new AIRequest(text));
        }

        public async Task<AIResponse> TextRequestAsync(string text, RequestExtras requestExtras)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            return await TextRequestAsync(new AIRequest(text, requestExtras));
        }

        public async Task<AIResponse> TextRequestAsync(AIRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return await DataService.RequestAsync(request);
        }

        public async Task<AIResponse> VoiceRequestAsync(Stream voiceStream, RequestExtras requestExtras = null)
        {
            return await DataService.VoiceRequestAsync(voiceStream, requestExtras);
        }

        public async Task<AIResponse> VoiceRequest(float[] samples)
        {
            try
            {
                var trimmedSamples = TrimSilence(samples);

                if (trimmedSamples != null)
                {

                    var pcm16 = ConvertIeeeToPcm16(trimmedSamples);
                    var bytes = ConvertArrayShortToBytes(pcm16);

                    var voiceStream = new MemoryStream(bytes);
                    voiceStream.Seek(0, SeekOrigin.Begin);

                    var aiResponse = VoiceRequestAsync(voiceStream);
                    return await aiResponse;
                }

            }
            catch (AIServiceException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AIServiceException(e);
            }

            return null;
        }

        /// <summary>
        /// Make Cortana to speech api.ai response.
        /// </summary>
        /// <param name="voiceServiceConnection"></param>
        /// <param name="aiResponse"></param>
        /// <returns></returns>
        public async Task SendResponseToCortanaAsync(VoiceCommandServiceConnection voiceServiceConnection, AIResponse aiResponse)
        {
            var textResponse = aiResponse.Result.Fulfillment?.Speech ?? string.Empty;
            var userMessage = new VoiceCommandUserMessage
            {
                DisplayMessage = textResponse,
                SpokenMessage = textResponse
            };

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            // Cortana will present a “Go to app_name” link that the user 
            // can tap to launch the app. 
            // Pass in a launch to enable the app to deep link to a page 
            // relevant to the voice command.
            //response.AppLaunchArgument =
            //  string.Format("destination={0}”, “Las Vegas");

            await voiceServiceConnection.ReportSuccessAsync(response);
        }

        /// <summary>
        /// Launch app and pass the appropriate parameters to it
        /// </summary>
        /// <param name="voiceServiceConnection"></param>
        /// <param name="aiResponse"></param>
        /// <returns></returns>
        public async Task LaunchAppInForegroundAsync(VoiceCommandServiceConnection voiceServiceConnection, AIResponse aiResponse)
        {
            var textMessage = aiResponse?.Result?.Fulfillment?.Speech ?? string.Empty;

            var userMessage = new VoiceCommandUserMessage
            {
                SpokenMessage = textMessage,
                DisplayMessage = textMessage
            };

            var response = VoiceCommandResponse.CreateResponse(userMessage);
            response.AppLaunchArgument = JsonConvert.SerializeObject(aiResponse, Formatting.Indented);
            
            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }
    }
}
