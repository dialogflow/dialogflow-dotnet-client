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
        private int sampleRate;
        private const int frameSize = 10;

        private const int startSilenceFramesCount = 30;

        // frame size in ms
        private readonly int samplesInFrame;

        private int silenceFramesCount = 0;

        private int energyPrimThresh = 40;
        private int fPrimTresh = 185;
        private int sfPrimTresh = 5;

        private int frameNumber;

        private double minE;
        private double minF;
        private double minSF;

        private FFT2 fft;

        public bool Enabled { get; set; }

        public event Action SpeechBegin;
        public event Action SpeechEnd;
        public event Action<float> AudioLevelChange;


        public VoiceActivityDetectorV2(int sampleRate)
        {
            this.sampleRate = sampleRate;

            samplesInFrame = sampleRate / (1000 / frameSize);

            fft = new FFT2();
            fft.init((uint)Math.Log(samplesInFrame, 2));

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

            var fftResult = CalcFFT(frame);
            var frameSpectralFlatness = CalcSpectralFlatness(fftResult);
            var frameDominantFrequence = CalcDominantFrequence(fftResult);

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

        double CalcSpectralFlatness(FFTResult fftResult)
        {
            throw new NotImplementedException();
        }

        double CalcDominantFrequence(FFTResult fftResult)
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

        private FFTResult CalcFFT(short[] frame)
        {
            // get frame part according to fft size
            uint fftSize = fft.N;
            var re = new double[fftSize];
            var im = new double[fftSize];
            Array.Copy(frame, (frame.Length - fftSize) / 2, re, 0, fftSize);
            fft.run(re, im);

            return new FFTResult { Re = re, Im = im };
        }

        private struct FFTResult
        {
            public double[] Re;
            public double[] Im;
        }
    }
}

