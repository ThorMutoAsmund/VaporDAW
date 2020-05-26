using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class AutoDisposeFileReader : IPositionedSampleProvider
    {
        public event Action<long> PositionUpdated;

        private readonly AudioFileReader reader;
        private long position;
        private bool isDisposed;

        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (this.isDisposed)
            {
                return 0;
            }
            this.PositionUpdated?.Invoke(position / this.WaveFormat.Channels);

            int read = this.reader.Read(buffer, offset, count);
            if (read == 0)
            {
                this.reader.Dispose();
                this.isDisposed = true;
                return read;
            }

            position += count;
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
}
