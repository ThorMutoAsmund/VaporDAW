using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

            //Env.TimePerPixel = 0.01m;
            Env.SamplesPerPixel = 441;
            Env.MainWindow = this;
            Env.TrackPanel = this.trackPanel;
            Env.SongPanel = this.songPanel;

            this.timeCounterText.Background = new SolidColorBrush(Colors.TrackHead);
            this.fileMenu.SubmenuOpened += (sender, e) => FileMenuSubmenuOpened();
            this.songPanel.Zoom += Zoom;
            this.headButtons.Background = new SolidColorBrush(Colors.TrackHead);

            Song.RequestEditScript += script => OpenScriptTab(script);
            Song.ProjectLoaded += loaded => ProjectLoaded(loaded);
            Song.ChangeStateChanged += UpdateTitle;

            this.saveCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.closeCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.newScriptCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.importSamplesCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.importMP3FilesCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.playSongCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.stopSongCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.addTrackCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.zoomInCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;
            this.zoomOutCommand.CanExecute += (sender, e) => e.CanExecute = Env.Song != null;

            this.timeRuler.SetSelector += SetSelector;

            this.timeCounterBorder.BorderBrush = new SolidColorBrush(Colors.TrackHeadBorder);

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

            this.timeCounterText.MouseDown += (sender, e) => SetSelector(0, 0, true);

            GuiManager.Create(this, this.trackHeadPanel, this.trackPanel);
            ResourceMonitor.Create(this.cpuUsageTextBlock, this.ramUsageTextBlock);
            AudioPlaybackEngine.Instance.TimeUpdated += PlaybackTimeUpdated;
        }

        private string AppNameAndVersion => $"{Env.ApplicationName} {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

        private void SetTitle(string projectName = null)
        {
            this.title = $"{(String.IsNullOrEmpty(projectName) ? String.Empty : $"{projectName} - ")}{this.AppNameAndVersion}";
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            this.Title = $"{title}{(Song.ChangesMade ? " *" : String.Empty)}";
        }

        private void OnExit(object sender, ExecutedRoutedEventArgs e) => this.Close();

        private void OnNewProject(object sender, ExecutedRoutedEventArgs e)
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
                    Song.CreateNew(selectedPath, dialog.SongName, dialog.NumberOfTracks, dialog.SampleFrequency, (int)(dialog.SongLength * dialog.SampleFrequency));
                }
            }
        }

        private void OnOpenProject(object sender, ExecutedRoutedEventArgs e) => OpenProject();
        
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
                menuItem.Click += (sender, e) => OpenProject(recentFilePath);
                this.recentFilesMenu.Items.Add(menuItem);
            }
        }

        private void OnNewScript(object sender, ExecutedRoutedEventArgs e)
        {
            Dialogs.AddNewScript(this);
        }

        private void OnCloseProject(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Env.ConfirmChangesMade())
            {
                return;
            }

            Song.Close();

            // Remove tabs
            foreach (var scriptTabItem in this.tabControl.Items.WhereIs<ScriptTabItem>().ToArray())
            {
                this.tabControl.Items.Remove(scriptTabItem);
            }
            this.tabControl.SelectedIndex = 0;
        }

        private void OnSaveProject(object sender, ExecutedRoutedEventArgs e)
        {
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

        private void OnCloseTab(object sender, ExecutedRoutedEventArgs e) => CloseTab();
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
                SetTitle();
            }
            else
            {
                this.tabControl.IsEnabled = true;
                SetTitle(Env.Song.SongName);
                SetSelector(0, 0, true);
            }
        }

        private void OpenScriptTab(ScriptRef script)
        {
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

        private void OnImportSamples(object sender, ExecutedRoutedEventArgs e)
        {
            string[] selectedFiles;
            if (Dialogs.BrowseFiles("Select files to import", Env.ApplicationPath, out selectedFiles))
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
        private void OnImportMP3Files(object sender, ExecutedRoutedEventArgs e)
        {
            string[] selectedFiles;
            if (Dialogs.BrowseFiles("Select files to convert and import", Env.ApplicationPath, out selectedFiles, filter: "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*"))
            {
                var cnt = 0;
                foreach (var sourceFilePath in selectedFiles)
                {
                    if (Env.Song.AddMp3File(sourceFilePath))
                    {
                        cnt++;
                    }
                }

                if (selectedFiles.Length != cnt)
                {
                    MessageBox.Show($"Successfully converted and imported {cnt} file(s). {selectedFiles.Length - cnt} files could not be converted.");
                }
                else
                {
                    MessageBox.Show($"Successfully converted and imported {cnt} file(s).");
                }
            }
        }

        private void OnPlay(object sender, ExecutedRoutedEventArgs e) => PlaySong();

        private void PlaySong()
        {
            var songLength = Env.Song.GetActualSampleLength();
            if (this.selectSampleStart >= songLength)
            {
                return;
            }
            PlaySongRange(this.selectSampleStart, this.selectSampleLength == 0f ? songLength - this.selectSampleStart : this.selectSampleLength);
        }

        private void PlaySongRange(int sampleStart, int sampleLength)
        {
            var task = Env.Song.Generate(sampleStart, sampleLength);

            task.ContinueWith(taskResult =>
            {
                if (taskResult.IsFaulted)
                {
                    Console.WriteLine("Error playing song: " + taskResult.Exception.Message);
                    return;
                }

                var result = taskResult.Result;

                AudioPlaybackEngine.Instance.StopPlayback();
                AudioPlaybackEngine.Instance.PlaySound(result.Channel, 0d);

                Console.WriteLine($"Generate async task finished in {result.ElapsedMilliseconds} ms");
            });
        }

        private void OnAddTrack(object sender, ExecutedRoutedEventArgs e)
        {
            Env.Song.AddTrack();
        }

        private void OnZoomIn(object sender, ExecutedRoutedEventArgs e) => Zoom(true);
        private void OnZoomOut(object sender, ExecutedRoutedEventArgs e) => Zoom(false); 
        private void Zoom(bool zoomIn)
        {
            //var stringRep = Env.TimePerPixel.ToString();
            //Env.TimePerPixel = zoomIn ?
            //    (stringRep.Contains("5") ? Env.TimePerPixel / 5 * 2 : Env.TimePerPixel / 2) :
            //    (stringRep.Contains("2") ? Env.TimePerPixel / 2 * 5 : Env.TimePerPixel * 2);
            //GuiManager.Instance.RedrawSong();
            //Env.OnViewChanged();
        }

        private void OnStop(object sender, ExecutedRoutedEventArgs e) => StopAudioPlayback();
        private void StopAudioPlayback()
        {
            AudioPlaybackEngine.Instance.StopPlayback();
        }

        private int selectSampleStart;
        private int selectSampleLength;

        private void SetSelector(double startPosition, double endPosition, bool noSelection)
        {
            this.selectSampleStart = (int)(startPosition * Env.SamplesPerPixel);
            this.selectSampleLength = 0;

            if (noSelection)
            {
                this.selector.Width = 1d;
                Canvas.SetLeft(this.selector, startPosition);
            }
            else
            {
                var width = endPosition - startPosition;
                this.selectSampleLength = (int)(width * Env.SamplesPerPixel);
                
                if (width == 0)
                {
                    width = 1;
                }
                if (width >= 0)
                {
                    this.selector.Width = width;
                    Canvas.SetLeft(this.selector, startPosition);
                }
                else
                {
                    this.selector.Width = -width;
                    Canvas.SetLeft(this.selector, endPosition);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Prevent alt key from stopping a snap-drag
            if (e.Key == Key.System && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)) // && e.OriginalSource is TextBox)
            {
                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Delete:
                    GuiManager.Instance.DeleteSelectedParts();
                    break;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //AudioPlaybackEngine.Instance.StopPlayback();
            e.Cancel = !Env.ConfirmChangesMade();
            if (!e.Cancel)
            {
                AudioPlaybackEngine.Instance.Dispose();
            }
        }

        private void OnAbout(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show($"{this.AppNameAndVersion}\n\n(c) 2020 by Thor Muto Asmund\nthorasmund@gmail.com");
        }

        private void OnCheckForUpdates(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Not implemented yet");
        }
        private void OnSettings(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Not implemented yet");
        }

        private void PlaybackTimeUpdated(TimeSpan newTime)
        {
            Dispatcher.Invoke(() =>
            {
                this.timeCounterText.Text = String.Format("{0:m\\:ss\\:ff}", newTime);
            });
        }
    }
}

