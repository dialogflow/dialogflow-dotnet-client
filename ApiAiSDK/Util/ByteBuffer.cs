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
using System.Runtime.InteropServices;

namespace ApiAiSDK.Util
{
    /// <summary>
    /// Class for converts among different array types. Inspired with NAudio WaveBuffer class https://github.com/naudio/NAudio
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    internal sealed class ByteBuffer
    {
        [FieldOffset(0)]
        private int numberOfBytes;

        [FieldOffset(8)]
        private byte[] byteBuffer;

        [FieldOffset(8)]
        private float[] floatBuffer;

        [FieldOffset(8)]
        private short[] shortBuffer;

        [FieldOffset(8)]
        private int[] intBuffer;

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

        public float[] FloatArray
        {
            get { return floatBuffer; }
        }

        public int FloatArrayLength
        {
            get { return numberOfBytes / 4; }
        }

        public short[] ShortArray
        {
            get { return shortBuffer; }
        }

        public int ShortArrayLength
        {
            get { return numberOfBytes / 2; }
        }
    }
}

