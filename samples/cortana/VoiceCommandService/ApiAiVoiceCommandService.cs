//
// API.AI Cortana sample
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
using System.Threading.Tasks;

using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using ApiAiSDK;
using System.Diagnostics;

namespace ApiAiDemo.VoiceCommands
{
    public sealed class ApiAiVoiceCommandService : IBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        private VoiceCommandServiceConnection voiceServiceConnection;
        private ApiAi apiAi;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();
     
            taskInstance.Canceled += OnTaskCanceled;
            
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null)
            {

                var config = new AIConfiguration("cb9693af-85ce-4fbf-844a-5563722fc27f",
                           "40048a5740a1455c9737342154e86946",
                           SupportedLanguage.English);

                apiAi = new ApiAi(config);
                apiAi.DataService.PersistSessionId();
                
                try
                {
                    voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                    voiceServiceConnection.VoiceCommandCompleted += VoiceCommandCompleted;
                    var voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                    var recognizedText = voiceCommand.SpeechRecognitionResult?.Text;

                    switch (voiceCommand.CommandName)
                    {
                        case "type":
                            {
                                var aiResponse = await apiAi.TextRequestAsync(recognizedText);
                                await apiAi.LaunchAppInForegroundAsync(voiceServiceConnection, aiResponse);
                            }
                            break;
                        case "unknown":
                            {
                                if (!string.IsNullOrEmpty(recognizedText))
                                {
                                    var aiResponse = await apiAi.TextRequestAsync(recognizedText);
                                    if (aiResponse != null)
                                    {
                                        await apiAi.SendResponseToCortanaAsync(voiceServiceConnection, aiResponse);
                                    }
                                }
                            }
                            break;

                        case "greetings":
                            {
                                var aiResponse = await apiAi.TextRequestAsync(recognizedText);
                                
                                var repeatMessage = new VoiceCommandUserMessage
                                {
                                    DisplayMessage = "Repeat please",
                                    SpokenMessage = "Repeat please"
                                };

                                var processingMessage = new VoiceCommandUserMessage
                                {
                                    DisplayMessage = aiResponse?.Result?.Fulfillment?.Speech ?? "Pizza",
                                    SpokenMessage = ""
                                };

                                var resp = VoiceCommandResponse.CreateResponseForPrompt(processingMessage, repeatMessage);
                                await voiceServiceConnection.ReportSuccessAsync(resp);
                                break;
                            }

                        default:
                            if (!string.IsNullOrEmpty(recognizedText))
                            {
                                var aiResponse = await apiAi.TextRequestAsync(recognizedText);
                                if (aiResponse != null)
                                {
                                    await apiAi.SendResponseToCortanaAsync(voiceServiceConnection, aiResponse);
                                }
                            }
                            else
                            {
                                await SendResponse("Can't recognize");
                            }
                            
                            break;
                    }
                    
                }
                catch(Exception e)
                {
                    var message = e.ToString();
                    Debug.WriteLine(message);
                }
                finally
                {
                    serviceDeferral?.Complete();
                }
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            serviceDeferral?.Complete();            
        }

        private void VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            serviceDeferral?.Complete();
        }

        private async Task SendResponse(string textResponse)
        {
            var userMessage = new VoiceCommandUserMessage
            {
                DisplayMessage = textResponse,
                SpokenMessage = textResponse
            };

            var response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(response);
        }
    }
}
