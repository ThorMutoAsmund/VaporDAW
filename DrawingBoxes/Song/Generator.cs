using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class Generator
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string ScriptId { get; set; }
        [JsonProperty] public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        public Generator Clone()
        {
            return new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = this.ScriptId,
                Settings = new Dictionary<string, object>(this.Settings)
            };
        }
    }
}
