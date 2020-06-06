using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VaporDAW
{
    public class ProcessEnv
    {
        public Song Song { get; private set; }
        public Dictionary<string, Processor> Processors { get; private set; }

        public Channel EmptyChannel { get; private set; }

        private Processor Mixer { get; set; }

        public ProcessEnv CreateFrom(Song song)
        {
            this.Song = song;
            this.Mixer = song.CreateProcessor(this, song.ScriptId, "SONG");
            this.EmptyChannel = new Channel(this.Mixer, string.Empty);

            this.Processors = new Dictionary<string, Processor>() { { this.Mixer.GeneratorId, this.Mixer } };

            song.Tracks.SelectMany(track => track.TrackGenerators.Select(generator => new { track, generator })).
                ToList().ForEach(trackAndGenerator => this.Processors[trackAndGenerator.generator.Id] = song.CreateProcessor(this, trackAndGenerator.generator.ScriptId, trackAndGenerator.generator.Id, track: trackAndGenerator.track));

            song.Parts.SelectMany(part => part.PartGenerators.Select(generator => new { part, generator })).
                ToList().ForEach(partAndGenerator => this.Processors[partAndGenerator.generator.Id] = song.CreateProcessor(this, partAndGenerator.generator.ScriptId, partAndGenerator.generator.Id, part: partAndGenerator.part));

            song.Parts.SelectMany(part => part.Generators.Select(generator => new { part, generator })).
                ToList().ForEach(partAndGenerator => this.Processors[partAndGenerator.generator.Id] = song.CreateProcessor(this, partAndGenerator.generator.ScriptId, partAndGenerator.generator.Id, part: partAndGenerator.part));
            song.Samples.ForEach(sample => this.Processors[sample.Id] = song.CreateSampleDataProcessor(this, sample.Id));
            return this;
        }

        public Task<ProcessResult> Generate(int startSampleTime, int sampleLength)
        {
            return Task.Run(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                var processParams = new ProcessParams(this, startSampleTime, sampleLength);
                var dependanceTree = new Dictionary<string, List<string>>();
                if (BuildDependancyTree(dependanceTree, processParams))
                {
                    Process(dependanceTree, processParams);
                }

                watch.Stop();

                return new ProcessResult(this.Mixer.GetOutputChannel(Tags.MainOutput), watch.ElapsedMilliseconds);
            });

        }

        private class InitProcessorException : Exception 
        {
            public InitProcessorException(string message, Exception innerException) :
                base(message, innerException)
            {
            }
        }

        /// <summary>
        /// Value is the generators depending on Key
        /// </summary>        
        public bool BuildDependancyTree(Dictionary<string, List<string>> dependanceTree, ProcessParams processParams)
        {
            void Recurse(Processor processor)
            {
                if (!dependanceTree.ContainsKey(processor.GeneratorId))
                {
                    dependanceTree[processor.GeneratorId] = new List<string>();
                    try
                    {
                        processor.Init(processParams);
                    }
                    catch (Exception ex)
                    {
                        throw new InitProcessorException($"Processor {processor.GeneratorId} failed init: {ex.Message}", ex);
                    }

                    foreach (var input in processor.Inputs)
                    {
                        if (!dependanceTree[processor.GeneratorId].Contains(input.Provider.GeneratorId))
                        {
                            dependanceTree[processor.GeneratorId].Add(input.Provider.GeneratorId);
                            Recurse(input.Provider);
                        }
                    }
                }
            }

            try
            {
                Recurse(this.Mixer);
            }
            catch (InitProcessorException e)
            {
                MessageBox.Show(e.Message, "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unhandled initializationfailure: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // TBD check for loops
            // 

            return true;
        }

        private void Process(Dictionary<string, List<string>> dependanceTree, ProcessParams processParams)
        {
            while (dependanceTree.Count > 0)
            {
                foreach (string elementId in dependanceTree.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToArray())
                {
                    var processor = this.Processors[elementId];
                    processor.ProcessResult = processor.Process(processParams);

                    dependanceTree.Remove(elementId);
                    foreach (var kvp in dependanceTree)
                    {
                        if (kvp.Value.Contains(elementId))
                        {
                            kvp.Value.Remove(elementId);
                        }
                    }
                }
            }
        }
    }
}
