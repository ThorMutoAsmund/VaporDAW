﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VaporDAW
{
    public class Song
    {
        [JsonProperty] public string ScriptId { get; set; } // Mixer
        [JsonProperty] public int Ver { get; set; }
        [JsonProperty] public DateTime CreationDate { get; set; }
        [JsonProperty] public DateTime ChangeDate { get; set; }
        [JsonProperty] public string SongName { get; set; }
        [JsonProperty] public List<Track> Tracks { get; set; }
        [JsonProperty] public List<Part> Parts { get; set; }
        [JsonProperty] public List<ScriptRef> Scripts { get; set; }
        [JsonProperty] public List<SampleRef> Samples { get; set; }
        [JsonProperty] public double SampleFrequency { get; set; }
        [JsonProperty] public double SongLength { get; set; }

        [JsonIgnore] public string ProjectFilePath => Path.Combine(this.projectPath, $"{Env.ProjectFileName}.json");
        [JsonIgnore] public string ScriptsPath => Path.Combine(this.projectPath, Env.ScriptsFolder);
        [JsonIgnore] public string SamplesPath => Path.Combine(this.projectPath, Env.SamplesFolder);

        public static bool ChangesMade
        {
            get => changesMade;
            set
            {
                if (value != changesMade)
                {
                    changesMade = value;
                    ChangeStateChanged?.Invoke();
                }
            }
        }
        public static event Action ClearVolatile;
        public static event Action ChangeStateChanged;

        public static event Action<bool> ProjectLoaded;
        public static event Action<ScriptRef> RequestEditScript;
        
        public static event Action<Part> PartAdded; 
        public static event Action<Part> PartChanged;
        public static event Action<Part> PartDeleted;

        public static event Action<Track> TrackAdded;
        public static event Action<Track> TrackChanged;
        
        private static bool changesMade = false;
        private string projectPath;

        public static void CreateEmpty()
        {
            var projectPath = Path.Combine(Env.ApplicationPath, "default_project");
            if (Directory.Exists(projectPath))
            {
                Open(projectPath);
            }
            else
            {
                CreateNew(projectPath, "Autocreated song", 4, 44100d, 120d);
            }
        }

        public static void CreateNew(string projectPath, string songName, int numberOfTracks, double sampleFrequency, double songLength)
        {
            Song.ChangesMade = false;
            Song.ClearVolatile?.Invoke();
            
            Env.Song = new Song()
            {
                projectPath = projectPath,
                
                CreationDate = DateTime.UtcNow,
                ChangeDate = DateTime.UtcNow,
                SongName = songName,
                Tracks = new List<Track>(numberOfTracks),
                Parts = new List<Part>(),
                Scripts = new List<ScriptRef>(),
                Samples = new List<SampleRef>(),
                SampleFrequency = sampleFrequency,
                SongLength = songLength                
            };

            CreateFolders(projectPath);

            var scriptRef = Env.Song.AddScript(Env.DefaultSongScriptName, Env.SongTemplateScriptName, false);
            Env.Song.ScriptId = scriptRef.Id;

            var trackScriptRef = Env.Song.AddScript(Env.DefaultTrackScriptName, Env.TrackTemplateScriptName, false);

            for (int t = 0; t < numberOfTracks; ++t)
            {
                Env.Song.Tracks.Add(new Track()
                {
                    Id = Base64.UUID(),
                    Title = $"Track{t+1}",
                    ScriptId = trackScriptRef.Id
                });
            }

            Env.Song.Save();

            Env.AddRecentFile(projectPath);

            Song.ProjectLoaded?.Invoke(true);
        }

        public static void Open(string projectPath)
        {
            Song.ChangesMade = false;
            Song.ClearVolatile?.Invoke();
 
            var songSerializer = SongSerializer.FromFile(new Song() { projectPath = projectPath }.ProjectFilePath);

            Env.Song = songSerializer.Song;
            Env.Song.projectPath = projectPath;

            Env.AddRecentFile(projectPath);

            Song.ProjectLoaded?.Invoke(true);
        }

        private static void CreateFolders(string path)
        {
            // Ensure script directory
            var scriptDirectory = Path.Combine(path, Env.ScriptsFolder);
            if (!Directory.Exists(scriptDirectory))
            {
                Directory.CreateDirectory(scriptDirectory);
            }

            // Ensure sample directory
            var sampleDirectory = Path.Combine(path, Env.SamplesFolder);
            if (!Directory.Exists(sampleDirectory))
            {
                Directory.CreateDirectory(sampleDirectory);
            }
        }

        public Processor CreateProcessor(string scriptId, string id)
        {
            var scriptRef = GetScriptRef(scriptId);

            if (scriptRef == null)
            {
                return Processor.Empty;
            }

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var className = scriptRef.FileName;
            var subStringLength = className.IndexOf(".");
            if (subStringLength > -1)
            {
                className = className.Substring(0, subStringLength);
            }
            var type = assembly.GetTypes().First(t => t.Name == className);
            if (type == null)
            {
                return Processor.Empty;
            }

            var processor = Activator.CreateInstance(type) as Processor ?? Processor.Empty;
            processor.SetElementId(id);

            return processor;
        }

        public Task<GenerateResult> Generate(StandardAudioFormat audioFormat = StandardAudioFormat.PCM, float startTime = 0f, float length = 5f)
        {
            // Create script classes
            var processEnv = new ProcessEnv().CreateFrom(Env.Song);
                        
            return Task.Run(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // Create script classes
                if (processEnv.InitProcessors())
                {
                    var processParams = new ProcessParams(processEnv, startTime, length);
                    processEnv.Process(processParams);
                }

                watch.Stop();

                return new GenerateResult(processEnv.Mixer.GetOutput(Tags.MainOutput), watch.ElapsedMilliseconds);
            });
        }

        public void Save()
        {
            var storeCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                SongSerializer.ToFile(this);
                Song.ChangesMade = false;
            }
            finally
            {
                Cursor.Current = storeCursor;
            }
        }

        public static void Close()
        {
            ClearVolatile();

            Env.Song = null;

            Song.ChangesMade = false;
            Song.ProjectLoaded?.Invoke(false);
        }

        public Track AddTrack()
        {
            var trackScriptRef = Env.Song.FindOrAddScript(Env.DefaultTrackScriptName, Env.TrackTemplateScriptName);
            var track = new Track()
            {
                Id = Base64.UUID(),
                Title = $"Track{this.Tracks.Count() + 1}",
                ScriptId = trackScriptRef.Id
            };
            this.Tracks.Add(track);

            Song.ChangesMade = true;
            Song.TrackAdded?.Invoke(track);

            return track;
        }

        public void OnTrackChanged(Track track)
        {
            Song.TrackChanged?.Invoke(track);
            Song.ChangesMade = true;
        }

        public Part AddPart(Track track, System.Windows.Point? point = null, string title = null)
        {
            if (point == null)
            {
                point = new System.Windows.Point(0d, 0d);
            }

            var start = point.Value.X * (double)Env.TimePerPixel;
            var length = Env.PartLength;

            var partScriptRef = Env.Song.FindOrAddScript(Env.DefaultPartScriptName, Env.PartTemplateScriptName);

            var part = new Part()
            {
                Id = Base64.UUID(),
                ScriptId = partScriptRef.Id,
                Start = start,
                Length = length,
                Title = title ?? Env.DefaultPartTitle,
                Generators = new List<Generator>(),
                TrackId = track.Id
            };

            this.Parts.Add(part);

            Song.ChangesMade = true;
            Song.PartAdded?.Invoke(part);

            return part;
        }

        public Part RefClonePart(Part original, double start, Track track)
        {
            var part = new Part()
            {
                Id = Base64.UUID(),
                RefId = original.Id,
                Start = start,
                TrackId = track.Id
            };

            this.Parts.Add(part);

            Song.ChangesMade = true;
            Song.PartAdded?.Invoke(part);

            return part;
        }

        public Part ClonePart(Part original, double start, Track track)
        {
            var part = new Part()
            {
                Id = Base64.UUID(),
                ScriptId = original.ScriptId,
                Start = start,
                Length = original.Length,
                Title = original.Title,
                Generators = original.Generators.Select(g => g.Clone()).ToList(),
                TrackId = track.Id
            };

            this.Parts.Add(part);

            Song.ChangesMade = true;
            Song.PartAdded?.Invoke(part);

            return part;
        }

        public void OnPartChanged(Part part)
        {
            Song.PartChanged?.Invoke(part);
            Song.ChangesMade = true;
        }

        public void DeletePart(Part part)
        {
            if (this.Parts.Remove(part))
            {
                Song.ChangesMade = true;
                Song.PartDeleted?.Invoke(part);
            }
        }

        public string GetNextAvailableScriptName()
        {
            int i = 0;
            string newScriptName;
            var allScripts = Directory.EnumerateFiles(this.ScriptsPath, "*.cs").
                Select(x => Path.GetFileName(x)).ToArray();

            do
            {
                newScriptName = $"Script{++i}.cs";
            }
            while (allScripts != null && allScripts.Any(s => s == newScriptName));

            return newScriptName;
        }

        public ScriptRef AddScript(string newScriptName, string templateScriptName = null, bool replaceName = false)
        {
            var filePath = Path.Combine(this.ScriptsPath, newScriptName);
            var content = Env.GetTemplateScriptContent(templateScriptName ?? Env.EmptyTemplateScriptName);
            if (replaceName)
            {
                content = content.Replace("__CLASS_NAME__", newScriptName.Replace(".cs", string.Empty).ToUpperFirst());
            }
            File.WriteAllText(filePath, content);

            var script = new ScriptRef()
            {
                Id = Base64.UUID(),
                FileName = newScriptName
            };

            this.Scripts.Add(script);
            Song.ChangesMade = true;

            return script;
        }

        public ScriptRef FindOrAddScript(string newScriptName, string templateScriptName)
        {
            var scriptRef = this.Scripts.FirstOrDefault(script => script.FileName == newScriptName);

            if (scriptRef == null)
            {
                scriptRef = AddScript(newScriptName, templateScriptName, false);
            }

            return scriptRef;
        }

        public ScriptRef FindScript(string scriptName)
        {
            var scriptRef = this.Scripts.FirstOrDefault(script => script.FileName == scriptName);

            if (scriptRef == null)
            {
                scriptRef = new ScriptRef()
                {
                    Id = Base64.UUID(),
                    FileName = scriptName
                };

                this.Scripts.Add(scriptRef);
                Song.ChangesMade = true;
            }

            return scriptRef;
        }

        public bool AddSample(string fileName)
        {
            var sampleName = Path.GetFileName(fileName);
            var destinationFilePath = Path.Combine(Env.Song.SamplesPath, sampleName);
            if (File.Exists(destinationFilePath))
            {
                return false;
            }
            try
            {
                File.Copy(fileName, destinationFilePath);
            }
            catch (Exception)
            {
                //MessageBox.Show($"Error copying file {sampleName}");
                return false;
            }

            var sampleRef = new SampleRef()
            {
                Id = Base64.UUID(),
                FileName = sampleName
            };

            this.Samples.Add(sampleRef);
            Song.ChangesMade = true;

            return true;
        }

        public SampleRef FindSample(string sampleName)
        {
            var sampleRef = this.Samples.FirstOrDefault(sr => sr.FileName == sampleName);
            if (sampleRef == null)
            {
                sampleRef = new SampleRef()
                {
                    Id = Base64.UUID(),
                    FileName = sampleName
                };
                this.Samples.Add(sampleRef);
                Song.ChangesMade = true;
            }

            return sampleRef;
        }

        public void EditScript(string fileName)
        {
            foreach (var existingScript in this.Scripts)
            {
                if (existingScript.FileName == fileName)
                {
                    Song.RequestEditScript?.Invoke(existingScript);
                    return;
                }
            }

            // Create new script
            var script = new ScriptRef()
            {
                Id = Base64.UUID(),
                FileName = fileName
            };

            this.Scripts.Add(script);
            Song.ChangesMade = true;

            Song.RequestEditScript?.Invoke(script);
        }

        public void UpdateScriptContent(ScriptRef script, string content)
        {
            try
            {
                File.WriteAllText(Path.Combine(this.ScriptsPath, script.FileName), content);
            }
            catch (Exception)
            {
                // TBD
            }
        }
        public string ReadScriptContent(ScriptRef script)
        {
            try
            {
                return File.ReadAllText(Path.Combine(this.ScriptsPath, script.FileName));
            }
            catch (Exception)
            {
                // TBD
            }

            return string.Empty;
        }

        public ScriptRef GetScriptRef(string id)
        {
            return this.Scripts.FirstOrDefault(s => s.Id == id);
        }

        public SampleRef GetSampleRef(string id)
        {
            return this.Samples.FirstOrDefault(s => s.Id == id);
        }
        public Part GetPart(string id)
        {
            return this.Parts.FirstOrDefault(s => s.Id == id);
        }
    }

    public class GenerateResult
    {
        //public string FileName { get; private set; }

        //public GenerateResult()
        //{
        //    this.FileName = Path.GetRandomFileName();
        //}
        public Channel Channel { get; private set; }

        public long ElapsedMilliseconds { get; private set; }
        public GenerateResult(Channel channel, long elapsedMilliseconds)
        {
            this.Channel = channel;
            this.ElapsedMilliseconds = elapsedMilliseconds;
        }
    }
}
