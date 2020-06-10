using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class ProcessorInput
    {
        public ProcessorV1 Provider { get; private set; }
        public string OutputTag { get; private set; }
        private object Originator { get; set; }

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

        public ProcessorInput(ProcessorV1 input, string outputTag, object originator = null)
        {
            this.Provider = input;
            this.OutputTag = outputTag;
            this.Originator = originator;
        }

        public T GetOriginator<T>() where T: class
        {
            return this.Originator as T;
        }
    }
}
