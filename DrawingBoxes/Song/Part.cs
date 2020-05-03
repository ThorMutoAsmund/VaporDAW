using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class Part
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string ScriptId { get; set; }
        [JsonProperty] public string TrackId { get; set; }
        [JsonProperty] public double Start { get; set; }
        [JsonProperty] public double Length { get; set; }
        [JsonProperty] public string Title { get; set; }
        [JsonProperty] public List<Generator> Generators { get; set; }

        public void ChangeTrack(int trackNo)
        {
            if (trackNo < 0 || trackNo >= Env.Song.Tracks.Count())
            {
                trackNo = 0;
            }

            this.TrackId = Env.Song.Tracks[trackNo].Id;

            Song.ChangesMade = true;
        }

        public void AddSample(string sampleName)
        {
            var sampleScriptRef = Env.Song.FindOrAddScript(Env.DefaultSampleScriptName, Env.SampleTemplateScriptName);

            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = sampleScriptRef.Id,
                Settings = new Dictionary<string, object>()
                {
                    { "sampleId", Env.Song.FindSample(sampleName).Id }
                }
            };
            this.Generators.Add(generator);
            Song.ChangesMade = true;
        }

        public void AddScript(string scriptName)
        {
            var scriptRef = Env.Song.FindScript(scriptName);

            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = scriptRef.Id,
            };
            this.Generators.Add(generator);
            Song.ChangesMade = true;
        }

        public int GetTrackNo()
        {
            var index = Env.Song.Tracks.FindIndex(track => track.Id == this.TrackId);
            return index > -1 ? index : 0;
        }
    }
}
