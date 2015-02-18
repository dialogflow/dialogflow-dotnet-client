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

using System.Collections;
using ApiAiSDK.Model;

namespace ApiAiSDK
{
	public class AIConfiguration
	{

		private const string SERVICE_PROD_URL = "https://api.api.ai/v1/";
		private const string SERVICE_DEV_URL = "https://dev.api.ai/api/";

		public string SubscriptionKey { get; private set; }

		public string ClientAccessToken { get; private set; }

		public SupportedLanguage Language { get; set; }

		public bool DebugMode { get; set; }
	
		public AIConfiguration(string subscriptionKey, string clientAccessToken, SupportedLanguage language)
		{
			this.SubscriptionKey = subscriptionKey;
			this.ClientAccessToken = clientAccessToken;
			this.Language = language;

			DebugMode = false;
		}

		public string RequestUrl {
			get {
				if (DebugMode) {
					return SERVICE_DEV_URL + "query";
				} else {
					return SERVICE_PROD_URL + "query";
				}
			}
		}

	}
}