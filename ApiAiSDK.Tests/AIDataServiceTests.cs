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
using System.Linq;
using NUnit.Framework;
using ApiAiSDK;
using ApiAiSDK.Model;

namespace ApiAiSDK.Tests
{
	[TestFixture]
	public class AIDataServiceTests
	{
		private const string SUBSCRIPTION_KEY = "cb9693af-85ce-4fbf-844a-5563722fc27f";
		private const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";

		[Test]
		public void TextRequestTest()
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

		[Test]
		public void DifferentAgentsTest()
		{
			var query = "I want pizza";

			{
				var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
				var dataService = new AIDataService(config);

				var request = new AIRequest(query);
				try {
					var response = dataService.Request(request);
					Assert.NotNull(response.Result);
					Assert.AreEqual("pizza", response.Result.Action);
				} catch (Exception ex) {
					Assert.Fail(ex.Message);
				}
			}

			{
				var config = new AIConfiguration(SUBSCRIPTION_KEY, "968235e8e4954cf0bb0dc07736725ecd", SupportedLanguage.English);
				var dataService = new AIDataService(config);
				var request = new AIRequest(query);
				try {
					var response = dataService.Request(request);
					Assert.NotNull(response.Result);
					Assert.IsTrue(string.IsNullOrEmpty(response.Result.Action));
				} catch (Exception ex) {
					Assert.Fail(ex.Message);
				}
			}

		}

		[Test]
		public void SessionTest()
		{
			var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
			try {
				var firstService = new AIDataService(config);
				var secondService = new AIDataService(config);

				{
					var weatherRequest = new AIRequest("weather");
					var weatherResponse = MakeRequest(firstService, weatherRequest);
				}
				
				{
					var checkSecondRequest = new AIRequest("check weather");
					var checkSecondResponse = MakeRequest(secondService, checkSecondRequest);
					Assert.IsNull(checkSecondResponse.Result.Action);
				}
				
				{
					var checkFirstRequest = new AIRequest("check weather");
					var checkFirstResponse = MakeRequest(firstService, checkFirstRequest);
					Assert.NotNull(checkFirstResponse.Result.Action);
					Assert.IsTrue(checkFirstResponse.Result.Action.Equals("checked", StringComparison.InvariantCultureIgnoreCase));
				}

			} catch (Exception ex) {
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void ParametersTest()
		{
			var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
			var dataService = new AIDataService(config);
			
			var request = new AIRequest("what is your name");
			try {
				var response = MakeRequest(dataService, request);

				Assert.NotNull(response.Result.Parameters);
				Assert.IsTrue(response.Result.Parameters.Count > 0);

				Assert.IsTrue(response.Result.Parameters.ContainsKey("my_name"));
				Assert.IsTrue(response.Result.Parameters.ContainsValue("Sam"));

				Assert.IsNotNull(response.Result.Contexts);
				Assert.IsTrue(response.Result.Contexts.Length > 0);
				var context = response.Result.Contexts[0];

				Assert.IsNotNull(context.Parameters);
				Assert.IsTrue(context.Parameters.ContainsKey("my_name"));
				Assert.IsTrue(context.Parameters.ContainsValue("Sam"));

			} catch (Exception e) {
				Assert.Fail(e.Message);
			}
		}

		private AIResponse MakeRequest(AIDataService service, AIRequest request)
		{
			var aiResponse = service.Request(request);
			Assert.NotNull(aiResponse);
			Assert.False(aiResponse.IsError);
			Assert.False(string.IsNullOrEmpty(aiResponse.Id));
			Assert.NotNull(aiResponse.Result);
			return aiResponse;
		}

	}
}

