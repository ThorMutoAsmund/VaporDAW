using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawingBoxes
{
    public enum InputOutputType
    {
        Sample,
        NumberArray,
        Text,
    }

    //public class InputOutput
    //{
    //    [JsonProperty] public InputOutput Type { get; set; }
    //}

    public class Generator
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string ScriptId { get; set; }
        [JsonProperty] public Dictionary<string,object> Settings { get; set; }
        //[JsonProperty] public List<InputOutput> Inputs { get; set; }
        //[JsonProperty] public List<InputOutput> Outputs { get; set; }
    }

    public class ScriptRef
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string FileName { get; set; }
    }

    public class SampleRef
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string FileName { get; set; }
    }

    public class Part
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string ScriptId { get; set; }
        [JsonProperty] public string TrackId { get; set; }
        [JsonProperty] public double Start { get; set; }
        [JsonProperty] public double Length { get; set; }
        [JsonProperty] public string Title { get; set; }
        [JsonProperty] public List<Generator> Generators { get; set; }
    }

    public class Track
    {
        [JsonProperty] public string Id { get; set; }
        [JsonProperty] public string ScriptId { get; set; }
    }

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

        [JsonIgnore] public List<ScriptRef> ScriptRefs => this.Scripts;
        [JsonIgnore] public List<SampleRef> SampleRefs => this.Samples;
        [JsonIgnore] public bool ChangesMade { get; private set; } = false;

        public static event Action<bool> ProjectLoaded;

        public static event Action<ScriptRef> RequestEditScript;

        private string projectPath;
        [JsonIgnore] public string ProjectFilePath => Path.Combine(this.projectPath, $"{Env.ProjectFileName}.json");
        [JsonIgnore] public string ScriptsPath => Path.Combine(this.projectPath, Env.ScriptsFolder);
        [JsonIgnore] public string SamplesPath => Path.Combine(this.projectPath, Env.SamplesFolder);

        public static void CreateEmpty()
        {
            var projectPath = Path.Combine(Env.ApplicationPath, "default_project");
            if (Directory.Exists(projectPath))
            {
                Open(projectPath);
            }
            else
            {
                CreateNew(projectPath, "Autocreated song", 10, 44100d, 120d);
            }
        }

        public static void CreateNew(string projectPath, string songName, int numberOfTracks, double sampleFrequency, double songLength)
        {
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
                    ScriptId = trackScriptRef.Id
                });
            }

            Env.Song.Save();

            Env.AddRecentFile(projectPath);

            Song.ProjectLoaded?.Invoke(true);
        }

        public static void Open(string projectPath)
        {
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
            var scriptRef = this.ScriptRefs.FirstOrDefault(s => s.Id == scriptId);

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
            if (Env.Song == null)
            {
                return null;
            }

            // Create script classes
            var processEnv = new ProcessEnv().CreateFrom(Env.Song);
                        
            return Task.Run(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // Create script classes
                processEnv.InitProcessors();

                var processParams = new ProcessParams(processEnv, startTime, length);
                processEnv.Process(processParams);
                
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
                this.ChangesMade = false;
            }
            finally
            {
                Cursor.Current = storeCursor;
            }
        }

        public static void Close()
        {
            Env.Song = null;

            Song.ProjectLoaded?.Invoke(false);
        }

        public Part AddPart(System.Windows.Point p, string name = null)
        {
            var start = p.X * Env.CanvasTimePerPixel + Env.CanvasStartTime;
            var trackNo = (int)(p.Y / Env.TrackHeight);
            var length = Env.PartLength;

            var partScriptRef = Env.Song.FindOrAddScript(Env.DefaultPartScriptName, Env.PartTemplateScriptName);

            var part = new Part()
            {
                Id = Base64.UUID(),
                ScriptId = partScriptRef.Id,
                Start = start,
                Length = length,
                Title = name ?? Env.DefaultPartTitle,
                Generators = new List<Generator>()
            };

            this.Parts.Add(part);

            ChangePartTrack(part, trackNo);

            this.ChangesMade = true;

            return part;
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
            this.ChangesMade = true;

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
                var script = new ScriptRef()
                {
                    Id = Base64.UUID(),
                    FileName = scriptName
                };

                this.Scripts.Add(script);
                this.ChangesMade = true;
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
            this.ChangesMade = true;

            return true;
        }

        public SampleRef FindSample(string sampleName)
        {
            var sampleRef = this.SampleRefs.FirstOrDefault(sr => sr.FileName == sampleName);
            if (sampleRef == null)
            {
                sampleRef = new SampleRef()
                {
                    Id = Base64.UUID(),
                    FileName = sampleName
                };
                this.Samples.Add(sampleRef);
                this.ChangesMade = true;
            }

            return sampleRef;
        }

        public void AddSampleToPart(Part part, string sampleName)
        {
            var sampleScriptRef = Env.Song.FindOrAddScript(Env.DefaultSampleScriptName, Env.SampleTemplateScriptName);

            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = sampleScriptRef.Id,
                Settings = new Dictionary<string, object>()
                {
                    { "sampleId", FindSample(sampleName).Id }
                }
            };
            part.Generators.Add(generator);
            this.ChangesMade = true;
        }

        public void AddScriptToPart(Part part, string scriptName)
        {
            var scriptRef = Env.Song.FindScript(scriptName);

            var generator = new Generator()
            {
                Id = Base64.UUID(),
                ScriptId = scriptRef.Id,
            };
            part.Generators.Add(generator);
            this.ChangesMade = true;
        }

        public void ChangePartTrack(Part part, int trackNo)
        {
            if (trackNo < 0 || trackNo >= this.Tracks.Count())
            {
                trackNo = 0;
            }

            part.TrackId = this.Tracks[trackNo].Id;

            this.ChangesMade = true;
        }

        public int GetPartTrackNo(Part part)
        {
            var index = this.Tracks.FindIndex(track => track.Id == part.TrackId);
            return index > -1 ? index : 0; 
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
            this.ChangesMade = true;

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
