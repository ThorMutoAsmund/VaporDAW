using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class ProcessParams
    {
        public int SampleStart { get; private set; }
        public int SampleLength { get; private set; }

        public double SampleRate { get; private set; }
        public int SampleEnd { get; private set; }

        public ProcessParams(ProcessEnv env, int sampleStart, int sampleLength)
        {
            this.SampleStart = sampleStart;
            this.SampleLength = sampleLength;
            this.SampleRate = env.Song.SampleRate;

            this.SampleEnd = this.SampleStart + this.SampleLength;
        }
    }    
}
