using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public abstract class Processor
    {
        public string ElementId { get; private set; }
        
        protected ProcessEnv Env { get; private set; }
        protected Song Song { get; private set; }
        protected Part Part { get; private set; }

        public static readonly Processor Empty = new EmptyProcessor();
        
        public virtual void Init(ProcessParams p) { }
        public Mode ProcessResult { get; set; } = Mode.Silence;
        public abstract Mode Process(ProcessParams p);

        private Dictionary<string, Channel> outputChannels = new Dictionary<string, Channel>();

        private Dictionary<string, ProcessorInput> inputs = new Dictionary<string, ProcessorInput>();

        public Dictionary<string, Channel>.ValueCollection OutputChannels => this.outputChannels.Values;
        public Dictionary<string, ProcessorInput>.ValueCollection Inputs => this.inputs.Values;

        public Channel GetOutputChannel(string tag)
        {
            return this.outputChannels.ContainsKey(tag) ? this.outputChannels[tag] : this.Env.EmptyChannel;
        }

        public ProcessorInput SetInput(string inputTag, string outputTag, Processor processor, object originator = null)
        {
            var processorInput = new ProcessorInput(processor, outputTag, originator);
            this.inputs[inputTag] = processorInput;
            
            return processorInput;
        }

        protected Channel AddOutputChannel(string tag)
        {
            var result = new Channel(this, tag);
            this.outputChannels[tag] = result;

            return result;
        }
        protected Channel CreateChannel()
        {
            return new Channel(this);
        }

        public void Setup(ProcessEnv env, Song song, string elementId, Part part)
        {
            this.Env = env;
            this.Song = song;
            this.Part = part;
            this.ElementId = elementId;
        }

        // Helpers!

        protected void ReadSample(SampleRef sampleRef)
        {

        }
    }
}
