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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using ApiAiSDK.Model;
using ApiAiSDK.Util;

namespace ApiAiSDK
{
    public abstract class AIService : IDisposable
    {
        protected readonly AIDataService dataService;
        protected readonly AIConfiguration config;

        /// <summary>
        /// Unique SessionId. Normally should not be changed.
        /// </summary>
        public string SessionId
        {
            get { return dataService.SessionId; }
            set { dataService.SessionId = value; }
        }

        public static AIService CreateService(AIConfiguration config)
        {
            return new SystemRecognitionService(config);
        }

        public static async Task InstallVoiceCommands(StorageFile file)
        {
            await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(file);
        }

        protected AIService(AIConfiguration config)
        {
            this.config = config;
            dataService = new AIDataService(config);
        }

        public async Task ProcessOnActivated(IActivatedEventArgs e)
        {
            string callParameter;

            var commandArgs = e as VoiceCommandActivatedEventArgs;
            if (commandArgs != null)
            {
                // if program activated with voice command 
                // we make request to api.ai to activate context
                callParameter = commandArgs.Result?.Text;
                if (callParameter != null)
                {
                    await TextRequestAsync(callParameter);
                }
            }
            else
            {
                // if program activated with protocol 
                // we should restore SessionId, stored from 
                // VoiceCommandService

                // TODO
                var protocolArgs = e as ProtocolActivatedEventArgs;
                callParameter = protocolArgs?.Uri?.Query;

                if (!string.IsNullOrEmpty(callParameter))
                {
                    callParameter = callParameter.Substring("?LaunchContext=".Length);
                    callParameter = Uri.UnescapeDataString(callParameter);
                }
            }
        }

        public event Action OnListeningStarted;
        public event Action OnListeningStopped;
        
        /// <summary>
        /// Initialize service, call this method before any requests
        /// </summary>
        /// <returns></returns>
        public abstract Task InitializeAsync();

        /// <summary>
        /// Start listening
        /// </summary>
        /// <returns></returns>
        public abstract Task<AIResponse> StartRecognitionAsync(RequestExtras requestExtras = null);

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
                throw new ArgumentNullException(nameof(text));
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

        protected virtual void FireOnListeningStarted()
        {
            OnListeningStarted.InvokeSafely();
        }

        protected virtual void FireOnListeningStopped()
        {
            OnListeningStopped.InvokeSafely();
        }

        /// <summary>
        /// Dispose all used resources. Don't use the object after calling the method.
        /// </summary>
        public virtual void Dispose()
        {
            
        }
    }
}
