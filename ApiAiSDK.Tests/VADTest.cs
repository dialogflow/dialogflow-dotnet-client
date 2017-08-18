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

        bool speechEnd;
        bool speechBegin;
        bool speechNotDetected;

        [Test]
        public void TestSpeechDetect(){
            var vad = new VoiceActivityDetector(SAMPLE_RATE);

            var inputStream = ReadFileFromResource("speech.raw");

            speechEnd = false;
            speechBegin = false;
            speechNotDetected = false;

            vad.SpeechBegin += () => {speechBegin = true; };
            vad.SpeechEnd += () => {speechEnd = true; };
            vad.SpeechNotDetected += () => {speechNotDetected = true; };

            ProcessStream(vad, inputStream);

            Assert.IsTrue(speechBegin);
            Assert.IsTrue(speechEnd);
            Assert.IsFalse(speechNotDetected);

            Assert.IsTrue(vad.SpeechBeginTime > 800 && vad.SpeechBeginTime < 950, "vad.SpeechBeginTime " + vad.SpeechBeginTime);
            Assert.IsTrue(vad.SpeechEndTime > 2400 && vad.SpeechEndTime < 2900, "vad.SpeechEndTime " + vad.SpeechEndTime);
        }

        [Test]
        public void TestSilence() {
            var vad = new VoiceActivityDetector(SAMPLE_RATE);

            var inputStream = ReadFileFromResource("silence.raw");

            speechEnd = false;
            speechBegin = false;
            speechNotDetected = false;

            vad.SpeechBegin += () => {speechBegin = true; };
            vad.SpeechEnd += () => {speechEnd = true; };
            vad.SpeechNotDetected += () => {speechNotDetected = true; };

            ProcessStream(vad, inputStream);

            //Assert.IsFalse(speechBegin);
            Assert.IsFalse(speechEnd);
            Assert.IsTrue(speechNotDetected);
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

        [Test]
        public void TestFFT()
        {
            const int SIZE = 512;
            var re = new double[SIZE];
            var im = new double[SIZE];

            for (int i = 0; i < SIZE; i++)
            {
                re[i] = Math.Sin(2 * Math.PI / SIZE * i);
                im[i] = 0;
            }

            var fft = new FFT2();
            var logN = (uint)Math.Log(SIZE, 2);
            fft.init(logN);

            var expected = new double[SIZE];
            Array.Copy(re, expected, re.Length);

            fft.run(re, im);
            fft.run(re, im, true);

            for (int i = 0; i < SIZE; i++)
            {
                Assert.AreEqual(expected[i], re[i], 0.000000001);
            }
        }

        //[Test] Yet not implemented
        public void TestVAD2()
        {
            var vad = new VoiceActivityDetectorV2(SAMPLE_RATE);
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
            var bufferSize = 3000 + new Random().Next(200);
            var buffer = new byte[bufferSize];
            int bytesRead = 0;
            bytesRead = inputStream.Read(buffer, 0, bufferSize);
            while (bytesRead > 0)
            {
                vad.ProcessBufferEx(buffer, bytesRead);

                bufferSize = 3000 + new Random().Next(200);
                buffer = new byte[bufferSize];
                bytesRead = inputStream.Read(buffer, 0, bufferSize);
            }
        }

        static void ProcessStream(VoiceActivityDetectorV2 vad, Stream inputStream)
        {
            var bufferSize = 3200;
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

