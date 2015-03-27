//
//  API.AI .NET SDK tests - client-side libraries for API.AI
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
using System.IO;
using NUnit.Framework;
using ApiAiSDK.Model;
using ApiAiSDK.Util;
using System.Reflection;

namespace ApiAiSDK.Tests
{
    [TestFixture]
    public class VADTest
    {
        private const int SAMPLE_RATE = 16000;

        bool speechEnd = false;
        bool speechBegin = false;

        [Test]
        public void TestSpeechDetect(){
            var vad = new VoiceActivityDetector(SAMPLE_RATE);

            var inputStream = ReadFileFromResource("speech.raw");

            speechEnd = false;
            speechBegin = false;

            vad.SpeechBegin += () => {speechBegin = true; };
            vad.SpeechEnd += () => {speechEnd = true; };

            ProcessStream(vad, inputStream);

            Assert.IsTrue(speechBegin);
            Assert.IsTrue(speechEnd);
        }

        [Test]
        public void TestSilence() {
            var vad = new VoiceActivityDetector(SAMPLE_RATE);

            var inputStream = ReadFileFromResource("silence.raw");

            speechEnd = false;
            speechBegin = false;

            vad.SpeechBegin += () => {speechBegin = true; };
            vad.SpeechEnd += () => {speechEnd = true; };

            ProcessStream(vad, inputStream);

            Assert.IsFalse(speechEnd);

        }

        [Test]
        public void TestEnabled() {
            var vad = new VoiceActivityDetector(SAMPLE_RATE);
            vad.Enabled = false;

            var inputStream = ReadFileFromResource("speech.raw");

            speechEnd = false;
            speechBegin = false;

            vad.SpeechBegin += () => {speechBegin = true; };
            vad.SpeechEnd += () => {speechEnd = true; };

            ProcessStream(vad, inputStream);

            Assert.IsTrue(speechBegin);
            Assert.IsFalse(speechEnd);
        }

        static void ProcessStream(VoiceActivityDetector vad, Stream inputStream)
        {
            var bufferSize = 1096;
            var buffer = new byte[bufferSize];
            int bytesRead = 0;
            bytesRead = inputStream.Read(buffer, 0, bufferSize);
            while (bytesRead > 0)
            {
                vad.ProcessBuffer(buffer, bytesRead);
                bytesRead = inputStream.Read(buffer, 0, bufferSize);
            }
        }

        private Stream ReadFileFromResource(string resourceId)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            Stream stream = a.GetManifestResourceStream("ApiAiSDK.Tests.TestData." + resourceId);
            return stream;
        }

    }
}

