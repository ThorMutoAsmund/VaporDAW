using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class Track
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string ScriptId { get; set; }
        [JsonProperty] public string Title { get; set; }
        [JsonProperty] public bool IsAudible { get; set; }
        [JsonProperty] public bool IsMuted { get; set; }
        [JsonProperty] public bool IsSolo { get; set; }
    }
}
