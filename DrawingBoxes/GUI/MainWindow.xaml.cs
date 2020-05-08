using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace VaporDAW
{
    public partial class MainWindow : Window
    {
        private string title;

        public MainWindow()
        {
            this.FontFamily = new System.Windows.Media.FontFamily("Arial");

            InitializeComponent();

            SetTitle();

            Env.TimePerPixel = 0.01m;
            Env.MainWindow = this;
            Env.TrackPanel = this.trackPanel;

            this.Closing += (sender, e) => e.Cancel = !Env.ConfirmChangesMade();

            this.fileMenu.SubmenuOpened += (__, _) => FileMenuSubmenuOpened();
            this.newScriptMenu.Click += (__, _) => NewScript();
            this.importSamplesMenu.Click += (__, _) => ImportSamples();
            this.generateOutputMenu.Click += (__, _) => GenerateOutput();
            this.addTrackMenuItem.Click += (__, _) => AddTrack();
            this.zoomInButton.Click += (__, _) => Zoom(true);
            this.zoomOutButton.Click += (__, _) => Zoom(false);
            Song.RequestEditScript += script => OpenScriptTab(script);
            Song.ProjectLoaded += loaded => ProjectLoaded(loaded);
            Song.ChangeStateChanged += UpdateTitle;

            this.scrollViewer.ScrollChanged += (object sender, ScrollChangedEventArgs e) =>
            {
                if (Env.Song != null)
                {
                    Env.OnViewChanged();
                }
            };

            bool isActivated = false;
            this.Activated += (sender, e) =>
            {
                if (!isActivated)
                {
                    Song.CreateEmpty();
                    isActivated = true;
                }
            };

            this.SizeChanged += (sender, e) =>
            {
                if (isActivated)
                {
                    //foreach (FrameworkElement element in this.canvas.Children)
                    //{
                    //    element.InvalidateVisual();
                    //}
                }
            };

            GuiManager.Create(this, this.trackHeadPanel, this.trackPanel);
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e) => NewProject();
        private void OnOpen(object sender, ExecutedRoutedEventArgs e) => OpenProject();
        private void OnClose(object sender, ExecutedRoutedEventArgs e) => CloseProject();
        private void OnSave(object sender, ExecutedRoutedEventArgs e) => SaveProject();
        private void OnExit(object sender, ExecutedRoutedEventArgs e) => this.Close();
        private void OnCloseTab(object sender, ExecutedRoutedEventArgs e) => CloseTab();
        

        private void SetTitle(string projectName = null)
        {
            this.title = $"{(String.IsNullOrEmpty(projectName) ? String.Empty : $"{projectName} - ")}{Env.ApplicationName}";
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            this.Title = $"{title}{(Song.ChangesMade ? " *" : String.Empty)}";
        }


        private void NewProject()
        {
            if (!Env.ConfirmChangesMade())
            {
                return;
            }

            if (Dialogs.BrowseFolder("Select a new empty folder", Env.LastProjectPath ?? Env.ApplicationPath, out var selectedPath))
            {
                var dialog = NewProjectDialog.Create(this, selectedPath);
                if (dialog.ShowDialog() ?? false)
                {
                    Song.CreateNew(selectedPath, dialog.SongName, dialog.NumberOfTracks, dialog.SampleFrequency, dialog.SongLength);
                }
            }
        }

        private void OpenProject(string path = null)
        {
            if (!Env.ConfirmChangesMade())
            {
                return;
            }

            if (String.IsNullOrEmpty(path))
            {
                if (Dialogs.BrowseFolder("Select an existing project folder", Env.LastProjectPath ?? Env.ApplicationPath, out var selectedPath))
                {
                    Song.Open(selectedPath);
                }
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    MessageBox.Show("Selected path not found");
                    return;
                }

                Song.Open(path);
            }
        }

        private void FileMenuSubmenuOpened()
        {
            this.recentFilesMenu.Items.Clear();

            foreach (var recentFilePath in Env.RecentFiles)
            {
                var menuItem = new MenuItem()
                {
                    Header = recentFilePath,
                };
                menuItem.Click += (__, _) => OpenProject(recentFilePath);
                this.recentFilesMenu.Items.Add(menuItem);
            }
        }

        private void NewScript()
        {
            if (Env.Song == null)
            {
                return;
            }

            var newScriptName = Env.Song.GetNextAvailableScriptName();
            var dialog = EditStringDialog.Create(this, "Enter Script Name", "Script name", newScriptName);
            if (dialog.ShowDialog() ?? false)
            {
                Env.Song.AddScript(dialog.Text);
            }
        }

        private void CloseProject()
        {
            if (Env.Song == null)
            {
                return;
            }

            if (!Env.ConfirmChangesMade())
            {
                return;
            }

            // Close song
            Song.Close();

            // Remove tabs
            foreach (var scriptTabItem in this.tabControl.Items.WhereIs<ScriptTabItem>().ToArray())
            {
                this.tabControl.Items.Remove(scriptTabItem);
            }
            this.tabControl.SelectedIndex = 0;
        }

        private void SaveProject()
        {
            if (Env.Song == null)
            {
                return;
            }

            // Save tabs
            foreach (var scriptTabItem in this.tabControl.Items.WhereIs<ScriptTabItem>())
            {
                if (scriptTabItem.HasChanges)
                {
                    Env.Song.UpdateScriptContent(scriptTabItem.Script, scriptTabItem.ScriptContent);
                    scriptTabItem.ClearHasChanges();
                }
            }

            Env.Song.Save();
        }

        private void CloseTab(ScriptTabItem scriptTabItem = null)
        {
            // Close tab
            if (scriptTabItem == null)
            {
                scriptTabItem = this.tabControl.SelectedItem as ScriptTabItem;
            }

            if (scriptTabItem != null)
            {
                if (scriptTabItem.HasChanges)
                {
                    if (MessageBox.Show("Changes have been made. Close tab without saving?", "Changes Made", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                this.tabControl.Items.RemoveAt(this.tabControl.SelectedIndex);
            }
        }

        private void ProjectLoaded(bool loaded)
        {
            if (!loaded)
            {
                this.tabControl.IsEnabled = false;
                this.saveMenu.IsEnabled = false;
                this.closeMenu.IsEnabled = false;
                this.toolsMenu.IsEnabled = false;

                SetTitle();
            }
            else
            {
                this.tabControl.IsEnabled = true;
                this.saveMenu.IsEnabled = true;
                this.closeMenu.IsEnabled = true;
                this.toolsMenu.IsEnabled = true;
                SetTitle(Env.Song.SongName);
            }
        }

        private void OpenScriptTab(ScriptRef script)
        {
            if (Env.Song == null)
            {
                return;
            }

            // Check if already open
            foreach (var scriptTabItem in this.tabControl.Items.WhereIs<ScriptTabItem>())
            {
                if (scriptTabItem.Script == script)
                {
                    this.tabControl.SelectedIndex = this.tabControl.Items.IndexOf(scriptTabItem);
                    return;
                }
            }

            // Else create new tab item and select it
            var newScriptTabItem = new ScriptTabItem(script, Env.Song.ReadScriptContent(script));
            this.tabControl.Items.Insert(this.tabControl.Items.Count, newScriptTabItem);

            newScriptTabItem.RequestClose += () => this.CloseTab(newScriptTabItem);

            this.tabControl.SelectedIndex = this.tabControl.Items.Count - 1;
        }

        private void ImportSamples()
        {
            if (Env.Song == null)
            {
                return;
            }

            string[] selectedFiles;
            if (Dialogs.BrowseFiles("Select samples ot import", Env.ApplicationPath, out selectedFiles))
            {
                var cnt = 0;
                foreach (var sourceFilePath in selectedFiles)
                {
                    if (Env.Song.AddSample(sourceFilePath))
                    {
                        cnt++;
                    }
                }
                if (selectedFiles.Length != cnt)
                {
                    MessageBox.Show($"Successfully imported {cnt} file(s). {selectedFiles.Length - cnt} files could not be imported.");
                }
                else
                {
                    MessageBox.Show($"Successfully imported {cnt} file(s).");
                }
            }
        }

        private void GenerateOutput()
        {
            if (Env.Song == null)
            {
                return;
            }

            var task = Env.Song.Generate();

            task.ContinueWith(taskResult =>
            {
                var result = taskResult.Result;

                Console.WriteLine($"Generate async task finished in {result.ElapsedMilliseconds} ms");
            });
        }

        private void AddTrack()
        {
            if (Env.Song == null)
            {
                return;
            }

            Env.Song.AddTrack();
        }

        private void Zoom(bool zoomIn)
        {
            var stringRep = Env.TimePerPixel.ToString();
            Env.TimePerPixel = zoomIn ?
                (stringRep.Contains("5") ? Env.TimePerPixel / 5 * 2 : Env.TimePerPixel / 2) :
                (stringRep.Contains("2") ? Env.TimePerPixel / 2 * 5 : Env.TimePerPixel * 2);
            GuiManager.Instance.RedrawSong();
            Env.OnViewChanged();
        }
    }
}
