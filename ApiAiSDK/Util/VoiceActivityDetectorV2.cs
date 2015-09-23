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
using ApiAiSDK.Util;

namespace ApiAiSDK
{
    /// <summary>
    /// Voice activity detector v2. Based on http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.176.6740&rep=rep1&type=pdf
    /// </summary>
    public class VoiceActivityDetectorV2
    {
        int sampleRate;
        const int frameSize = 10;

        const int startSilenceFramesCount = 30;

        // frame size in ms
        readonly int samplesInFrame;

        int silenceFramesCount = 0;

        int energyPrimThresh = 40;
        int fPrimTresh = 185;
        int sfPrimTresh = 5;

        int frameNumber;

        double minE;
        double minF;
        double minSF;

        public bool Enabled { get; set; }

        public event Action SpeechBegin;
        public event Action SpeechEnd;
        public event Action<float> AudioLevelChange;


        public VoiceActivityDetectorV2(int sampleRate)
        {
            this.sampleRate = sampleRate;

            samplesInFrame = sampleRate / (1000 / frameSize);

            Enabled = true; // default value
        }


        public void ProcessBuffer(byte[] buffer, int bytesRead)
        {
            var byteBuffer = new ByteBuffer(buffer, bytesRead);
            var shortBuffer = byteBuffer.ShortArray;
            int numOfFrames = byteBuffer.ShortArrayLength / samplesInFrame;

            for (int i = 0; i < numOfFrames; i++)
            {
                var frame = new short[samplesInFrame];
                Array.Copy(shortBuffer, samplesInFrame * i, frame, 0, samplesInFrame);
                ProcessFrame(frame);

            }
        }


        private void ProcessFrame(short[] frame)
        {
            frameNumber++;

            var frameEnergy = CalcFrameEnergy(frame);
            var frameSpectralFlatness = CalcSpectralFlatness(frame);
            var frameDominantFrequence = CalcDominantFrequence(frame);

            if (frameNumber <= startSilenceFramesCount)
            {
                minE = Math.Min(minE, frameEnergy);
                minSF = Math.Min(minSF, frameSpectralFlatness);
                minF = Math.Min(minF, frameDominantFrequence);

                return;
            }

            var threshE = energyPrimThresh * Math.Log10(minE);
            var threshF = fPrimTresh;
            var threshSF = sfPrimTresh;

            var counter = 0;

            if (frameEnergy - minE >= threshE)
            {
                counter++;
            }

            if (frameDominantFrequence - minF >= threshF)
            {
                counter++;
            }

            if (frameSpectralFlatness - minSF >= threshSF)
            {
                counter++;
            }

            if (counter > 1)
            {
                // speech
            }
            else
            {
                // silence

                silenceFramesCount++;

                minE = ((silenceFramesCount * minE) + frameEnergy) / (silenceFramesCount + 1);
                threshE = energyPrimThresh * Math.Log10(minE);
            }

        }

        private double CalcFrameEnergy(short[] frame)
        {
            var sum = 0.0;
            var n = frame.Length / 2;
            for (int i = 0; i < frame.Length; i++)
            {
                sum += Math.Pow(frame[i] * hammingWindow(n - i), 2);
            }
            return sum;
        }

        double CalcSpectralFlatness(short[] frame)
        {
            throw new NotImplementedException();
        }

        double CalcDominantFrequence(short[] frame)
        {
            throw new NotImplementedException();
        }

        private double hammingWindow(int n)
        {
            return 0.54 - 0.46 * Math.Cos((2 * Math.PI * n) / (samplesInFrame - 1));
        }

        private double rectWindow(int n)
        {
            return 1;
        }

    }
}

