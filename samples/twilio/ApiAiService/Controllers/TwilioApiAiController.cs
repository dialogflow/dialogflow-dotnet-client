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
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using ApiAiSDK;
using ApiAiService.Models;
using Microsoft.WindowsAzure;
using Twilio.TwiML;

namespace ApiAiService.Controllers
{
    public class TwilioApiAiController : ApiController
    {
        private readonly ApiAi apiAi;

        public TwilioApiAiController()
        {
            try
            {
                var aiConfiguration = new AIConfiguration(
                    CloudConfigurationManager.GetSetting("SubscriptionKey"), 
                    CloudConfigurationManager.GetSetting("AccessToken"), 
                    SupportedLanguage.English);

                apiAi = new ApiAi(aiConfiguration);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());   
            }
        }

        public HttpResponseMessage Get()
        {
            var apiAiResponse = apiAi.TextRequest("hello");

            var twilioResponse = new TwilioResponse();
            twilioResponse.Sms(apiAiResponse.Result.Fulfillment.Speech);

            return new HttpResponseMessage()
            {
                Content = new StringContent(twilioResponse.ToXDocument().ToString(), Encoding.UTF8, "application/xml")
            };

        }

        [HttpPost]
        [ActionName("sms")]
        public HttpResponseMessage Sms(TwilioQuery query)
        {
            var textQuery = query.Body;

            if (string.IsNullOrEmpty(textQuery))
            {
                throw new ArgumentNullException("Body");
            }

            try
            {
                var apiAiResponse = apiAi.TextRequest(textQuery);

                var twilioResponse = new TwilioResponse();

                if (apiAiResponse != null)
                {
                    if (apiAiResponse.IsError)
                    {
                        twilioResponse.Sms(apiAiResponse.Status.ErrorDetails);
                    }
                    else
                    {
                        if (apiAiResponse.Result != null && apiAiResponse.Result.Fulfillment != null)
                        {
                            twilioResponse.Sms(apiAiResponse.Result.Fulfillment.Speech);
                        }
                    }
                }
                else
                {
                    twilioResponse.Sms("Empty response from ApiAi");
                }

                return new HttpResponseMessage()
                {
                    Content = new StringContent(twilioResponse.ToXDocument().ToString(), Encoding.UTF8, "application/xml")
                };

            }
            catch (AIServiceException aiServiceException)
            {
                Trace.TraceError(aiServiceException.ToString());
                var contentResult = new StringContent(aiServiceException.ToString());
                return new HttpResponseMessage()
                {
                    Content = contentResult
                };
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                var contentResult = new StringContent(e.ToString());
                return new HttpResponseMessage()
                {
                    Content = contentResult
                };
            }

            
        }
    }
}
