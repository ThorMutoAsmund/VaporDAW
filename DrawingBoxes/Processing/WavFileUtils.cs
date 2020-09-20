using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public static class WavFileUtils
    {
        // https://github.com/naudio/NAudio
        public static void ReadWavFile(string fileName, Channel channel)
        {
            var inPath = System.IO.Path.Combine(Env.Song.SamplesPath, fileName);
            byte[] samples = null;

            using (var reader = WaveStreamFromPath(inPath))
            {
                if (reader.WaveFormat.Channels != 1 && reader.WaveFormat.Channels != 2)
                {
                    throw new Exception($"Unsupported number of channels {reader.WaveFormat.Channels} in sample {fileName}");
                }
                if (reader.WaveFormat.BitsPerSample != 8 && reader.WaveFormat.BitsPerSample != 16 &&
                    reader.WaveFormat.BitsPerSample != 24)
                {
                    throw new Exception($"Unsupported number of bits per sample {reader.WaveFormat.Channels} in sample {fileName}");
                }

                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;
                int blockAlign = reader.WaveFormat.BlockAlign;

                //int GetPos(double milliSeconds)
                //{
                //    int pos = (int)milliSeconds * bytesPerMillisecond;
                //    return pos - pos % blockAlign;
                //}
                int ToInt24(byte[] value, int startIndex)
                {
                    return value[startIndex] | value[startIndex + 1] << 8 | (sbyte)value[startIndex + 2] << 16;
                }

                reader.Position = 0;

                samples = new byte[reader.Length];
                int read = reader.Read(samples, 0, samples.Length);
                int bytesPerFrame = reader.WaveFormat.Channels * reader.WaveFormat.BitsPerSample / 8;
                bool isStereo = reader.WaveFormat.Channels == 2;

                channel.Reset(read / bytesPerFrame, reader.WaveFormat.SampleRate);

                if (reader.WaveFormat.BitsPerSample == 8)
                {
                    for (int i = 0; i < read / bytesPerFrame; i++)
                    {
                        var intSampleValue = samples[i * bytesPerFrame];
                        channel.Data[i].Left = intSampleValue / 128.0d;
                        if (isStereo)
                        {
                            intSampleValue = samples[i * bytesPerFrame + 1];
                            channel.Data[i].Right = intSampleValue / 128.0d;
                        }
                        else
                        {
                            channel.Data[i].Right = channel.Data[i].Left;
                        }
                    }
                }
                else if (reader.WaveFormat.BitsPerSample == 16)
                {
                    for (int i = 0; i < read / bytesPerFrame; i++)
                    {
                        var intSampleValue = BitConverter.ToInt16(samples, i * bytesPerFrame);
                        channel.Data[i].Left = intSampleValue / 32768.0d;
                        if (isStereo)
                        {
                            intSampleValue = BitConverter.ToInt16(samples, i * bytesPerFrame + 2);
                            channel.Data[i].Right = intSampleValue / 32768.0d;
                        }
                        else
                        {
                            channel.Data[i].Right = channel.Data[i].Left;
                        }
                    }
                }
                else if (reader.WaveFormat.BitsPerSample == 24)
                {
                    for (int i = 0; i < read / bytesPerFrame; i++)
                    {
                        var intSampleValue = ToInt24(samples, i * bytesPerFrame);
                        channel.Data[i].Left = intSampleValue / 8388608.0d;
                        if (isStereo)
                        {
                            intSampleValue = ToInt24(samples, i * bytesPerFrame + 3);
                            channel.Data[i].Right = intSampleValue / 8388608.0d;
                        }
                        else
                        {
                            channel.Data[i].Right = channel.Data[i].Left;
                        }
                    }
                }

            }
        }

        public static int GetWavFileSampleLength(string fileName)
        {
            var inPath = System.IO.Path.Combine(Env.Song.SamplesPath, fileName);

            using (var reader = WaveStreamFromPath(inPath))
            {
                if (reader.WaveFormat.Channels != 1 && reader.WaveFormat.Channels != 2)
                {
                    throw new Exception($"Unsupported number of channels {reader.WaveFormat.Channels} in sample {fileName}");
                }
                if (reader.WaveFormat.BitsPerSample != 8 && reader.WaveFormat.BitsPerSample != 16 &&
                    reader.WaveFormat.BitsPerSample != 24)
                {
                    throw new Exception($"Unsupported number of bits per sample {reader.WaveFormat.Channels} in sample {fileName}");
                }

                int bytesPerFrame = reader.WaveFormat.Channels * reader.WaveFormat.BitsPerSample / 8;
                return (int)(reader.Length / bytesPerFrame);
            }
        }

        private static WaveStream WaveStreamFromPath(string inPath)
        {
            string fileName = Path.GetFileName(inPath);
            if (fileName.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase))
            {
                return new WaveFileReader(inPath);
            }
            //else if (fileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    return new Mp3FileReader(inPath);
            //}

            return new NullWaveStream();

            //throw new Exception($"Unsupported fileformat in file {inPath}");
        }
    }

    public class NullWaveStream : WaveStream
    {
        public NullWaveStream()
        {
        }

        public override WaveFormat WaveFormat => new WaveFormat((int)Env.Song.SampleRate, 2);

        public override long Length => 0;

        public override long Position 
        {
            get => 0;
            set { }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }
    }

}
