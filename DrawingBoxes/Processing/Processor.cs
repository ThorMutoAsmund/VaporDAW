using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public abstract class Processor
    {
        public string ElementId { get; private set; }

        public static readonly Processor Empty = new EmptyProcessor();
        public virtual void Init(ProcessEnv env, Song song) {  }
        public abstract Mode Process(ProcessParams p);

        private Dictionary<string, Channel> outputChannels = new Dictionary<string, Channel>();

        private Dictionary<string, Channel> inputChannels = new Dictionary<string, Channel>();

        public Channel GetOutput(string tag)
        {
            return this.outputChannels.ContainsKey(tag) ? this.outputChannels[tag] : Channel.Empty;
        }

        public void SetInput(string inputTag, string outputTag, Processor input)
        {
            this.inputChannels[inputTag] = input.GetOutput(outputTag);
        }

        protected Channel AddOutputChannel(string tag)
        {
            var result = new Channel();
            this.outputChannels[tag] = result;

            return result;
        }

        public void SetElementId(string elementId)
        {
            this.ElementId = elementId;
        }
    }

    public class EmptyProcessor : Processor
    {
        public override Mode Process(ProcessParams p)
        {
            return Mode.Silence;
        }
    }
}
