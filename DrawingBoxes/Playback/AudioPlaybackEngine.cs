using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public delegate double GetPositionDelegate();
    public class AudioPlaybackEngine : IDisposable
    {
        public event Action<TimeSpan> TimeUpdated;

        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);

        private AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            var waveOutEvent = new WaveOutEvent();
            this.outputDevice = waveOutEvent;
            this.mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            this.mixer.ReadFully = true;
            this.outputDevice.Init(mixer);
            this.outputDevice.Play();

            this.GetPosition = () => waveOutEvent.GetPosition();
        }

        public GetPositionDelegate GetPosition { get; private set; } = () => 0d;

        public void StopPlayback()
        {
            this.mixer.RemoveAllMixerInputs();
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == this.mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && this.mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        //public void PlaySound(CachedSound sound)
        //{
        //    AddMixerInput(new CachedSoundSampleProvider(sound));
        //}
        public void PlaySound(string fileName)
        {
            AddMixerInput(new AutoDisposeFileReader(new AudioFileReader(fileName)), 0d);
        }

        public void PlaySound(Channel channel, double startTime)
        {
            AddMixerInput(new ChannelSampleProvider(channel), startTime);
        }

        private void AddMixerInput(IPositionedSampleProvider sampleProvider, double startTime)
        {
            sampleProvider.PositionUpdated += position =>
            {
                var t = position / Env.Song.SampleRate + startTime;
                this.TimeUpdated?.Invoke(TimeSpan.FromSeconds(t));
            };
            this.mixer.AddMixerInput(ConvertToRightChannelCount(sampleProvider));
        }

        public void Dispose()
        {
            this.outputDevice.Dispose();
        }
    }
}
