using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrawingBoxes
{
    public class Watchers
    {
        public List<string> SamplesList { get; private set; } = new List<string>();
        public List<string> ScriptsList { get; private set; } = new List<string>();

        public event Action<List<string>> SamplesListChanged;

        public event Action<List<string>> ScriptsListChanged;

        private FileSystemWatcher samplesWatcher;
        private FileSystemWatcher scriptsWatcher;

        public Watchers()
        {
            Song.ProjectLoaded += loaded =>
            {
                if (loaded)
                {
                    ConfigureWatchers();
                    RescanSamples();
                    RescanSripts();
                }
                else
                {
                    StopWatchers();
                }
            };
        }

        private void ConfigureWatchers()
        {
            // Samples
            var context = SynchronizationContext.Current;
            this.samplesWatcher = new FileSystemWatcher()
            {
                Path = Env.Song.SamplesPath,
                NotifyFilter = NotifyFilters.FileName,
                Filter = "*.wav",
            };
            this.samplesWatcher.Changed += (source, e) => context.Post(val => RescanSamples(), source);
            this.samplesWatcher.Created += (source, e) => context.Post(val => RescanSamples(), source);
            this.samplesWatcher.Deleted += (source, e) => context.Post(val => RescanSamples(), source);
            this.samplesWatcher.Renamed += (source, e) => context.Post(val => RescanSamples(), source);

            this.samplesWatcher.EnableRaisingEvents = true;

            // Scripts
            this.scriptsWatcher = new FileSystemWatcher()
            {
                Path = Env.Song.ScriptsPath,
                NotifyFilter = NotifyFilters.FileName,
                Filter = "*.cs",
            };
            this.scriptsWatcher.Changed += (source, e) => context.Post(val => RescanSripts(), source);
            this.scriptsWatcher.Created += (source, e) => context.Post(val => RescanSripts(), source);
            this.scriptsWatcher.Deleted += (source, e) => context.Post(val => RescanSripts(), source);
            this.scriptsWatcher.Renamed += (source, e) => context.Post(val => RescanSripts(), source);

            this.scriptsWatcher.EnableRaisingEvents = true;
        }

        private void StopWatchers()
        {
            this.samplesWatcher.EnableRaisingEvents = false;
            this.samplesWatcher = null;
            this.scriptsWatcher.EnableRaisingEvents = false;
            this.scriptsWatcher = null;

            this.SamplesList.Clear();
            this.ScriptsList.Clear();
        }

        private void RescanSamples()
        {
            try
            {
                this.SamplesList = Directory.EnumerateFiles(Env.Song.SamplesPath, "*.wav").Select(x => System.IO.Path.GetFileName(x)).ToList();
                this.SamplesListChanged?.Invoke(this.SamplesList);
            }
            catch (Exception)
            {
                this.SamplesList.Clear();
            }
        }

        private void RescanSripts()
        {
            try
            {
                this.ScriptsList = Directory.EnumerateFiles(Env.Song.ScriptsPath, "*.cs").Select(x => System.IO.Path.GetFileName(x)).ToList();
                this.ScriptsListChanged?.Invoke(this.ScriptsList);
            }
            catch (Exception)
            {
                this.ScriptsList.Clear();
            }
        }

    }
}
