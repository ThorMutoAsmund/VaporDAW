﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class Part
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string RefId { get; set; }
        [JsonProperty] public string TrackId { get; set; }
        [JsonProperty] public double Start { get; set; }
        [JsonProperty] public double Length
        {
            get => this.IsReference ? this.ReferencedPart.Length : this.length;
            set { this.length = value; }
        }
        public bool ShouldSerializeLength() => !this.IsReference;

        //[JsonProperty] public string ScriptId 
        //{
        //    get => this.IsReference ? this.ReferencedPart.ScriptId : this.scriptId;
        //    set { this.scriptId = value; }
        //}
        //public bool ShouldSerializeScriptId() => !this.IsReference;
        [JsonProperty] public List<Generator> PartGenerators { get; set; } = new List<Generator>();
        public bool ShouldSerializePartGenerators() => !this.IsReference;

        [JsonProperty] public string Title
        {
            get => this.IsReference ? $"[{this.ReferencedPart.Title}]" : this.title;
            set { this.title = value; }
        }
        public bool ShouldSerializeTitle() => !this.IsReference;

        [JsonProperty] public List<Generator> Generators { get; set; } = new List<Generator>();
        public bool ShouldSerializeGenerators() => !this.IsReference;

        [JsonIgnore] public bool IsReference => !string.IsNullOrEmpty(this.RefId);
        [JsonIgnore] public double End => this.Start + this.Length;
        [JsonIgnore] public int SampleLength => (int)(this.Length * Env.Song.SampleFrequency);

        private Part referencedPart;
        private Part ReferencedPart
        {
            get
            {
                if (this.referencedPart == null)
                {
                    this.referencedPart = Env.Song.GetPart(this.RefId);
                }
                return this.referencedPart;
            }
        }

        private double length;
        private string title;

        public void ChangeTrack(string trackId)
        {
            this.TrackId = trackId;

            Song.ChangesMade = true;
        }

        public void AddPartScript(ScriptRef scriptRef)
        {
            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = scriptRef.Id,
            };
            this.PartGenerators.Add(generator);
            Song.ChangesMade = true;
        }

        public Generator AddSample(string sampleName)
        {
            var sampleScriptRef = Env.Song.FindOrAddScript(Env.DefaultSampleScriptName, Env.SampleTemplateScriptName);

            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = sampleScriptRef.Id,
                Settings = new Dictionary<string, object>()
                {
                    { Tags.SampleId, Env.Song.FindSample(sampleName).Id }
                }
            };
            this.Generators.Add(generator);
            Song.ChangesMade = true;

            return generator;
        }

        public void AddScript(string scriptName)
        {
            var scriptRef = Env.Song.FindScript(scriptName);

            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = scriptRef.Id,
            };
            this.Generators.Add(generator);
            Song.ChangesMade = true;
        }

        public int GetTrackNo()
        {
            var index = Env.Song.Tracks.FindIndex(track => track.Id == this.TrackId);
            return index > -1 ? index : 0;
        }
    }
}
