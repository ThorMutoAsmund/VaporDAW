using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class Generator
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string ScriptId { get; set; }
        [JsonProperty] public Dictionary<string, object> Settings { get; set; }
        //[JsonProperty] public List<InputOutput> Inputs { get; set; }
        //[JsonProperty] public List<InputOutput> Outputs { get; set; }
    }
}
