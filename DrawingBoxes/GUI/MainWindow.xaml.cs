using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DrawingBoxes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point contextMousePosition;

        public MainWindow()
        {
            InitializeComponent();

            SetTitle();

            Env.CanvasStartTime = 0d;
            Env.CanvasTimePerPixel = 0.01d;

            Env.MainWindow = this;
            Env.Canvas = this.canvas;

            this.Closing += (sender, e) => e.Cancel = !Env.ConfirmChangesMade();

            this.fileMenu.SubmenuOpened += (sender, e) =>
            {
                this.recentFilesMenu.Items.Clear();

                foreach (var recentFilePath in Env.RecentFiles)
                {
                    var menuItem = new MenuItem()
                    {
                        Header = recentFilePath,
                    };
                    menuItem.Click += (_sender, _e) => OpenProject(recentFilePath);
                    this.recentFilesMenu.Items.Add(menuItem);
                }
            };

            this.newScriptMenu.Click += (sender, e) => NewScript();
            this.importSamplesMenu.Click += (sender, e) => ImportSamples();
            this.generateOutputMenu.Click += (sender, e) => GenerateOutput();

            Song.RequestEditScript += script => OpenScriptTab(script);

            Song.ProjectLoaded += loaded =>
            {
                if (!loaded)
                {
                    this.tabControl.IsEnabled = false;
                    this.saveMenu.IsEnabled = false;
                    this.closeMenu.IsEnabled = false;
                    this.toolsMenu.IsEnabled = false;

                    SetTitle();

                    RedrawCanvas();
                }
                else
                {
                    this.tabControl.IsEnabled = true;
                    this.saveMenu.IsEnabled = true;
                    this.closeMenu.IsEnabled = true;
                    this.toolsMenu.IsEnabled = true;
                    SetTitle(Env.Song.SongName);

                    RedrawCanvas();
                }
            };

            // Store mouse down on context menu click
            this.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    this.contextMousePosition = e.GetPosition(Env.Canvas);
                }
            };

            this.addPartMenuItem.Click += (sender, e) => AddPart(this.contextMousePosition);

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
        }


        private void SetupCanvas()
        {
            //this.ts1 = new PartShape()
            //{
            //    TrackNo = 1,
            //    Width = 100
            //};
            //this.canvas.Children.Add(ts1);

            //ts1.MouseDown += (_, __) =>
            //{
            //    this.MouseMove += ts1.MoveMouse;
            //};
            //this.MouseUp += (_, __) =>
            //{
            //    this.MouseMove -= ts1.MoveMouse;
            //    ts1.UpMouse();
            //};
            //this.MouseLeave += (_, __) =>
            //{
            //    this.MouseMove -= ts1.MoveMouse;
            //    ts1.UpMouse();
            //};
        }


        private void RedrawCanvas()
        {
            this.canvas.Children.Clear();

            if (Env.Song == null)
            {
                return;
            }

            var bkg = new EditorBackgroundShape();
   
            bkg.SetBinding(FrameworkElement.WidthProperty, new Binding("ActualWidth") { ElementName = "canvas" });
            bkg.SetBinding(FrameworkElement.HeightProperty, new Binding("ActualHeight") { ElementName = "canvas" });

            this.canvas.Children.Add(bkg);
            Canvas.SetLeft(bkg, 0);
            Canvas.SetTop(bkg, 0);

            foreach (var part in Env.Song.Parts)
            {
                PartShape.Create(this.canvas, part);
            }
        }
        
        private void OnNew(object sender, ExecutedRoutedEventArgs e) => NewProject();
        private void OnOpen(object sender, ExecutedRoutedEventArgs e) => OpenProject();
        private void OnClose(object sender, ExecutedRoutedEventArgs e) => CloseProject();
        private void OnSave(object sender, ExecutedRoutedEventArgs e) => SaveProject();
        private void OnExit(object sender, ExecutedRoutedEventArgs e) => this.Close();
        private void OnCloseTab(object sender, ExecutedRoutedEventArgs e) => CloseTab();
        

        private void SetTitle(string projectName = null)
        {
            this.Title = $"{(String.IsNullOrEmpty(projectName) ? String.Empty : $"{projectName} - ")}{Env.ApplicationName}";
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

        private void AddPart(Point p)
        {
            if (Env.Song == null)
            {
                return;
            }

            var part = Env.Song.AddPart(p);
            PartShape.Create(this.canvas, part);
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

        private void Canvas_DragEnterOver(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetDataPresent("sample") || e.Data.GetDataPresent("script")))
            {
                return;
            }
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (Env.Song == null)
            {
                return;
            }

            if (e.Data.GetDataPresent("sample"))
            {
                var sampleName = e.Data.GetData("sample") as string;

                var part = Env.Song.AddPart(e.GetPosition(Env.Canvas), sampleName);
                PartShape.Create(this.canvas, part);
                Env.Song.AddSampleToPart(part, sampleName);
            }
            else if (e.Data.GetDataPresent("script"))
            {
                var scriptName = e.Data.GetData("script") as string;

                var part = Env.Song.AddPart(e.GetPosition(Env.Canvas), scriptName);
                PartShape.Create(this.canvas, part);
                Env.Song.AddScriptToPart(part, scriptName);                
            }
        }

    }
}
