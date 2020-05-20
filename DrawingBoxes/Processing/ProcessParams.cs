using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class ProcessParams
    {
        public double Start { get; private set; }
        public double Length { get; private set; }

        public double SampleRate { get; private set; }

        public int NumSamples { get; private set; }
        public double End { get; private set; }

        public ProcessParams(ProcessEnv env, double start, double length)
        {
            this.Start = start;
            this.Length = length;
            this.SampleRate = env.Song.SampleFrequency;

            this.NumSamples = (int)(this.Length * this.SampleRate);
            this.End = this.Start + this.Length;
        }
    }    
}
