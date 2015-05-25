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
using System.Collections.Generic;

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
			
			var response = dataService.Request(request);
			Assert.IsNotNull(response);
			Assert.AreEqual("greeting", response.Result.Action);
            Assert.AreEqual("Hi! How are you?", response.Result.Fulfillment.Speech);
		}

		[Test]
		public void DifferentAgentsTest()
		{
			var query = "I want pizza";

			{
				var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
				var dataService = new AIDataService(config);

				var request = new AIRequest(query);
				
				var response = dataService.Request(request);
				Assert.IsNotNull(response.Result);
				Assert.AreEqual("pizza", response.Result.Action);
				
			}

			{
				var config = new AIConfiguration(SUBSCRIPTION_KEY, "968235e8e4954cf0bb0dc07736725ecd", SupportedLanguage.English);
				var dataService = new AIDataService(config);
				var request = new AIRequest(query);
				
				var response = dataService.Request(request);
				Assert.IsNotNull(response.Result);
				Assert.IsTrue(string.IsNullOrEmpty(response.Result.Action));
				
			}

		}

		[Test]
		public void SessionTest()
		{
			var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
			
			var firstService = new AIDataService(config);
			var secondService = new AIDataService(config);

			{
				var weatherRequest = new AIRequest("weather");
				var weatherResponse = MakeRequest(firstService, weatherRequest);
                Assert.IsNotNull(weatherResponse);
			}
			
			{
				var checkSecondRequest = new AIRequest("check weather");
				var checkSecondResponse = MakeRequest(secondService, checkSecondRequest);
                Assert.IsEmpty(checkSecondResponse.Result.Action);
			}
			
			{
				var checkFirstRequest = new AIRequest("check weather");
				var checkFirstResponse = MakeRequest(firstService, checkFirstRequest);
				Assert.IsNotNull(checkFirstResponse.Result.Action);
				Assert.IsTrue(checkFirstResponse.Result.Action.Equals("checked", StringComparison.InvariantCultureIgnoreCase));
			}
		}

		[Test]
		public void ParametersTest()
		{
			var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
			var dataService = new AIDataService(config);
			
			var request = new AIRequest("what is your name");
			
			var response = MakeRequest(dataService, request);

			Assert.IsNotNull(response.Result.Parameters);
			Assert.IsTrue(response.Result.Parameters.Count > 0);

			Assert.IsTrue(response.Result.Parameters.ContainsKey("my_name"));
			Assert.IsTrue(response.Result.Parameters.ContainsValue("Sam"));

			Assert.IsNotNull(response.Result.Contexts);
			Assert.IsTrue(response.Result.Contexts.Length > 0);
			var context = response.Result.Contexts[0];

			Assert.IsNotNull(context.Parameters);
			Assert.IsTrue(context.Parameters.ContainsKey("my_name"));
			Assert.IsTrue(context.Parameters.ContainsValue("Sam"));
		}

        [Test]
        public void ContextsTest()
        {
            var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);
            var aiRequest = new AIRequest("weather");

            dataService.ResetContexts();

            var aiResponse = MakeRequest(dataService, aiRequest);
            var action = aiResponse.Result.Action;
            Assert.AreEqual("showWeather", action);
            Assert.IsTrue(aiResponse.Result.Contexts.Any(c=>c.Name == "weather"));
                
        }

        [Test]
        public void ResetContextsTest()
        {
            var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);

            dataService.ResetContexts();

            {
                var aiRequest = new AIRequest("what is your name");
                var aiResponse = MakeRequest(dataService, aiRequest);
                Assert.IsTrue(aiResponse.Result.Contexts.Any(c=>c.Name == "name_question"));
                var resetSucceed = dataService.ResetContexts();
                Assert.IsTrue(resetSucceed);
            }

            {
                var aiRequest = new AIRequest("hello");
                var aiResponse = MakeRequest(dataService, aiRequest);
                Assert.IsFalse(aiResponse.Result.Contexts.Any(c=>c.Name == "name_question"));
            }
                
        }

        [Test]
        public void EntitiesTest()
        {
            var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);

            var aiRequest = new AIRequest("hi nori");

            var myDwarfs = new Entity("dwarfs");
            myDwarfs.AddEntry(new EntityEntry("Ori", new [] {"ori", "Nori"}));
            myDwarfs.AddEntry(new EntityEntry("bifur", new [] {"Bofur","Bombur"}));

            var extraEntities = new List<Entity> { myDwarfs };

            aiRequest.Entities = extraEntities;

            var aiResponse = MakeRequest(dataService, aiRequest);

            Assert.IsTrue(!string.IsNullOrEmpty(aiResponse.Result.ResolvedQuery));
            Assert.AreEqual("say_hi", aiResponse.Result.Action);
            Assert.AreEqual("hi Bilbo, I am Ori", aiResponse.Result.Fulfillment.Speech);
        }

        [Test]
        public void WrongEntitiesTest()
        {
            var config = new AIConfiguration(SUBSCRIPTION_KEY, ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);

            var aiRequest = new AIRequest("hi nori");

            var myDwarfs = new Entity("not_dwarfs");
            myDwarfs.AddEntry(new EntityEntry("Ori", new [] {"ori", "Nori"}));
            myDwarfs.AddEntry(new EntityEntry("bifur", new [] {"Bofur","Bombur"}));

            var extraEntities = new List<Entity> { myDwarfs };

            aiRequest.Entities = extraEntities;

            try
            {
                var aiResponse = MakeRequest(dataService, aiRequest);
                Assert.True(false, "Request should throws bad_request exception");
            }
            catch(AIServiceException e)
            {
                Assert.IsTrue(true);
            }
        }

		private AIResponse MakeRequest(AIDataService service, AIRequest request)
		{
			var aiResponse = service.Request(request);
			Assert.IsNotNull(aiResponse);
			Assert.IsFalse(aiResponse.IsError);
			Assert.IsFalse(string.IsNullOrEmpty(aiResponse.Id));
			Assert.IsNotNull(aiResponse.Result);
			return aiResponse;
		}

	}
}

