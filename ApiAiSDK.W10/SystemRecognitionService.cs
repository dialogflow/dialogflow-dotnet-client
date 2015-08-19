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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using ApiAiSDK.Model;

namespace ApiAiSDK
{
    internal class SystemRecognitionService : AIService
    {
        private const uint HRESULT_PRIVACY_STATEMENT_DECLINED = 0x80045509;
        private const uint HRESULT_LANG_NOT_SUPPORTED = 0x800455BC;

        private volatile SpeechRecognizer speechRecognizer;
        private readonly object speechRecognizerLock = new object();
        
        private volatile IAsyncOperation<SpeechRecognitionResult> currentOperation;

        public SystemRecognitionService(AIConfiguration config) : base(config)
        {
            
        }

        public override async Task InitializeAsync()
        {
            if (speechRecognizer == null)
            {
                try
                {
                    var recognizer = new SpeechRecognizer(ConvertAILangToSystem(config.Language));
                    recognizer.StateChanged += Recognizer_StateChanged;

                    // INFO: Dictation is default Constraint
                    //var webSearchGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
                    //recognizer.Constraints.Add(webSearchGrammar);

                    await recognizer.CompileConstraintsAsync();

                    lock (speechRecognizerLock)
                    {
                        if (speechRecognizer == null)
                        {
                            speechRecognizer = recognizer;
                        }
                        else
                        {
                            recognizer.Dispose();
                        }
                    }
                }
                catch (Exception e)
                {
                    if ((uint)e.HResult == HRESULT_LANG_NOT_SUPPORTED)
                    {
                        throw new AIServiceException(string.Format("Specified language {0} not supported or not installed on device", config.Language.code), e);
                    }
                    throw;
                }

            }
        }

        private void Recognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine("SpeechRecognizer_StateChanged " + args.State);

            switch (args.State)
            {
                case SpeechRecognizerState.Idle:
                    break;
                case SpeechRecognizerState.Capturing:
                    FireOnListeningStarted();
                    break;
                case SpeechRecognizerState.Processing:
                    FireOnListeningStopped();
                    break;
                case SpeechRecognizerState.SoundStarted:
                    break;
                case SpeechRecognizerState.SoundEnded:
                    break;
                case SpeechRecognizerState.SpeechDetected:
                    break;
                case SpeechRecognizerState.Paused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Language ConvertAILangToSystem(SupportedLanguage language)
        {
            switch (language.code)
            {
                case "en":
                    return new Language("en-US");
                    break;
                case "ru":
                    return new Language("ru-RU");
                    break;
                case "de":
                    return new Language("de-DE");
                    break;
                case "pt":
                    return new Language("pt-PT");
                    break;
                case "pt-BR":
                    return new Language("pt-BR");
                    break;
                case "es":
                    return new Language("es-ES");
                    break;
                case "fr":
                    return new Language("fr-FR");
                    break;
                case "it":
                    return new Language("it-IT");
                    break;
                case "ja":
                    return new Language("ja-JP");
                    break;
                case "zh-CN":
                    return new Language("zh-CN");
                    break;
                case "zh-HK":
                    return new Language("zh-HK");
                    break;
                case "zh-TW":
                    return new Language("zh-TW");
                    break;
            }

            return new Language("en-US");
        }

        public override async Task StartRecognitionAsync(RequestExtras requestExtras = null)
        {
            if (currentOperation == null)
            {
                try
                {
                    var speechRecognitionResultTask = speechRecognizer.RecognizeAsync();
                    currentOperation = speechRecognitionResultTask;

                    var results = await speechRecognitionResultTask;
                    currentOperation = null;

                    switch (results.Status)
                    {
                        case SpeechRecognitionResultStatus.Success:
                            ProcessRecognitionResultsAsync(results, requestExtras);
                            break;
                        case SpeechRecognitionResultStatus.TopicLanguageNotSupported:
                            FireOnError(new AIServiceException("This language is not supported"));
                            break;
                        case SpeechRecognitionResultStatus.GrammarLanguageMismatch:
                            FireOnError(new AIServiceException("GrammarLanguageMismatch"));
                            break;
                        case SpeechRecognitionResultStatus.GrammarCompilationFailure:
                            FireOnError(new AIServiceException("GrammarCompilationFailure"));
                            break;
                        case SpeechRecognitionResultStatus.AudioQualityFailure:
                            FireOnError(new AIServiceException("AudioQualityFailure"));
                            break;
                        case SpeechRecognitionResultStatus.UserCanceled:
                            // do nothing
                            break;
                        case SpeechRecognitionResultStatus.Unknown:
                            FireOnError(new AIServiceException("Unknown recognition error"));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (TaskCanceledException)
                {
                    currentOperation = null;
                }
                catch (Exception e)
                {
                    currentOperation = null;

                    if ((uint) e.HResult == HRESULT_PRIVACY_STATEMENT_DECLINED)
                    {
                        throw new AIServiceException(
                            "You must accept privacy statement before using speech recognition.", e);
                    }
                    else
                    {
                        throw new AIServiceException("Exception while recognition", e);
                    }
                }
            }
        }


        public override void Cancel()
        {
            currentOperation?.Cancel();
        }

        public override async Task<AIResponse> TextRequestAsync(AIRequest request)
        {
            return await dataService.RequestAsync(request);
        }

        private async Task ProcessRecognitionResultsAsync(SpeechRecognitionResult results, RequestExtras requestExtras)
        {
            if (!string.IsNullOrWhiteSpace(results.Text))
            {
                var request = CreateAIRequest(results);

                try
                {
                    requestExtras?.CopyTo(request);

                    var response = await dataService.RequestAsync(request);
                    FireOnResult(response);
                }
                catch (Exception e)
                {
                    FireOnError(new AIServiceException(e));
                }
            }
        }
        
        private AIRequest CreateAIRequest(SpeechRecognitionResult recognitionResults)
        {
            var texts = new List<string> { recognitionResults.Text };
            var confidences = new List<float> { ConfidenceToFloat(recognitionResults.Confidence) };

            var aiRequest = new AIRequest();

            var alternates = recognitionResults.GetAlternates(5);
            if (alternates != null)
            {
                foreach (var a in alternates)
                {
                    texts.Add(a.Text);
                    confidences.Add(ConfidenceToFloat(a.Confidence));
                }
            }
            aiRequest.Query = texts.ToArray();
            aiRequest.Confidence = confidences.ToArray();
            return aiRequest;
        }

        private float ConfidenceToFloat(SpeechRecognitionConfidence confidence)
        {
            switch (confidence)
            {
                case SpeechRecognitionConfidence.High:
                    return 0.99f;
                case SpeechRecognitionConfidence.Medium:
                    return 0.6f;
                case SpeechRecognitionConfidence.Low:
                    return 0.3f;
                case SpeechRecognitionConfidence.Rejected:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(confidence), confidence, null);
            }
        }

    }
}
