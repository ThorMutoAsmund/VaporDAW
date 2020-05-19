using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class ProcessorInput
    {
        public Processor Provider { get; private set; }
        public string OutputTag { get; private set; }

        private Channel channel;
        public Channel ProviderOutputChannel 
        { 
            get
            {
                if (this.channel == null)
                {
                    this.channel = this.Provider.GetOutputChannel(this.OutputTag);
                }

                return this.channel;
            }
        }

        public ProcessorInput(Processor input, string outputTag)
        {
            this.Provider = input;
            this.OutputTag = outputTag;
        }
    }
}
