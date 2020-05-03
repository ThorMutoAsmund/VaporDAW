using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class ScriptRef
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string FileName { get; set; }
        public string Name => Path.GetFileNameWithoutExtension(this.FileName);
    }
}
