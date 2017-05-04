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
using ApiAiSDK;
using ApiAiSDK.Model;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace ApiAiSDK.Tests
{
	public class AIDataServiceTests
	{
		private const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";

		[Fact]
		public void TextRequestTest()
		{
			var dataService = CreateDataService();

		    var request = new AIRequest("Hello");

			var response = dataService.Request(request);
			Assert.NotNull(response);
			Assert.Equal("greeting", response.Result.Action);
            Assert.Equal("Hi! How are you?", response.Result.Fulfillment.Speech);
            Assert.False(string.IsNullOrEmpty(response.SessionId));
		}

	    private AIDataService CreateDataService()
	    {
	        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
	        var dataService = new AIDataService(config);
	        return dataService;
	    }

	    [Fact]
		public void DifferentAgentsTest()
		{
			var query = "I want pizza";

			{
				var dataService = CreateDataService();

				var request = new AIRequest(query);

				var response = dataService.Request(request);
				Assert.NotNull(response.Result);
				Assert.Equal("pizza", response.Result.Action);

			}

			{
				var config = new AIConfiguration("968235e8e4954cf0bb0dc07736725ecd", SupportedLanguage.English);
				var dataService = new AIDataService(config);
				var request = new AIRequest(query);

				var response = dataService.Request(request);
				Assert.NotNull(response.Result);
				Assert.True(string.IsNullOrEmpty(response.Result.Action));

			}

		}

		[Fact]
		public void SessionTest()
		{
			var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

			var firstService = new AIDataService(config);
			var secondService = new AIDataService(config);

			{
				var weatherRequest = new AIRequest("weather");
				var weatherResponse = MakeRequest(firstService, weatherRequest);
                Assert.NotNull(weatherResponse);
			}

			{
				var checkSecondRequest = new AIRequest("check weather");
				var checkSecondResponse = MakeRequest(secondService, checkSecondRequest);
                Assert.Empty(checkSecondResponse.Result.Action);
			}

			{
				var checkFirstRequest = new AIRequest("check weather");
				var checkFirstResponse = MakeRequest(firstService, checkFirstRequest);
				Assert.NotNull(checkFirstResponse.Result.Action);
				Assert.True(checkFirstResponse.Result.Action.Equals("checked", StringComparison.OrdinalIgnoreCase));
			}
		}

		[Fact]
		public void ParametersTest()
		{
			var dataService = CreateDataService();

			var request = new AIRequest("what is your name");

			var response = MakeRequest(dataService, request);

			Assert.NotNull(response.Result.Parameters);
			Assert.True(response.Result.Parameters.Count > 0);

			Assert.True(response.Result.Parameters.ContainsKey("my_name"));
			Assert.True(response.Result.Parameters.ContainsValue("Sam"));

			Assert.NotNull(response.Result.Contexts);
			Assert.True(response.Result.Contexts.Length > 0);
			var context = response.Result.Contexts[0];

			Assert.NotNull(context.Parameters);
			Assert.True(context.Parameters.ContainsKey("my_name"));
			Assert.True(context.Parameters.ContainsValue("Sam"));
		}

        [Fact]
        public void ContextsTest()
        {
            var dataService = CreateDataService();
            var aiRequest = new AIRequest("weather");

            dataService.ResetContexts();

            var aiResponse = MakeRequest(dataService, aiRequest);
            var action = aiResponse.Result.Action;
            Assert.Equal("showWeather", action);
            Assert.True(aiResponse.Result.Contexts.Any(c=>c.Name == "weather"));

        }

        [Fact]
        public void ResetContextsTest()
        {
            var dataService = CreateDataService();

            dataService.ResetContexts();

            {
                var aiRequest = new AIRequest("what is your name");
                var aiResponse = MakeRequest(dataService, aiRequest);
                Assert.True(aiResponse.Result.Contexts.Any(c=>c.Name == "name_question"));
                var resetSucceed = dataService.ResetContexts();
                Assert.True(resetSucceed);
            }

            {
                var aiRequest = new AIRequest("hello");
                var aiResponse = MakeRequest(dataService, aiRequest);
                Assert.False(aiResponse.Result.Contexts.Any(c=>c.Name == "name_question"));
            }

        }

        [Fact]
        public void EntitiesTest()
        {
            var dataService = CreateDataService();

            var aiRequest = new AIRequest("hi nori");

            var myDwarfs = new Entity("dwarfs");
            myDwarfs.AddEntry(new EntityEntry("Ori", new [] {"ori", "Nori"}));
            myDwarfs.AddEntry(new EntityEntry("bifur", new [] {"Bofur","Bombur"}));

            var extraEntities = new List<Entity> { myDwarfs };

            aiRequest.Entities = extraEntities;

            var aiResponse = MakeRequest(dataService, aiRequest);

            Assert.True(!string.IsNullOrEmpty(aiResponse.Result.ResolvedQuery));
            Assert.Equal("say_hi", aiResponse.Result.Action);
            Assert.Equal("hi Bilbo, I am Ori", aiResponse.Result.Fulfillment.Speech);
        }

        [Fact]
        public void WrongEntitiesTest()
        {
            Assert.Throws<AIServiceException>(() =>
            { 
                var dataService = CreateDataService();

                var aiRequest = new AIRequest("hi nori");

                var myDwarfs = new Entity("not_dwarfs");
                myDwarfs.AddEntry(new EntityEntry("Ori", new[] { "ori", "Nori" }));
                myDwarfs.AddEntry(new EntityEntry("bifur", new[] { "Bofur", "Bombur" }));

                var extraEntities = new List<Entity> { myDwarfs };

                aiRequest.Entities = extraEntities;

                MakeRequest(dataService, aiRequest);
            });
        }

        [Fact]
	    public void InputContextWithParametersTest()
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

            var response = MakeRequest(dataService, aiRequest);

            Assert.Equal("Weather in London for tomorrow", response.Result.Fulfillment.Speech);
	    }

        [Fact]
        public void ContextLifespanTest()
        {
            var dataService = CreateDataService();
            var aiResponse = MakeRequest(dataService, new AIRequest("weather in london"));
            Assert.Equal(2, aiResponse.Result.GetContext("shortContext").Lifespan.Value);
            Assert.Equal(5, aiResponse.Result.GetContext("weather").Lifespan.Value);
            Assert.Equal(10, aiResponse.Result.GetContext("longContext").Lifespan.Value);

            for (int i = 0; i < 3; i++)
            {
                aiResponse = MakeRequest(dataService, new AIRequest("another request"));
            }
            Assert.Null(aiResponse.Result.GetContext("shortContext"));
            Assert.NotNull(aiResponse.Result.GetContext("weather"));
            Assert.NotNull(aiResponse.Result.GetContext("longContext"));

            for (int i = 0; i < 3; i++)
            {
                aiResponse = MakeRequest(dataService, new AIRequest("another request"));
            }
            Assert.Null(aiResponse.Result.GetContext("shortContext"));
            Assert.Null(aiResponse.Result.GetContext("weather"));
            Assert.NotNull(aiResponse.Result.GetContext("longContext"));
        }

        [Fact]
        public void InputContextLifespanTest()
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

            var response = MakeRequest(dataService, aiRequest);
            Assert.NotNull(response.Result.GetContext("weather"));

            for (int i = 0; i < 2; i++)
            {
                response = MakeRequest(dataService, new AIRequest("next request"));
            }

            Assert.NotNull(response.Result.GetContext("weather"));
            response = MakeRequest(dataService, new AIRequest("next request"));
            Assert.Null(response.Result.GetContext("weather"));
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

