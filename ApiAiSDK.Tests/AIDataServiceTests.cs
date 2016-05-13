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
		private const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";

		[Test]
		public void TextRequestTest()
		{
			var dataService = CreateDataService();

		    var request = new AIRequest("Hello");
			
			var response = dataService.Request(request);
			Assert.IsNotNull(response);
			Assert.AreEqual("greeting", response.Result.Action);
            Assert.AreEqual("Hi! How are you?", response.Result.Fulfillment.Speech);
		}

	    private AIDataService CreateDataService()
	    {
	        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
	        var dataService = new AIDataService(config);
	        return dataService;
	    }

	    [Test]
		public void DifferentAgentsTest()
		{
			var query = "I want pizza";

			{
				var dataService = CreateDataService();

				var request = new AIRequest(query);
				
				var response = dataService.Request(request);
				Assert.IsNotNull(response.Result);
				Assert.AreEqual("pizza", response.Result.Action);
				
			}

			{
				var config = new AIConfiguration("968235e8e4954cf0bb0dc07736725ecd", SupportedLanguage.English);
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
			var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
			
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
			var dataService = CreateDataService();
			
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
            var dataService = CreateDataService();
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
            var dataService = CreateDataService();

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
            var dataService = CreateDataService();

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
            var dataService = CreateDataService();

            var aiRequest = new AIRequest("hi nori");

            var myDwarfs = new Entity("not_dwarfs");
            myDwarfs.AddEntry(new EntityEntry("Ori", new [] {"ori", "Nori"}));
            myDwarfs.AddEntry(new EntityEntry("bifur", new [] {"Bofur","Bombur"}));

            var extraEntities = new List<Entity> { myDwarfs };

            aiRequest.Entities = extraEntities;

            Assert.Throws<AIServiceException>(delegate 
                {
                    MakeRequest(dataService, aiRequest);
                }); 
        }

        [Test]
        public void ComplexParameterTest()
        {
            var config = new AIConfiguration(
                "23e7d37f6dd24e4eb7dbbd7491f832cf", // special agent with domains
                SupportedLanguage.English);
            
            var dataService = new AIDataService(config);

            var aiRequest = new AIRequest("Turn off TV at 7pm");
            var response = MakeRequest(dataService, aiRequest);

            Assert.AreEqual("domains", response.Result.Source);
            Assert.AreEqual("smarthome.appliances_off", response.Result.Action);

            var actionCondition = response.Result.GetJsonParameter("action_condition");

            var timeToken = actionCondition.SelectToken("time");
            Assert.IsNotNull(timeToken);
        }

        [Test]
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

            Assert.AreEqual("Weather in London for tomorrow", response.Result.Fulfillment.Speech);
	    }

        [Test]
        public void ContextLifespanTest()
        {
            var dataService = CreateDataService();
            var aiResponse = MakeRequest(dataService, new AIRequest("weather in london"));
            Assert.AreEqual(2, aiResponse.Result.GetContext("shortContext").Lifespan.Value);
            Assert.AreEqual(5, aiResponse.Result.GetContext("weather").Lifespan.Value);
            Assert.AreEqual(10, aiResponse.Result.GetContext("longContext").Lifespan.Value);

            for (int i = 0; i < 3; i++)
            {
                aiResponse = MakeRequest(dataService, new AIRequest("another request"));
            }
            Assert.IsNull(aiResponse.Result.GetContext("shortContext"));
            Assert.IsNotNull(aiResponse.Result.GetContext("weather"));
            Assert.IsNotNull(aiResponse.Result.GetContext("longContext"));

            for (int i = 0; i < 3; i++)
            {
                aiResponse = MakeRequest(dataService, new AIRequest("another request"));
            }
            Assert.IsNull(aiResponse.Result.GetContext("shortContext"));
            Assert.IsNull(aiResponse.Result.GetContext("weather"));
            Assert.IsNotNull(aiResponse.Result.GetContext("longContext"));
        }

        [Test]
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
            Assert.IsNotNull(response.Result.GetContext("weather"));

            for (int i = 0; i < 2; i++)
            {
                response = MakeRequest(dataService, new AIRequest("next request"));
            }

            Assert.IsNotNull(response.Result.GetContext("weather"));
            response = MakeRequest(dataService, new AIRequest("next request"));
            Assert.IsNull(response.Result.GetContext("weather"));
        }

        [Test]
        public void DutchLangTest()
        {
            var config = new AIConfiguration("77a69b3645a745999db66fcd7f0fd813", SupportedLanguage.English);
            var dataService = new AIDataService(config);

            var response = MakeRequest(dataService, new AIRequest("hallo"));
            Assert.IsNotNull(response);
            Assert.AreEqual("goededag!", response.Result.Fulfillment.Speech);
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

