using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using ApiAiSDK.Model;
using ApiAiSDK.Util;

namespace ApiAiSDK.W10
{
    public abstract class AIService
    {
        public static AIService CreateService(AIConfiguration config)
        {
            return new SystemRecognitionService(config);
        }

        public static async Task InstallVoiceCommands(StorageFile file)
        {
            await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(file);
        }

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
    }
}
