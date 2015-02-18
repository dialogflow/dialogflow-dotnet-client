//
// API.AI .NET SDK tests - client-side tests for API.AI
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
using NUnit.Framework;
using ApiAiSDK;
using ApiAiSDK.Model;

namespace ApiAiSDK.Tests
{
	[TestFixture]
	public class AIDataServiceTests
	{
		private readonly string SUBSCRIPTION_KEY = "cb9693af-85ce-4fbf-844a-5563722fc27f";
		private readonly string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";

		[Test]
		public void TestTextRequest()
		{
			var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
			var dataService = new AIDataService(config);

			var request = new AIRequest("Hello");
			try {
				var response = dataService.Request(request);
				Assert.NotNull(response);
				Assert.AreEqual("greeting", response.Result.Action);
				Assert.AreEqual("Hi! How are you?", response.Result.Speech);
			} catch (Exception e) {
				Assert.Fail(e.Message);
			}
		}
	}
}

