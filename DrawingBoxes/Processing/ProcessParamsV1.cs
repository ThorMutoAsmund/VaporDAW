using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class ProcessParamsV1
    {
        public int SampleStart { get; private set; }
        public int SampleLength { get; private set; }

        public double SampleRate { get; private set; }
        public int SampleEnd { get; private set; }

        public ProcessParamsV1(ProcessEnv env, int sampleStart, int sampleLength)
        {
            this.SampleStart = sampleStart;
            this.SampleLength = sampleLength;
            this.SampleRate = env.Song.SampleRate;

            this.SampleEnd = this.SampleStart + this.SampleLength;
        }
    }    
}
