using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawingBoxes
{
    public class Channel
    {
        public static readonly Channel Empty = new Channel();
        public Sample[] Samples { get; set; }

        public Channel()
        {
            this.Samples = new Sample[0];
        }
        public Channel(int size)
        {
            this.Samples = new Sample[size];
        }

        public int SampleLength => this.Samples.Length;

        public void Clear(int? resize)
        {
            if (resize.HasValue && resize.Value != this.SampleLength)
            {
                this.Samples = new Sample[resize.Value];
            }
            for (int s = 0; s < this.SampleLength; ++s)
            {
                this.Samples[s].Left = 0;
                this.Samples[s].Right = 0;
            }
        }

        public void Add(Channel channel)
        {
            for (int s = 0; s < Math.Min(this.SampleLength, channel.SampleLength); ++s)
            {
                this.Samples[s].Add(channel.Samples[s]);
            }
        }
    }
}
