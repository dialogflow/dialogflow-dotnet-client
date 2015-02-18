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
using System.Collections.Generic;
using fastJSON;

namespace ApiAiSDK.Model
{
	[Serializable]
	public class AIRequest : QuestionMetadata
	{
		[JsonProperty("query")]
		public string[] Query { get; set; }
	
		[JsonProperty("confidence")]
		public float[] Confidence { get; set; }
	
		[JsonProperty("contexts")]
		public List<String> Contexts { get; set; }
	
		[JsonProperty("resetContexts")]
		public bool ResetContexts { get; set; }

		public AIRequest ()
		{
		}

		public AIRequest (string text)
		{
			Query = new string[] { text };
			Confidence = new float[] { 1.0f };
		}

	}
}
