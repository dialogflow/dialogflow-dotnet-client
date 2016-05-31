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
using Newtonsoft.Json;

namespace ApiAiSDK.Model
{
    [JsonObject]
	public class AIResponse
	{
		[JsonProperty("id")]
		public string Id { get; set; }
	
		[JsonProperty("timestamp")]
		public DateTime Timestamp{ get; set; }

		[JsonProperty("result")]
		public Result Result{ get; set; }
	
		[JsonProperty("status")]
		public Status Status{ get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        public bool IsError
		{
			get
			{
				if (Status != null && Status.Code.HasValue && Status.Code >= 400) {
					return true;
				}

				return false;
			}
		}
	}

}