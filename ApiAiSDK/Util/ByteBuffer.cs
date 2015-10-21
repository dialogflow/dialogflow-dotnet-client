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
    /// <summary>
    /// Class for converts among different array types. Inspired with NAudio WaveBuffer class https://github.com/naudio/NAudio
    /// </summary>
    internal sealed class ByteBuffer
    {
        private readonly int numberOfBytes;

        private byte[] byteBuffer;

        public ByteBuffer(byte[] byteArray)
        {
            numberOfBytes = ByteArray.Length;
            byteBuffer = byteArray;
        }

        public ByteBuffer(byte[] byteArray, int length)
        {
            numberOfBytes = length;
            byteBuffer = byteArray;
        }

        public byte[] ByteArray
        {
            get { return byteBuffer; }
        }

        public int ByteArrayLength
        {
            get { return numberOfBytes; }
        }

        public short[] ShortArray
        {
            get 
            {
                var result = new short[ShortArrayLength];

                var tmpBuffer = new byte[2];

                for (int i = 0; i < numberOfBytes; i += 2)
                {
                    if (BitConverter.IsLittleEndian)
                    {
//                        tmpBuffer[0] = byteBuffer[i + 1];
//                        tmpBuffer[1] = byteBuffer[i];

                        tmpBuffer[0] = byteBuffer[i];
                        tmpBuffer[1] = byteBuffer[i + 1];
                    }
                    else
                    {
                        tmpBuffer[0] = byteBuffer[i];
                        tmpBuffer[1] = byteBuffer[i + 1];
                    }
                        
                    result[i / 2] = BitConverter.ToInt16(tmpBuffer, 0);
                }
                
                return result; 
            }
        }

        public int ShortArrayLength
        {
            get { return numberOfBytes / 2; }
        }
    }
}

