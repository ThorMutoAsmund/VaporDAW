using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingBoxes
{
    public class ProcessEnv
    {
        public Processor Mixer { get; private set; }
        public Dictionary<string, Processor> Processors { get; private set; }
        private Song song;

        public ProcessEnv CreateFrom(Song song)
        {
            this.song = song;

            this.Mixer = song.CreateProcessor(song.ScriptId, string.Empty);
            var trackProcessors = new List<Processor>();

            this.Processors = new Dictionary<string, Processor>() { { song.ScriptId, this.Mixer } };
            song.Tracks.ForEach(track => this.Processors[track.Id] = song.CreateProcessor(track.ScriptId, track.Id));
            song.Parts.ForEach(part => this.Processors[part.Id] = song.CreateProcessor(part.ScriptId, part.Id));
            song.Parts.SelectMany(part => part.Generators).ToList().ForEach(generator => this.Processors[generator.Id] = song.CreateProcessor(generator.ScriptId, generator.Id));

            return this;
        }

        public void InitProcessors()
        {
            foreach (var processor in this.Processors.Values)
            {
                processor.Init(this, this.song);
            }
        }

        public void Process(ProcessParams processParams) => this.Mixer.Process(processParams);
    }
}
