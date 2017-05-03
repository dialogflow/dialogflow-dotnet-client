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
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ApiAiSDK.Tests
{
	[TestFixture]
	public class AIDataServiceTests
	{
		private const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";

		[Test]
		public async Task TextRequestTest()
		{
			var dataService = CreateDataService();

		    var request = new AIRequest("Hello");

			var response = await dataService.RequestAsync(request);
			Assert.IsNotNull(response);
			Assert.AreEqual("greeting", response.Result.Action);
            Assert.AreEqual("Hi! How are you?", response.Result.Fulfillment.Speech);
            Assert.False(string.IsNullOrEmpty(response.SessionId));
		}

	    private AIDataService CreateDataService()
	    {
	        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
	        var dataService = new AIDataService(config);
	        return dataService;
	    }

	    [Test]
		public async Task DifferentAgentsTest()
		{
			var query = "I want pizza";

			{
				var dataService = CreateDataService();

				var request = new AIRequest(query);

				var response = await dataService.RequestAsync(request);
				Assert.IsNotNull(response.Result);
				Assert.AreEqual("pizza", response.Result.Action);

			}

			{
				var config = new AIConfiguration("968235e8e4954cf0bb0dc07736725ecd", SupportedLanguage.English);
				var dataService = new AIDataService(config);
				var request = new AIRequest(query);

				var response = await dataService.RequestAsync(request);
				Assert.IsNotNull(response.Result);
				Assert.IsTrue(string.IsNullOrEmpty(response.Result.Action));

			}

		}

		[Test]
		public async Task SessionTest()
		{
			var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

			var firstService = new AIDataService(config);
			var secondService = new AIDataService(config);

			{
				var weatherRequest = new AIRequest("weather");
				var weatherResponse = await MakeRequest(firstService, weatherRequest);
                Assert.IsNotNull(weatherResponse);
			}

			{
				var checkSecondRequest = new AIRequest("check weather");
				var checkSecondResponse = await MakeRequest(secondService, checkSecondRequest);
                Assert.IsEmpty(checkSecondResponse.Result.Action);
			}

			{
				var checkFirstRequest = new AIRequest("check weather");
				var checkFirstResponse = await MakeRequest(firstService, checkFirstRequest);
				Assert.IsNotNull(checkFirstResponse.Result.Action);
				Assert.IsTrue(checkFirstResponse.Result.Action.Equals("checked", StringComparison.CurrentCultureIgnoreCase));
			}
		}

		[Test]
		public async Task ParametersTest()
		{
			var dataService = CreateDataService();

			var request = new AIRequest("what is your name");

			var response = await MakeRequest(dataService, request);

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
        public async Task ContextsTest()
        {
            var dataService = CreateDataService();
            var aiRequest = new AIRequest("weather");

            await dataService.ResetContextsAsync();

            var aiResponse = await MakeRequest(dataService, aiRequest);
            var action = aiResponse.Result.Action;
            Assert.AreEqual("showWeather", action);
            Assert.IsTrue(aiResponse.Result.Contexts.Any(c=>c.Name == "weather"));

        }

        [Test]
        public async Task ResetContextsTest()
        {
            var dataService = CreateDataService();

            await dataService.ResetContextsAsync();

            {
                var aiRequest = new AIRequest("what is your name");
                var aiResponse = await MakeRequest(dataService, aiRequest);
                Assert.IsTrue(aiResponse.Result.Contexts.Any(c=>c.Name == "name_question"));
                var resetSucceed = await dataService.ResetContextsAsync();
                Assert.IsTrue(resetSucceed);
            }

            {
                var aiRequest = new AIRequest("hello");
                var aiResponse = await MakeRequest(dataService, aiRequest);
                Assert.IsFalse(aiResponse.Result.Contexts.Any(c=>c.Name == "name_question"));
            }

        }

        [Test]
        public async Task EntitiesTest()
        {
            var dataService = CreateDataService();

            var aiRequest = new AIRequest("hi nori");

            var myDwarfs = new Entity("dwarfs");
            myDwarfs.AddEntry(new EntityEntry("Ori", new [] {"ori", "Nori"}));
            myDwarfs.AddEntry(new EntityEntry("bifur", new [] {"Bofur","Bombur"}));

            var extraEntities = new List<Entity> { myDwarfs };

            aiRequest.Entities = extraEntities;

            var aiResponse = await MakeRequest(dataService, aiRequest);

            Assert.IsTrue(!string.IsNullOrEmpty(aiResponse.Result.ResolvedQuery));
            Assert.AreEqual("say_hi", aiResponse.Result.Action);
            Assert.AreEqual("hi Bilbo, I am Ori", aiResponse.Result.Fulfillment.Speech);
        }

        [Test]
        public void WrongEntitiesTest()
        {
            Assert.ThrowsAsync<AIServiceException>(async () =>
            {
                var dataService = CreateDataService();

                var aiRequest = new AIRequest("hi nori");

                var myDwarfs = new Entity("not_dwarfs");
                myDwarfs.AddEntry(new EntityEntry("Ori", new [] {"ori", "Nori"}));
                myDwarfs.AddEntry(new EntityEntry("bifur", new [] {"Bofur","Bombur"}));

                var extraEntities = new List<Entity> { myDwarfs };

                aiRequest.Entities = extraEntities;

                await MakeRequest(dataService, aiRequest);
            });
        }

        [Test]
	    public async Task InputContextWithParametersTest()
	    {
            var dataService = CreateDataService();

            var aiRequest = new AIRequest("and for tomorrow");
            var aiContext = new AIContext
            {
                Name = "weather",
                Parameters = new Dictionary<string,string>
                {
                    { "location", "London"}
                }
            };

            aiRequest.Contexts =
                new List<AIContext>
                {
                    aiContext
                };

            var response = await MakeRequest(dataService, aiRequest);

            Assert.AreEqual("Weather in London for tomorrow", response.Result.Fulfillment.Speech);
	    }

        [Test]
        public async Task ContextLifespanTest()
        {
            var dataService = CreateDataService();
            var aiResponse = await MakeRequest(dataService, new AIRequest("weather in london"));
            Assert.AreEqual(2, aiResponse.Result.GetContext("shortContext").Lifespan.Value);
            Assert.AreEqual(5, aiResponse.Result.GetContext("weather").Lifespan.Value);
            Assert.AreEqual(10, aiResponse.Result.GetContext("longContext").Lifespan.Value);

            for (int i = 0; i < 3; i++)
            {
                aiResponse = await MakeRequest(dataService, new AIRequest("another request"));
            }
            Assert.IsNull(aiResponse.Result.GetContext("shortContext"));
            Assert.IsNotNull(aiResponse.Result.GetContext("weather"));
            Assert.IsNotNull(aiResponse.Result.GetContext("longContext"));

            for (int i = 0; i < 3; i++)
            {
                aiResponse = await MakeRequest(dataService, new AIRequest("another request"));
            }
            Assert.IsNull(aiResponse.Result.GetContext("shortContext"));
            Assert.IsNull(aiResponse.Result.GetContext("weather"));
            Assert.IsNotNull(aiResponse.Result.GetContext("longContext"));
        }

        [Test]
        public async Task InputContextLifespanTest()
        {
            var dataService = CreateDataService();
            var aiRequest = new AIRequest("and for tomorrow");
            var aiContext = new AIContext
            {
                Name = "weather",
                Parameters = new Dictionary<string, string>
                {
                    { "location", "London"}
                },
                Lifespan = 3
            };

            aiRequest.Contexts =
                new List<AIContext>
                {
                    aiContext
                };

            var response = await MakeRequest(dataService, aiRequest);
            Assert.IsNotNull(response.Result.GetContext("weather"));

            for (int i = 0; i < 2; i++)
            {
                response = await MakeRequest(dataService, new AIRequest("next request"));
            }

            Assert.IsNotNull(response.Result.GetContext("weather"));
            response = await MakeRequest(dataService, new AIRequest("next request"));
            Assert.IsNull(response.Result.GetContext("weather"));
        }


		private async Task<AIResponse> MakeRequest(AIDataService service, AIRequest request)
		{
			var aiResponse = await service.RequestAsync(request);
			Assert.IsNotNull(aiResponse);
			Assert.IsFalse(aiResponse.IsError);
			Assert.IsFalse(string.IsNullOrEmpty(aiResponse.Id));
			Assert.IsNotNull(aiResponse.Result);
			return aiResponse;
		}

	}
}

