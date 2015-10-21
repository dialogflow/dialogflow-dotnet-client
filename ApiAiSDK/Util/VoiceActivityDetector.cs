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
using System.Diagnostics;
using System.Text;

namespace ApiAiSDK.Util
{
    public class VoiceActivityDetector
    {
        private readonly string TAG = typeof(VoiceActivityDetector).Name;

        private readonly int sampleRate;

        /// <summary>
        /// 160 samples * 2 bytes per sample
        /// </summary>
        private const int REQUIRED_BUFFER_SIZE = 320; 

        private const double frameLengthMillis = REQUIRED_BUFFER_SIZE / 2 / 16;

        private const int minCZ = (int)(5 * frameLengthMillis / 10);
        private const int maxCZ = minCZ * 3;

        private double averageNoiseEnergy = 0.0;
        private double lastActiveTime = -1.0;
        private double lastSequenceTime = 0.0;
        private int sequenceCounter = 0;
        private double time = 0.0;

        private readonly double sequenceLengthMilis = 30.0;
        private readonly int minSpeechSequenceCount = 3;

        private const double energyFactor = 3.1;

        private const double maxSilenceLengthMilis = 3.5 * 1000;
        private const double minSilenceLengthMilis = 0.8 * 1000;

        private double silenceLengthMilis = maxSilenceLengthMilis;

        private bool speechActive = false;
        private const int startNoiseInterval = 150;

        public bool Enabled { get; set; }

        public event Action SpeechBegin;
        public event Action SpeechEnd;
        public event Action SpeechNotDetected;
        public event Action<float> AudioLevelChange;

        public double SpeechBeginTime { get; private set; }
        public double SpeechEndTime { get; private set; }

        private readonly byte[] bufferToProcess = new byte[REQUIRED_BUFFER_SIZE];
        private byte[] tempBuffer = null;

        public VoiceActivityDetector(int sampleRate)
        {
            this.sampleRate = sampleRate;
            Enabled = true; // default value
        }

        public void ProcessBufferEx(byte[] buffer, int bytesRead)
        {
            if (bytesRead > 0)
            {
                byte[] sourceBuffer;

                if (tempBuffer == null)
                {
                    sourceBuffer = buffer;
                }
                else
                {
                    sourceBuffer = new byte[tempBuffer.Length + buffer.Length];
                    Array.Copy(tempBuffer, 0, sourceBuffer, 0, tempBuffer.Length);
                    Array.Copy(buffer, 0, sourceBuffer, tempBuffer.Length, bytesRead);
                }

                var currentIndex = 0;
                while (currentIndex + REQUIRED_BUFFER_SIZE < sourceBuffer.Length)
                {
                    Array.Copy(sourceBuffer, currentIndex, bufferToProcess, 0, REQUIRED_BUFFER_SIZE);
                    ProcessBuffer(bufferToProcess, bufferToProcess.Length);
                    currentIndex += REQUIRED_BUFFER_SIZE;
                }

                var bytesToSave = sourceBuffer.Length - currentIndex;

                if (bytesToSave > 0)
                {
                    tempBuffer = new byte[bytesToSave];
                    Array.Copy(sourceBuffer, currentIndex, tempBuffer, 0, bytesToSave);    
                }
                else
                {
                    tempBuffer = null;
                }
            }       
        }

        public void ProcessBuffer(byte[] buffer, int bytesRead) {
            
            var byteBuffer = new ByteBuffer(buffer, bytesRead);
            var active = IsFrameActive(byteBuffer);

            int frameSize = bytesRead / 2; // 16 bit encoding
            time = time + (frameSize * 1000) / sampleRate; // because of sampleRate given for seconds

            if (active) {
                if (lastActiveTime >= 0 &&
                    time - lastActiveTime < sequenceLengthMilis) {

                    sequenceCounter++;

                    if (sequenceCounter >= minSpeechSequenceCount) {

                        if (!speechActive) {
                            Debug.WriteLine("SPEECH BEGIN " + time);
                            OnSpeechBegin();

                            speechActive = true;
                        }

                        lastSequenceTime = time;
                        silenceLengthMilis = Math.Max(minSilenceLengthMilis, silenceLengthMilis - (maxSilenceLengthMilis - minSilenceLengthMilis) / 4);
                    }
                } else {
                    sequenceCounter = 1;
                }
                lastActiveTime = time;
            } else {
                if (time - lastSequenceTime > silenceLengthMilis) {
                    if (lastSequenceTime > 0) {
                        Debug.WriteLine("SPEECH END: " + time);
                        if (speechActive) {
                            speechActive = false;
                            OnSpeechEnd();
                        }
                    } else {
                        Debug.WriteLine("NOSPEECH: " + time);
                        OnSpeechNotDetected();
                    }
                }
            }

        }

        private bool IsFrameActive(ByteBuffer frame) 
        {
            var lastSign = 0;
            var czCount = 0;
            var energy = 0.0;

            var frameSize = frame.ShortArrayLength;
            short[] shorts = frame.ShortArray;

            for (int i = 0; i < frameSize; i++) {
                var amplitudeValue = shorts[i] / (float)short.MaxValue;

                energy += amplitudeValue * amplitudeValue / (float)frameSize;

                int sign;

                if (amplitudeValue > 0) {
                    sign = 1;
                } else {
                    sign = -1;
                }

                if (lastSign != 0 && sign != lastSign) {
                    czCount += 1;
                }
                lastSign = sign;
            }

            if (Math.Abs(time % 100) < 1) // level feedback every 100 ms
            {
                var audioLevel = Math.Sqrt(energy) * 3;
                if (audioLevel > 1)
                {
                    audioLevel = 1;
                }
                OnAudioLevelChange(Convert.ToSingle(audioLevel));    
            }


            var result = false;
            if (time < startNoiseInterval) {
                averageNoiseEnergy = averageNoiseEnergy + energy / (startNoiseInterval/frameLengthMillis);
            } else {
                if (czCount >= minCZ && czCount <= maxCZ) {
                    if (energy > Math.Max(averageNoiseEnergy, 0.001818) * energyFactor) {
                        result = true;
                    }
                }
            }

            return result;

        }

        public void Reset() {
            time = 0.0;

            averageNoiseEnergy = 0.0;
            lastActiveTime = -1.0;
            lastSequenceTime = 0.0;
            sequenceCounter = 0;
            silenceLengthMilis = maxSilenceLengthMilis;

            speechActive = false;

            SpeechBeginTime = 0;
            SpeechEndTime = 0;

            tempBuffer = null;
        }

        protected void OnSpeechBegin()
        {
            SpeechBeginTime = time;
            SpeechBegin.InvokeSafely();
        }

        protected void OnSpeechEnd()
        {
            SpeechEndTime = time;
            if (Enabled)
            {
                SpeechEnd.InvokeSafely();    
            }
        }

        protected void OnSpeechNotDetected()
        {
            SpeechNotDetected?.Invoke();
        }

        protected void OnAudioLevelChange(float level)
        {
            AudioLevelChange.InvokeSafely(level);
        }
    }
}

