//
//  API.AI .NET SDK - client-side libraries for API.AI
//  =================================================
//
//  Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
//  https://www.api.ai
//
//  ***********************************************************************************************************************
//
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
//  the License. You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
//  an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
//  specific language governing permissions and limitations under the License.
//
//  ***********************************************************************************************************************
using System;
using NUnit.Framework;
using Newtonsoft.Json;
using ApiAiSDK.Model;
using Newtonsoft.Json.Linq;

namespace ApiAiSDK.Tests
{
    [TestFixture]
    public class ModelTests
    {
        private const string TEST_DATA = "{\"id\":\"2d2d947b-6ccd-4615-8f16-59b8bfc0fa6b\",\"timestamp\":\"2015-04-13T11:03:43.023Z\",\"result\":{\"source\":\"agent\",\"resolvedQuery\":\"test params 1.23\",\"speech\":\"\",\"action\":\"test_params\",\"parameters\":{\"number\":\"1.23\", \"integer\":\"17\", \"str\":\"string value\", \"complex_param\":{\"nested_key\": \"nested_value\"}},\"contexts\":[],\"metadata\":{\"intentId\":\"46a278fb-0ffc-4748-aa9a-5563d89199ee\",\"intentName\":\"test params\"}},\"status\":{\"code\":200,\"errorType\":\"success\"}}";

        [Test]
        public void TestResultGetString()
        {
            var response = JsonConvert.DeserializeObject<AIResponse>(TEST_DATA);

            Assert.AreEqual("1.23", response.Result.GetStringParameter("number"));
            Assert.AreEqual("default_value", response.Result.GetStringParameter("non_exist_parameter", "default_value"));
            Assert.AreEqual(string.Empty, response.Result.GetStringParameter("non_exist_parameter"));

            Assert.AreEqual("string value", response.Result.GetStringParameter("str"));
        }

        [Test]
        public void TestResultGetInt()
        {
            var response = JsonConvert.DeserializeObject<AIResponse>(TEST_DATA);

            Assert.AreEqual(1, response.Result.GetIntParameter("number"));
            Assert.AreEqual(2, response.Result.GetIntParameter("non_exist_parameter", 2));
            Assert.AreEqual(0, response.Result.GetIntParameter("non_exist_parameter"));

            Assert.AreEqual(17, response.Result.GetIntParameter("integer"));

            Assert.AreEqual(5, response.Result.GetIntParameter("str"), 5);
            Assert.AreEqual(0, response.Result.GetIntParameter("str"));
        }

        [Test]
        public void TestResultGetFloat()
        {
            var response = JsonConvert.DeserializeObject<AIResponse>(TEST_DATA);

            Assert.AreEqual(1.23f, response.Result.GetFloatParameter("number"), float.Epsilon);
            Assert.AreEqual(1.44f, response.Result.GetFloatParameter("non_exist_parameter", 1.44f), float.Epsilon);
            Assert.AreEqual(0, response.Result.GetFloatParameter("non_exist_parameter"));

            Assert.AreEqual(17, response.Result.GetFloatParameter("integer"), float.Epsilon);

            Assert.AreEqual(5f, response.Result.GetFloatParameter("str", 5f), float.Epsilon);
            Assert.AreEqual(0, response.Result.GetFloatParameter("str"));
        }

        [Test]
        public void TestResultGetComplex()
        {
            var response = JsonConvert.DeserializeObject<AIResponse>(TEST_DATA);

            var complexParam = response.Result.GetJsonParameter("complex_param");
            Assert.IsNotNull(complexParam);

            var nestedToken = complexParam["nested_key"] as JValue;
            Assert.NotNull(nestedToken);
            Assert.AreEqual(JTokenType.String, nestedToken.Type);
            Assert.AreEqual("nested_value", nestedToken.ToString());
        }
    }
}

