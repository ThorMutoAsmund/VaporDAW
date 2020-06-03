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
        //[JsonProperty] public string ScriptId { get; set; }

        [JsonProperty] public List<Generator> TrackGenerators { get; set; } = new List<Generator>();

        [JsonProperty] public string Title { get; set; }
        [JsonProperty] public bool IsAudible { get; set; }
        [JsonProperty] public bool IsMuted { get; set; }
        [JsonProperty] public bool IsSolo { get; set; }

        public void AddTrackScript(ScriptRef scriptRef)
        {
            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = scriptRef.Id,
            };
            this.TrackGenerators.Add(generator);
            Song.ChangesMade = true;
        }
    }
}
