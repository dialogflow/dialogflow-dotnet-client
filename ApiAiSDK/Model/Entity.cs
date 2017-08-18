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
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApiAiSDK.Model
{
    [JsonObject]
    public class Entity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("entries")]
        public List<EntityEntry> Entries { get; set; }

        public Entity()
        {
        }

        public Entity(string name)
        {
            this.Name = name;
        }

        public Entity(string name, List<EntityEntry> entries)
        {
            this.Name = name;
            this.Entries = entries;
        }

        public void AddEntry(EntityEntry entry)
        {
            if (Entries == null)
            {
                Entries = new List<EntityEntry>();
            }

            Entries.Add(entry);
        }
        
    }
}

