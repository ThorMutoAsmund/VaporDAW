using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingBoxes
{
    public class ProcessParams
    {
        public ProcessEnv Env { get; private set; }
        public double Start { get; private set; }
        public double Length { get; private set; }

        public float SampleRate { get; private set; }

        public int SampleLength { get; private set; }
        public double End { get; private set; }

        public ProcessParams(ProcessEnv env, double start, double length)
        {
            this.SampleRate = 96000;
            this.Env = env;
            this.Start = start;
            this.Length = length;

            this.SampleLength = (int)(this.Length * this.SampleRate);
            this.End = this.Start + this.Length;
        }
    }    
}
