using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

            //var trackProcessors = new List<Processor>();

            this.Processors = new Dictionary<string, Processor>() { { this.Mixer.ElementId, this.Mixer } };
            song.Tracks.ForEach(track => this.Processors[track.Id] = song.CreateProcessor(this, track.ScriptId, track.Id));
            song.Parts.ForEach(part => this.Processors[part.Id] = song.CreateProcessor(this, part.ScriptId, part.Id));
            song.Parts.SelectMany(part => part.Generators.Select(generator => new { part, generator })).
                ToList().ForEach(partAndGenerator => this.Processors[partAndGenerator.generator.Id] = song.CreateProcessor(this, partAndGenerator.generator.ScriptId, partAndGenerator.generator.Id, part: partAndGenerator.part));

            return this;
        }

        public Task<GenerateResult> Generate(float startTime, float length)
        {
            return Task.Run(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // Create script classes
                if (InitProcessors())
                {
                    var dependanceTree = new Dictionary<string, List<string>>();
                    if (BuildDependanceTree(dependanceTree))
                    {
                        var processParams = new ProcessParams(this, startTime, length);
                        Process(dependanceTree, processParams);
                    }
                }

                watch.Stop();

                return new GenerateResult(this.Mixer.GetOutputChannel(Tags.MainOutput), watch.ElapsedMilliseconds);
            });

        }

        /// <summary>
        /// Initialize all processors before Process is called
        /// </summary>
        /// <returns></returns>
        public bool InitProcessors()
        {
            foreach (var processor in this.Processors.Values)
            {
                try
                {
                    processor.Init();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Processor {processor.ElementId} failed init: {e.Message}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Value is the generators depending on Key
        /// </summary>        
        public bool BuildDependanceTree(Dictionary<string, List<string>> dependanceTree)
        {
            void Recurse(Processor processor)
            {
                if (!dependanceTree.ContainsKey(processor.ElementId))
                {
                    dependanceTree[processor.ElementId] = new List<string>();
                }

                foreach (var input in processor.Inputs)
                {
                    if (!dependanceTree[processor.ElementId].Contains(input.Provider.ElementId))
                    {
                        dependanceTree[processor.ElementId].Add(input.Provider.ElementId);
                        Recurse(input.Provider);
                    }
                }
            }

            Recurse(this.Mixer);

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
                    this.Processors[elementId].Process(processParams);
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
