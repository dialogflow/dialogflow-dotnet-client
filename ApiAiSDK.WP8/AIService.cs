//
//  API.AI SDK - client-side libraries for API.AI
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using ApiAiSDK.Util;

namespace ApiAiSDK
{
    public abstract class AIService
    {
        public static AIService CreateService(AIConfiguration config)
        {
            return new SystemRecognitionService(config);
        }

        /// <summary>
        /// Still under development....
        /// </summary>
        protected virtual event Action<float> AudioLevelChanged;

        /// <summary>
        /// Event fired on the success processing result received from server
        /// </summary>
        public virtual event Action<AIResponse> OnResult;

        /// <summary>
        /// Event will fire if an error appears
        /// </summary>
        public virtual event Action<AIServiceException> OnError;

        /// <summary>
        /// Initialize service, call this method before any requests
        /// </summary>
        /// <returns></returns>
        public abstract Task InitializeAsync();
        
        /// <summary>
        /// Start listening
        /// </summary>
        /// <returns></returns>
        public abstract Task StartRecognitionAsync(RequestExtras requestExtras = null);
        
        /// <summary>
        /// Cancel all listening and request processes
        /// </summary>
        public abstract void Cancel();

        /// <summary>
        /// Send simple text request. This method does not use OnResult and OnSuccess callbacks for simplicity
        /// </summary>
        /// <param name="text">Request in text form</param>
        /// <param name="requestExtras">Optional request parameters such as Entities and Contexts</param>
        /// <returns>Server response</returns>
        /// <exception cref="ArgumentNullException">If text null or empty</exception>
        /// <exception cref="AIServiceException">If any error appears while request</exception>
        public async Task<AIResponse> TextRequestAsync(string text, RequestExtras requestExtras = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            return await TextRequestAsync(new AIRequest(text, requestExtras));
        }

        /// <summary>
        /// Send simple text request. This method does not use OnResult and OnSuccess callbacks because for simplicity
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Server response</returns>
        /// <exception cref="ArgumentNullException">If text null or empty</exception>
        /// <exception cref="AIServiceException">If any error appears while request</exception>
        public abstract Task<AIResponse> TextRequestAsync(AIRequest request);

        protected virtual void FireOnResult(AIResponse response)
        {
            OnResult.InvokeSafely(response);
        }

        protected virtual void FireOnError(AIServiceException aiException)
        {
            OnError.InvokeSafely(aiException);
        }

        protected virtual void OnAudioLevelChanged(float level)
        {
            AudioLevelChanged.InvokeSafely(level);
        }
    }
}
