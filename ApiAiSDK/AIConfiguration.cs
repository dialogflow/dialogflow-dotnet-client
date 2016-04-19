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
using System;

namespace ApiAiSDK
{
	public class AIConfiguration
	{

		private const string SERVICE_PROD_URL = "https://api.api.ai/v1/";
		private const string SERVICE_DEV_URL = "https://dev.api.ai/api/";

        private const string CURRENT_PROTOCOL_VERSION = "20150910";
        
		public string ClientAccessToken { get; private set; }

		public SupportedLanguage Language { get; set; }

        public bool VoiceActivityDetectionEnabled { get; set; }

        public string SessionId { get; set; }

		/// <summary>
		/// If true, will be used Testing API.AI server instead of Production server. This option for TESTING PURPOSES ONLY.
		/// </summary>
		public bool DevMode { get; set; }

		/// <summary>
		/// If true, all request and response content will be printed to the console. Use this option only FOR DEVELOPMENT.
		/// </summary>
		public bool DebugLog { get; set; }

        string protocolVersion;
        public string ProtocolVersion
        {
            get
            {
                return protocolVersion;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                protocolVersion = value;
            }
        }
        	
		public AIConfiguration(string clientAccessToken, SupportedLanguage language)
		{
			this.ClientAccessToken = clientAccessToken;
			this.Language = language;

			DevMode = false;
			DebugLog = false;
            VoiceActivityDetectionEnabled = true;

            ProtocolVersion = CURRENT_PROTOCOL_VERSION;
		}

		public string RequestUrl {
			get {
				var baseUrl = DevMode ? SERVICE_DEV_URL : SERVICE_PROD_URL;
                return string.Format("{0}{1}?v={2}", baseUrl, "query", ProtocolVersion);
			}
		}

	}
}