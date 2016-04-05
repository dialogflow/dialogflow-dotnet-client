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
namespace ApiAiSDK
{
	/// <summary>
	/// List of the the languages, supported by API.AI service
	/// </summary>
	public class SupportedLanguage
	{
		public static readonly SupportedLanguage English = new SupportedLanguage("en");
		public static readonly SupportedLanguage Russian = new SupportedLanguage("ru");
		public static readonly SupportedLanguage German = new SupportedLanguage("de");
		public static readonly SupportedLanguage Portuguese = new SupportedLanguage("pt");
		public static readonly SupportedLanguage PortugueseBrazil = new SupportedLanguage("pt-BR");
		public static readonly SupportedLanguage Spanish = new SupportedLanguage("es");
		public static readonly SupportedLanguage French = new SupportedLanguage("fr");
		public static readonly SupportedLanguage Italian = new SupportedLanguage("it");
        public static readonly SupportedLanguage Dutch = new SupportedLanguage("nl");
        public static readonly SupportedLanguage Japanese = new SupportedLanguage("ja");
		public static readonly SupportedLanguage ChineseChina = new SupportedLanguage("zh-CN");
		public static readonly SupportedLanguage ChineseHongKong = new SupportedLanguage("zh-HK");
		public static readonly SupportedLanguage ChineseTaiwan = new SupportedLanguage("zh-TW");

        private static readonly SupportedLanguage[] AllLangs = 
        { 
                English, 
                Russian,  
                German,
                Portuguese,
                PortugueseBrazil,
                Spanish,
                French,
                Italian,
                Dutch,
                Japanese,
                ChineseChina,
                ChineseHongKong,
                ChineseTaiwan
        };

		public readonly string code;

		private SupportedLanguage(string code)
		{
			this.code = code;
		}

        public static SupportedLanguage FromLanguageTag(string languageTag)
        {
            foreach (var item in AllLangs)
            {
                if (string.Equals(item.code, languageTag, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return English;
        }
	}
}

