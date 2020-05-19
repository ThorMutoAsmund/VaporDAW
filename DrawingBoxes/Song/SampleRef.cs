using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class SampleRef
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string FileName { get; set; }
        public string Name => Path.GetFileNameWithoutExtension(this.FileName);
    }
}
