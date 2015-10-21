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

namespace ApiAiSDK
{
    public abstract class ApiAiBase
    {
        protected float[] TrimSilence(float[] samples)
        {
            if (samples == null) {
                return null;
            }

            const float min = 0.000001f;

            var startIndex = 0;
            var endIndex = samples.Length;

            for (var i = 0; i < samples.Length; i++) {

                if (Math.Abs(samples[i]) > min) {
                    startIndex = i;
                    break;
                }
            }

            for (var i = samples.Length - 1; i > 0; i--) {
                if (Math.Abs(samples[i]) > min) {
                    endIndex = i;
                    break;
                }
            }

            if (endIndex <= startIndex) {
                return null;
            }

            var result = new float[endIndex - startIndex];
            Array.Copy(samples, startIndex, result, 0, endIndex - startIndex);
            return result;

        }

        protected static byte[] ConvertArrayShortToBytes(short[] array)
        {
            var numArray = new byte[array.Length * 2];
            Buffer.BlockCopy(array, 0, numArray, 0, numArray.Length);
            return numArray;
        }

        protected static short[] ConvertIeeeToPcm16(float[] source)
        {
            var resultBuffer = new short[source.Length];
            for (var i = 0; i < source.Length; i++) {
                var f = source[i] * 32768f;

                if (f > (double)short.MaxValue)
                    f = short.MaxValue;
                else if (f < (double)short.MinValue)
                    f = short.MinValue;
                resultBuffer[i] = Convert.ToInt16(f);
            }

            return resultBuffer;
        }
    }
}

