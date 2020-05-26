using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class ChannelSampleProvider : IPositionedSampleProvider
    {
        public event Action<long> PositionUpdated;

        private readonly Channel channel;
        private long position;

        public WaveFormat WaveFormat
        {
            get => new WaveFormat(this.channel.SampleRate, 2);
        }

        public ChannelSampleProvider(Channel channel)
        {
            this.channel = channel;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            PositionUpdated?.Invoke(position/2);

            var availableSamples = channel.Data.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            for (int s=0; s < samplesToCopy/2; ++s)
            {
                buffer[offset + s * 2] = (float)channel.Data[position / 2 + s].Left;
                buffer[offset + s * 2 + 1] = (float)channel.Data[position / 2 + s].Right;
            }
            //Array.Copy(channel.Data, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return (int)samplesToCopy;
        }
    }
}
