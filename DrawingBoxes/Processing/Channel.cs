﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class Channel
    {
        public Sample[] Data { get; private set; }
        public string Tag { get; private set; }
        public Processor Owner { get; private set; }
        public int SampleRate { get; private set; }

        public Channel(Processor owner, string tag = null, int size = 0, int sampleRate = 44100)
        {
            this.Owner = owner;
            this.Tag = tag;
            this.Data = new Sample[size];
            this.SampleRate = sampleRate;
        }

        public int SampleLength => this.Data.Length;

        public void Clear(int? resize)
        {
            if (resize.HasValue && resize.Value != this.SampleLength)
            {
                this.Data = new Sample[resize.Value];
            }
            else
            {
                Array.Clear(this.Data, 0, this.Data.Length);
            }
        }

        public void Reset(int size, int sampleRate)
        {
            this.Data = new Sample[size];
            this.SampleRate = sampleRate;
        }

        public void Add(Channel channel)
        {
            for (int s = 0; s < Math.Min(this.SampleLength, channel.SampleLength); ++s)
            {
                this.Data[s].Add(channel.Data[s]);
            }
        }

        public void AddRange(Channel channel, int srcOffset, int destOffset, int length)
        {
            for (int s = 0; s < length; ++s) // Tbd Math.Min(length, this.SampleLength, channel.SampleLength); ++s)
            {
                this.Data[s + destOffset].Add(channel.Data[s + srcOffset]);
            }
        }
    }
}
