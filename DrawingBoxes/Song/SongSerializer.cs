using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporDAW
{
    public class SongSerializer
    {
        public static int CurrentVersion = 1;

        public Song Song { get; set; }

        public SongSerializer()
        {
        }

        private void WriteToFile(string projectFilePath)
        {
            this.Song.Ver = CurrentVersion;

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            File.WriteAllText(projectFilePath, json);
        }

        public static SongSerializer FromFile(string projectFilePath)
        {
            var json = File.ReadAllText(projectFilePath);
            return JsonConvert.DeserializeObject<SongSerializer>(json);
        }

        public static void ToFile(Song song)
        {
            new SongSerializer()
            {
                Song = song,
            }.WriteToFile(song.ProjectFilePath);
        }
    }
}
