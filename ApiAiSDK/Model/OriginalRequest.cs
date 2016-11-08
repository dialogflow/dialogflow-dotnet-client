using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiAiSDK.Model
{
    [JsonObject]
    public class OriginalRequest
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
