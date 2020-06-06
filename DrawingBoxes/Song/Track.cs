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

        public void AddTrackGenerator(ScriptRef scriptRef)
        {
            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = scriptRef.Id,
            };
            this.TrackGenerators.Add(generator);
            Env.Song.OnTrackChanged(this);
        }

        public void DeleteTrackGenerator(Generator generator)
        {
            if (this.TrackGenerators.Contains(generator))
            {
                this.TrackGenerators.Remove(generator);
                Env.Song.OnTrackChanged(this);
            }
        }
    }
}
