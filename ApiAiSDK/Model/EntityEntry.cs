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
using Newtonsoft.Json;

namespace ApiAiSDK.Model
{
    [JsonObject]
    public class EntityEntry
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("synonyms")]
        public List<String> Synonyms { get; set; }

        public EntityEntry()
        {
        }

        public EntityEntry(string value, List<string> synonyms)
        {
            this.Value = value;
            this.Synonyms = synonyms;
        }

        public EntityEntry(string value, string[] synonyms): this(value, new List<string>(synonyms))
        {
            
        }
        
    }
}

