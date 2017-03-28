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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.Media.SpeechSynthesis;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using ApiAiSDK;
using ApiAiSDK.Model;

namespace ApiAiDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage 
    {
        private SpeechSynthesizer speechSynthesizer;

        private AIService AIService => (Application.Current as App)?.AIService;

        private volatile bool recognitionActive = false;

        public MainPage()
        {
            InitializeComponent();

            InitializeSynthesizer();

            mediaElement.MediaEnded += MediaElement_MediaEnded;

        }
        
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MediaElement_MediaEnded");
            Listen_Click(listenButton, null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SetupTheme();

            var response = e.Parameter as AIResponse;
            if (response != null)
            {
                var aiResponse = response;
                OutputJson(aiResponse);
                OutputParams(aiResponse);
            }
            
            InitializeRecognizer();

            AIService.OnListeningStarted += AIService_OnListeningStarted;
            AIService.OnListeningStopped += AIService_OnListeningStopped;
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
            AIService.OnListeningStarted -= AIService_OnListeningStarted;
            AIService.OnListeningStopped -= AIService_OnListeningStopped;

            speechSynthesizer?.Dispose();
            speechSynthesizer = null;
        }

        private void AIService_OnListeningStopped()
        {
            RunInUIThread(() => listenButton.Content = "Processing...");
        }

        private void AIService_OnListeningStarted()
        {
            RunInUIThread(() => listenButton.Content = "Listening...");
        }

        private async void ProcessResult(AIResponse aiResponse)
        {
            RunInUIThread(() =>
            {
                listenButton.Content = "Listen";
                OutputJson(aiResponse);
                OutputParams(aiResponse);
            });
            
            var speechText = aiResponse.Result?.Fulfillment?.Speech;
            if (!string.IsNullOrEmpty(speechText))
            {
                var speechStream = await speechSynthesizer.SynthesizeTextToStreamAsync(speechText);
                mediaElement.SetSource(speechStream, speechStream.ContentType);
                mediaElement.Play();
            }
        }
   
        private async Task InitializeRecognizer()
        {
            await AIService.InitializeAsync();
            listenButton.IsEnabled = true;
        }

        private void InitializeSynthesizer()
        {
            speechSynthesizer = new SpeechSynthesizer();
        }

        private async void Listen_Click(object sender, RoutedEventArgs e)
        {

            if(mediaElement.CurrentState == MediaElementState.Playing)
            {
                mediaElement.Stop();
            }

            try
            {
                if (!recognitionActive)
                {
                    recognitionActive = true;
                    var aiResponse = await AIService.StartRecognitionAsync();
                    recognitionActive = false;

                    if (aiResponse != null)
                    {
                        ProcessResult(aiResponse);
                    }
                }
                else
                {
                    AIService.Cancel();
                }
            }
            catch (OperationCanceledException)
            {
                recognitionActive = false;
                resultTextBlock.Text = "Cancelled";
            }
            catch (Exception ex)
            {
                recognitionActive = false;
                Debug.WriteLine(ex.ToString());
                resultTextBlock.Text = $"Empty or error result: {Environment.NewLine}{ex}";
            }
            finally
            {
                listenButton.Content = "Listen";
            }
            
        }

        private void OutputParams(AIResponse aiResponse)
        {
            var contextsParams = new Dictionary<string,string>();

            if (aiResponse.Result?.Contexts != null)
            {
                foreach (var context in aiResponse.Result?.Contexts)
                {
                    if (context.Parameters != null)
                    {
                        foreach (var parameter in context.Parameters)
                        {
                            if (!contextsParams.ContainsKey(parameter.Key))
                            {
                                contextsParams.Add(parameter.Key, parameter.Value);
                            }
                        }
                    }
                }
            }

            var resultBuilder = new StringBuilder();
            foreach (var contextsParam in contextsParams)
            {
                resultBuilder.AppendLine(contextsParam.Key + ": " + contextsParam.Value);
            }

            parametersTextBlock.Text = resultBuilder.ToString();
        }

        private void OutputJson(AIResponse aiResponse)
        {
            resultTextBlock.Text = JsonConvert.SerializeObject(aiResponse, Formatting.Indented);
        }

        private async void InstallVoiceCommands_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommands.xml"));
                await AIService.InstallVoiceCommands(storageFile);

                parametersTextBlock.Text = "Voice commands installed";
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                resultTextBlock.Text = ex.ToString();
            }
        }

        private async void UninstallCommands_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///UninstallCommands.xml"));
                await AIService.InstallVoiceCommands(storageFile);

                parametersTextBlock.Text = "Voice commands uninstalled";

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                resultTextBlock.Text = ex.ToString();
            }
        }

        private void JsonButton_Click(object sender, RoutedEventArgs e)
        {
            jsonContaner.Visibility = jsonContaner.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void SetupTheme()
        {
            var appView = ApplicationView.GetForCurrentView();
            var titleBar = appView.TitleBar;
            titleBar.BackgroundColor = Color.FromArgb(255, 43, 48, 62);
            titleBar.InactiveBackgroundColor = Color.FromArgb(255, 43, 48, 62);
            titleBar.ButtonBackgroundColor = Color.FromArgb(255, 43, 48, 62);
            titleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 43, 48, 62);
            titleBar.ForegroundColor = Color.FromArgb(255, 247, 255, 255);
        }

        private async void RunInUIThread(Action a)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => a());
        }
    }
}
