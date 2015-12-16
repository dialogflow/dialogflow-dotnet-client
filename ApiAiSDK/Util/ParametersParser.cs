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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ApiAiSDK.Model;

namespace ApiAiSDK.Util
{
    public static class ParametersParser
    {
        private const string UNSPECIFIED = "u";

        public static AIDate ParsePartialDate(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.Contains(UNSPECIFIED))
            {
                var parts = parameter.Split('-');
                if (parts.Length != 3)
                {
                    throw new ArgumentException($"Partial date must have format yyyy-mm-dd, but input {parameter}");
                }

                var result = new AIDate
                {
                    Year = ParsePart(parts[0]),
                    Month = ParsePart(parts[1]),
                    Day = ParsePart(parts[2])
                };
                return result;
            }
            else
            {
                var parsedDate = DateTime.ParseExact(parameter, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var result = new AIDate
                {
                    Year = parsedDate.Year,
                    Month = parsedDate.Month,
                    Day = parsedDate.Day
                };
                return result;
            }
        }

        public static AIDate ParsePartialDateTime(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.Contains(UNSPECIFIED))
            {
                var parts = parameter.Split('-', 'T');
                if (parts.Length != 4)
                {
                    throw new ArgumentException($"Partial date time must have format yyyy-MM-ddTHH:mm:sszzz, but input {parameter}");
                }
                var result = new AIDate
                {
                    Year = ParsePart(parts[0]),
                    Month = ParsePart(parts[1]),
                    Day = ParsePart(parts[2])
                };

                var parsedDate = DateTime.ParseExact(parts[3], "HH:mm:sszzz", CultureInfo.InvariantCulture);

                result.Hour = parsedDate.Hour;
                result.Minute = parsedDate.Minute;
                result.Second = parsedDate.Second;

                return result;
            }
            else
            {
                var parsedDate = DateTime.ParseExact(parameter, "yyyy-MM-dd\\THH:mm:sszzz", CultureInfo.InvariantCulture);
                var result = new AIDate(parsedDate);
                
                return result;
            }
        }

        private static int? ParsePart(string part)
        {
            if (part.Contains(UNSPECIFIED))
            {
                return null;
            }
            else
            {
                return Convert.ToInt32(part);
            }
        }
    }
}
