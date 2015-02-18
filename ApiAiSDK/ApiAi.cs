//
// API.AI .NET SDK - client-side libraries for API.AI
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
using System.Collections;
using System.Net;
using System.IO;
using ApiAiSDK.Model;

namespace ApiAiSDK
{
	public class ApiAi
	{
		private AIConfiguration config;
		private AIDataService dataService;

		public ApiAi(AIConfiguration config)
		{
			this.config = config;

			dataService = new AIDataService(this.config);
		}

		public AIResponse textRequest(string text)
		{
			return dataService.Request(new AIRequest(text));
		}

		public AIResponse voiceRequest(Stream voiceStream)
		{
			return dataService.VoiceRequest(voiceStream);
		}

		public AIResponse voiceRequest(float[] samples)
		{
			try {

				var trimmedSamples = TrimSilence(samples);
			
				if (trimmedSamples != null) {
				
					var pcm16 = ConvertIeeeToPcm16(trimmedSamples);
					var bytes = ConvertArrayShortToBytes(pcm16);

					var voiceStream = new MemoryStream(bytes);
					voiceStream.Seek(0, SeekOrigin.Begin);
				
					var aiResponse = voiceRequest(voiceStream);
					return aiResponse;
				}

			} catch (AIServiceException) {
				throw;
			} catch (Exception e) {
				throw new AIServiceException(e);
			}

			return null;
		}

		private float[] TrimSilence(float[] samples)
		{
			if (samples == null) {
				return null;
			}

			float min = 0.000001f;
			
			int startIndex = 0;
			int endIndex = samples.Length;
			
			for (int i = 0; i < samples.Length; i++) {
				
				if (Math.Abs(samples[i]) > min) {
					startIndex = i;
					break;
				}
			}

			for (int i = samples.Length - 1; i > 0; i--) {
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

		private static byte[] ConvertArrayShortToBytes(short[] array)
		{
			byte[] numArray = new byte[array.Length * 2];
			Buffer.BlockCopy((Array)array, 0, (Array)numArray, 0, numArray.Length);
			return numArray;
		}
		
		private static short[] ConvertIeeeToPcm16(float[] source)
		{
			short[] resultBuffer = new short[source.Length];
			for (int i = 0; i < source.Length; i++) {
				float f = source[i] * 32768f;
				
				if ((double)f > (double)short.MaxValue)
					f = (float)short.MaxValue;
				else if ((double)f < (double)short.MinValue)
					f = (float)short.MinValue;
				resultBuffer[i] = Convert.ToInt16(f);
			}
			
			return resultBuffer;
		}
	}
}