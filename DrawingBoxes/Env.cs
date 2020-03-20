using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DrawingBoxes
{
    public static class Env
    {
        public static string ApplicationName { get; set; } = "Vapor DAW";
        public static string ApplicationPath => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string SamplesFolder => "Samples";
        public static string ScriptsFolder => "Scripts";
        public static string ProjectFileName => "Project";

        private static string TemplateFolder => "Templates";
        public static string LastProjectPath { get; set; }
        public static Window MainWindow { get; set; }
        public static Canvas Canvas { get; set; }

        public static string DefaultPartTitle { get; set; } = "Untitled";

        public static double CanvasStartTime { get; set; }

        public static double CanvasTimePerPixel { get; set; }

        //public static Conf Conf { get; set; } = new Conf()
        public static List<string> RecentFiles { get; } = new List<string>();
        public static double TrackHeight { get; set; } = 50d;
        public static double PartLength { get; set; } = 1f; // seconds

        public static Song Song { get; set; }
        public static bool ChangesMade => Env.Song?.ChangesMade ?? false;

        public static Watchers Watchers { get; private set; } = new Watchers();

        public static void AddRecentFile(string projectPath)
        {
            LastProjectPath = projectPath;

            if (!RecentFiles.Contains(projectPath))
            {
                RecentFiles.Add(projectPath);
            }
        }

        public static bool ConfirmChangesMade()
        {
            if (Env.ChangesMade)
            {
                if (MessageBox.Show("Changes have been made. Continue without saving?", "Changes Made", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetTemplateScriptContent(string scriptFileName)
        {
            try
            {
                var path = Path.Combine(Env.ApplicationPath, TemplateFolder, scriptFileName);
                return File.ReadAllText(path);
            }
            catch (Exception)
            {
            }
            return string.Empty;
        }

        public static string EmptyTemplateScriptName = "EmptyScript.cs";

        public static string SongTemplateScriptName = "DefaultSongScript.cs";
        public static string DefaultSongScriptName = "DefaultMixer.cs";

        public static string TrackTemplateScriptName = "DefaultTrackScript.cs";
        public static string DefaultTrackScriptName = "DefaultTrack.cs";

        public static string PartTemplateScriptName = "DefaultPartScript.cs";
        public static string DefaultPartScriptName = "DefaultPart.cs";

        public static string SampleTemplateScriptName = "DefaultSampleScript.cs";
        public static string DefaultSampleScriptName = "DefaultSample.cs";

    }
}
