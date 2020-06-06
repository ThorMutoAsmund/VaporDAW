using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace VaporDAW
{
    public static class Dialogs
    {
        public static bool BrowseFolder(string description,  string initialPath, out string selectedPath)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.SelectedPath = initialPath;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                selectedPath = dialog.SelectedPath;

                return result == System.Windows.Forms.DialogResult.OK;
            }
        }

        public static bool BrowseFiles(string description, string initialPath, out string[] selectedFiles)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = description;
                dialog.InitialDirectory = initialPath;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                selectedFiles = dialog.FileNames;

                return result == System.Windows.Forms.DialogResult.OK;
            }
        }

        public static ScriptRef AddNewScript(Window owner)
        {
            var newScriptName = Env.Song.GetNextAvailableScriptName();
            var dialog = EditStringDialog.Create(owner, "Enter Script Name", "Script name", newScriptName);
            if (dialog.ShowDialog() ?? false)
            {
                var text = dialog.Text;
                if (!text.EndsWith(".cs"))
                {
                    text += ".cs";
                }
                return Env.Song.AddScript(text);
            }

            return null;
        }
    }
}
