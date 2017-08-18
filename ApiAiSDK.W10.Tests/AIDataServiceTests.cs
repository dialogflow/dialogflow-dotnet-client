/**
 * Copyright 2017 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiAiSDK.Model;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace ApiAiSDK.W10.Tests
{
    [TestClass]
    public class AIDataServiceTests
    {
        private const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";

        [TestMethod]
        public async Task TestTextRequest()
        {
            var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);

            var request = new AIRequest("Hello");

            var response = await dataService.RequestAsync(request);

            Assert.IsNotNull(response);
            Assert.AreEqual("greeting", response.Result.Action);
            Assert.AreEqual("Hi! How are you?", response.Result.Fulfillment.Speech);
        }

        [TestMethod]
        public async Task DifferentAgentsTest()
        {
            var query = "I want pizza";

            {
                var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
                var dataService = new AIDataService(config);

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

        [TestMethod]
        public async Task SessionTest()
        {
            var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

            var firstService = new AIDataService(config);
            var secondService = new AIDataService(config);

            {
                var weatherRequest = new AIRequest("weather");
                var weatherResponse = await MakeRequestAsync(firstService, weatherRequest);
                Assert.IsNotNull(weatherResponse);
            }

            {
                var checkSecondRequest = new AIRequest("check weather");
                var checkSecondResponse = await MakeRequestAsync(secondService, checkSecondRequest);
                Assert.IsTrue(string.IsNullOrEmpty(checkSecondResponse.Result.Action));
            }

            {
                var checkFirstRequest = new AIRequest("check weather");
                var checkFirstResponse = await MakeRequestAsync(firstService, checkFirstRequest);
                Assert.IsNotNull(checkFirstResponse.Result.Action);
                Assert.IsTrue(checkFirstResponse.Result.Action.Equals("checked", StringComparison.CurrentCultureIgnoreCase));
            }
        }

        [TestMethod]
        public async Task ParametersTest()
        {
            var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);

            var request = new AIRequest("what is your name");

            var response = await MakeRequestAsync(dataService, request);

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

        [TestMethod]
        public async Task ContextsTest()
        {
            var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);
            var aiRequest = new AIRequest("weather");

            await dataService.ResetContextsAsync();

            var aiResponse = await MakeRequestAsync(dataService, aiRequest);
            var action = aiResponse.Result.Action;
            Assert.AreEqual("showWeather", action);
            Assert.IsTrue(aiResponse.Result.Contexts.Any(c => c.Name == "weather"));

        }

        [TestMethod]
        public async Task ResetContextsTest()
        {
            var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);

            await dataService.ResetContextsAsync();

            {
                var aiRequest = new AIRequest("what is your name");
                var aiResponse = await MakeRequestAsync(dataService, aiRequest);
                Assert.IsTrue(aiResponse.Result.Contexts.Any(c => c.Name == "name_question"));
                var resetSucceed = await dataService.ResetContextsAsync();
                Assert.IsTrue(resetSucceed);
            }

            {
                var aiRequest = new AIRequest("hello");
                var aiResponse = await MakeRequestAsync(dataService, aiRequest);
                Assert.IsFalse(aiResponse.Result.Contexts.Any(c => c.Name == "name_question"));
            }

        }

        [TestMethod]
        public async Task EntitiesTest()
        {
            var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);

            var aiRequest = new AIRequest("hi nori");

            var myDwarfs = new Entity("dwarfs");
            myDwarfs.AddEntry(new EntityEntry("Ori", new[] { "ori", "Nori" }));
            myDwarfs.AddEntry(new EntityEntry("bifur", new[] { "Bofur", "Bombur" }));

            var extraEntities = new List<Entity> { myDwarfs };

            aiRequest.Entities = extraEntities;

            var aiResponse = await MakeRequestAsync(dataService, aiRequest);

            Assert.IsTrue(!string.IsNullOrEmpty(aiResponse.Result.ResolvedQuery));
            Assert.AreEqual("say_hi", aiResponse.Result.Action);
            Assert.AreEqual("hi Bilbo, I am Ori", aiResponse.Result.Fulfillment.Speech);
        }

        [TestMethod]
        public async Task WrongEntitiesTest()
        {
            var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);
            var dataService = new AIDataService(config);

            var aiRequest = new AIRequest("hi nori");

            var myDwarfs = new Entity("not_dwarfs");
            myDwarfs.AddEntry(new EntityEntry("Ori", new[] { "ori", "Nori" }));
            myDwarfs.AddEntry(new EntityEntry("bifur", new[] { "Bofur", "Bombur" }));

            var extraEntities = new List<Entity> { myDwarfs };

            aiRequest.Entities = extraEntities;

            try
            {
                var aiResponse = await MakeRequestAsync(dataService, aiRequest);
                Assert.IsTrue(false, "Request should throws bad_request exception");
            }
            catch (AIServiceException e)
            {
                Assert.IsTrue(true);
            }
        }

        private async Task<AIResponse> MakeRequestAsync(AIDataService service, AIRequest request)
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
