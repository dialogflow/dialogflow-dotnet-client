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
using System.Text;

namespace ApiAiSDK.Model
{
    public class AIDate
    {
        private const string UNSPECIFIED_YEAR = "uuuu";
        private const string UNSPECIFIED_MONTH = "uu";
        private const string UNSPECIFIED_DAY = "uu";
        private const string UNSPECIFIED_HOUR = "uu";
        private const string UNSPECIFIED_MINUTE = "uu";

        private DateTime dateTime;
        
        private int? year;
        private int? month;
        private int? day;
        private int? hour;
        private int? minute;
        private int? second;

        public int? Year
        {
            get
            {
                return year;
            }
            set
            {
                year = value;
            }
        }

        public int? Month
        {
            get { return month; }
            set { month = value; }
        }

        public int? Day
        {
            get { return day; }
            set { day = value; }
        }

        public int? Hour
        {
            get { return hour; }
            set { hour = value; }
        }

        public int? Minute
        {
            get { return minute; }
            set { minute = value; }
        }

        public int? Second
        {
            get { return second; }
            set { second = value; }
        }

        public AIDate()
        {
            
        }

        public AIDate(DateTime dateTime)
        {
            Year = dateTime.Year;
            Month = dateTime.Month;
            Day = dateTime.Day;
            Hour = dateTime.Hour;
            Minute = dateTime.Minute;
            Second = 0;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0}-{1}-{2}", 
                Year!= null ? Year.Value.ToString("0000") : UNSPECIFIED_YEAR,
                Month!= null ? Month.Value.ToString("00") : UNSPECIFIED_MONTH,
                Day != null ? Day.Value.ToString("00") : UNSPECIFIED_DAY);

            if (Hour != null || Minute!= null)
            {
                builder.Append("T");
                builder.AppendFormat("{0}:{1}:{2}",
                    Hour != null ? Hour.Value.ToString("00") : UNSPECIFIED_HOUR,
                    Minute != null ? Minute.Value.ToString("00") : UNSPECIFIED_MINUTE,
                    Second != null ? Second.Value.ToString("00") : "00");

                var timezone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                builder.Append(timezone.Hours.ToString("+00"));
                builder.Append(":");
                builder.Append(timezone.Minutes.ToString("00"));
            }
            
            return builder.ToString();
        }
    }
}

