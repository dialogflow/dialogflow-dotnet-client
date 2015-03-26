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

namespace ApiAiSDK.Util
{
    public class VoiceActivityDetector
    {
        private readonly int sampleRate;

        private double averageNoiseEnergy = 0.0;
        private double lastActiveTime = -1.0;
        private double lastSequenceTime = 0.0;
        private int sequenceCounter = 0;
        private double time = 0.0;

        private readonly double sequenceLengthMilis = 100.0;
        private readonly int minSpeechSequenceCount = 3;

        private const double energyFactor = 1.1;

        private const double maxSilenceLengthMilis = 0.35 * 1000;
        private const double minSilenceLengthMilis = 0.08 * 1000;

        private double silenceLengthMilis = maxSilenceLengthMilis;

        private bool speechActive = false;
        private const int startNoiseInterval = 150;

        public bool Enabled { get; set; }

        public event Action SpeechBegin;
        public event Action SpeechEnd;
        public event Action<float> AudioLevelChange;

        public VoiceActivityDetector(int sampleRate)
        {
            this.sampleRate = sampleRate;
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
                            OnSpeechBegin();
                        }

                        speechActive = true;

                        //Log.d(TAG, "LAST SPEECH " + time);
                        lastSequenceTime = time;
                        silenceLengthMilis = Math.Max(minSilenceLengthMilis, silenceLengthMilis - (maxSilenceLengthMilis - minSilenceLengthMilis) / 4);
                        //Log.d(TAG, "SM:" + silenceLengthMilis);

                    }
                } else {
                    sequenceCounter = 1;
                }
                lastActiveTime = time;
            } else {
                if (time - lastSequenceTime > silenceLengthMilis) {
                    if (lastSequenceTime > 0) {
                        //Log.d(TAG, "TERMINATE: " + time);
                        if (speechActive) {
                            speechActive = false;
                            OnSpeechEnd();
                        }

                    } else {
                        //Log.d(TAG, "NOSPEECH: " + time);
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
            var shorts = frame.ShortArray;

            for (int i = 0; i < frameSize; i++) {
                var amplitudeValue = shorts[i];
                energy += amplitudeValue * amplitudeValue / frameSize;

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

            OnAudiLevelChange(Convert.ToSingle(Math.Sqrt(energy / frameSize) / 10 /* normalization value */));

            var result = false;
            if (time < startNoiseInterval) {
                averageNoiseEnergy = (averageNoiseEnergy + energy) / 2.0;
            } else {
                int minCZ = (int) (frameSize * (1 / 3.0));
                int maxCZ = (int) (frameSize * (3 / 4.0));

                if (czCount >= minCZ && czCount <= maxCZ) {
                    if (energy > averageNoiseEnergy * energyFactor) {
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
        }

        protected void OnSpeechBegin()
        {
            SpeechBegin.InvokeSafely();
        }

        protected void OnSpeechEnd()
        {
            if (Enabled)
            {
                SpeechEnd.InvokeSafely();    
            }
        }

        protected void OnAudiLevelChange(float level)
        {
            AudioLevelChange.InvokeSafely(level);
        }
    }
}

