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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ApiAiSDK.Util;
using NUnit.Framework;

namespace ApiAiSDK.Tests
{
    [TestFixture]
    public class ParametersParserTests
    {
        [Test]
        public void ParsePartialDateTest()
        {
            // date in format "yyyy-MM-dd"
            var unknownDate = "1999-01-uu";
            var unknownMonth = "2005-uu-17";
            var unknownYear = "uuuu-07-23";
            var unknownMonthDate = "2008-uu-uu";

            var normalDate = "2003-12-30";

            var date = ParametersParser.ParsePartialDate(unknownDate);

            Assert.AreEqual(1999, date.Year);
            Assert.AreEqual(1, date.Month);
            Assert.AreEqual(null, date.Day);
            Assert.AreEqual(unknownDate, date.ToString());

            date = ParametersParser.ParsePartialDate(unknownMonth);
            Assert.AreEqual(2005, date.Year);
            Assert.AreEqual(null, date.Month);
            Assert.AreEqual(17, date.Day);
            Assert.AreEqual(unknownMonth, date.ToString());

            date = ParametersParser.ParsePartialDate(unknownYear);
            Assert.AreEqual(null, date.Year);
            Assert.AreEqual(7, date.Month);
            Assert.AreEqual(23, date.Day);
            Assert.AreEqual(unknownYear, date.ToString());

            date = ParametersParser.ParsePartialDate(unknownMonthDate);
            Assert.AreEqual(2008, date.Year);
            Assert.AreEqual(null, date.Month);
            Assert.AreEqual(null, date.Day);
            Assert.AreEqual(unknownMonthDate, date.ToString());

            date = ParametersParser.ParsePartialDate(normalDate);
            Assert.AreEqual(2003, date.Year);
            Assert.AreEqual(12, date.Month);
            Assert.AreEqual(30, date.Day);
            Assert.AreEqual(normalDate, date.ToString());
        }

        [Test]
        public void ParsePartialDateTimeTest()
        {
            // date-time in format "yyyy-MM-ddTHH:mm:ssZ"
            // only date part can be partial
            var unknownDate = "1999-05-uuT07:00:00+06:00";
            var unknownMonth = "2005-uu-17T12:30:00+06:00";
            var unknownYear = "uuuu-12-31T17:43:00+06:00";
            var unknownMonthDate = "2008-uu-uuT20:12:00+06:00";

            var normalDate = "2015-10-23T22:55:00+06:00";

            var date = ParametersParser.ParsePartialDateTime(unknownDate);
            Assert.AreEqual(1999, date.Year);
            Assert.AreEqual(5, date.Month);
            Assert.AreEqual(null, date.Day);
            Assert.AreEqual(7, date.Hour);
            Assert.AreEqual(0, date.Minute);
            Assert.AreEqual(0, date.Second);
            Assert.AreEqual(unknownDate, date.ToString());

            date = ParametersParser.ParsePartialDateTime(unknownMonth);
            Assert.AreEqual(2005, date.Year);
            Assert.AreEqual(null, date.Month);
            Assert.AreEqual(17, date.Day);
            Assert.AreEqual(12, date.Hour);
            Assert.AreEqual(30, date.Minute);
            Assert.AreEqual(unknownMonth, date.ToString());

            date = ParametersParser.ParsePartialDateTime(unknownYear);
            Assert.AreEqual(null, date.Year);
            Assert.AreEqual(12, date.Month);
            Assert.AreEqual(31, date.Day);
            Assert.AreEqual(17, date.Hour);
            Assert.AreEqual(43, date.Minute);
            Assert.AreEqual(unknownYear, date.ToString());

            date = ParametersParser.ParsePartialDateTime(unknownMonthDate);
            Assert.AreEqual(2008, date.Year);
            Assert.AreEqual(null, date.Month);
            Assert.AreEqual(null, date.Day);
            Assert.AreEqual(20, date.Hour);
            Assert.AreEqual(12, date.Minute);
            Assert.AreEqual(unknownMonthDate, date.ToString());

            date = ParametersParser.ParsePartialDateTime(normalDate);
            Assert.AreEqual(2015, date.Year);
            Assert.AreEqual(10, date.Month);
            Assert.AreEqual(23, date.Day);
            Assert.AreEqual(22, date.Hour);
            Assert.AreEqual(55, date.Minute);
            Assert.AreEqual(normalDate, date.ToString());
        }

    }
}
