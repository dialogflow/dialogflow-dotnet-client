using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using ApiAiSDK.Model;
using ApiAiSDK.Util;

namespace ApiAiSDK
{
    public abstract class AIService
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

        /// <summary>
        /// Event fired on the success processing result received from server
        /// </summary>
        public event Action<AIResponse> OnResult;

        /// <summary>
        /// Event will fire if an error appears
        /// </summary>
        public event Action<AIServiceException> OnError;

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

        protected virtual void FireOnResult(AIResponse response)
        {
            OnResult.InvokeSafely(response);
        }

        protected virtual void FireOnError(AIServiceException aiException)
        {
            OnError.InvokeSafely(aiException);
        }

        protected virtual void FireOnListeningStarted()
        {
            OnListeningStarted.InvokeSafely();
        }

        protected virtual void FireOnListeningStopped()
        {
            OnListeningStopped.InvokeSafely();
        }
    }
}
