using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VaporDAW
{
    public class GuiManager
    {
        private MainWindow mainWindow;
        private StackPanel trackHeadPanel;
        private StackPanel trackPanel;
        private Dictionary<string, TrackControl> trackControls = new Dictionary<string, TrackControl>();
        private Dictionary<string, PartControl> partControls = new Dictionary<string, PartControl>();
        private List<TrackControl> trackControlsByIndex = new List<TrackControl>();
        private List<PartControl> selectedParts = new List<PartControl>();
        private List<TrackControl> selectedTracks = new List<TrackControl>();
        private int zIndex = 0;
        private Thread keyPressThread;

        public Action EscapePressed;

        public static GuiManager Instance { get; private set; }
        
        private GuiManager(MainWindow mainWindow, StackPanel trackHeadPanel, StackPanel trackPanel)
        {
            Instance = this;

            this.mainWindow = mainWindow;
            this.trackHeadPanel = trackHeadPanel;
            this.trackPanel = trackPanel;

            Song.ClearVolatile += ClearVolatile;
            Song.ProjectLoaded += loaded => RedrawSong();
            Song.TrackAdded += AddTrackControl;
            Song.PartAdded += AddPartControl;
            Song.PartDeleted += PartDeleted;

            this.keyPressThread = new Thread(DetectKeyPress);
            this.keyPressThread.Start();

            Application.Current.Exit += ApplicationExit;
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            this.keyPressThread?.Abort();
        }

        public static void Create(MainWindow mainWindow, StackPanel trackHeadPanel, StackPanel trackPanel)
        {
            new GuiManager(mainWindow, trackHeadPanel, trackPanel);
        }

        private void DetectKeyPress()
        {
            for (; ; )
            {
                this.mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Keyboard.IsKeyDown(Key.Escape))
                    {
                        this.EscapePressed?.Invoke();
                    }
                }));

                Thread.Sleep(100);
            }
        }


        private void ClearVolatile()
        {
            this.selectedTracks.Clear();
            this.selectedParts.Clear();
        }

        public void RedrawSong()
        {
            this.trackHeadPanel.Children.Clear();
            this.trackPanel.Children.Clear();
            this.trackControls.Clear();
            this.trackControlsByIndex.Clear();
            this.partControls.Clear();

            if (Env.Song == null)
            {
                return;
            }

            Env.Song.Tracks.ForEach(track => AddTrackControl(track));
        }

        private void AddTrackControl(Track track)
        {
            // Add control
            var trackHeadControl = TrackHeadControl.Create( track);
            this.trackHeadPanel.Children.Add(trackHeadControl);
            var trackControl = TrackControl.Create(trackHeadControl, track);
            this.trackPanel.Children.Add(trackControl);
            this.trackControls[track.Id] = trackControl;
            this.trackControlsByIndex.Add(trackControl);

            trackHeadControl.MouseDown += (sender, e) => SelectTrackControl(trackHeadControl, trackControl);
            trackControl.PreviewMouseDown += (sender, e) => SelectTrackControl(trackHeadControl, trackControl);

            // Add parts
            foreach (var part in Env.Song.Parts.Where(p => p.TrackId == track.Id))
            {
                AddPartControl(part);
            }
        }

        public TrackControl GetTrackControl(int trackNo)
        {
            return trackNo >= 0 && trackNo < this.trackControlsByIndex.Count() ? this.trackControlsByIndex[trackNo] : null;
        }

        public PartControl GetPartControl(string id)
        {
            return this.partControls.ContainsKey(id) ? this.partControls[id] : null;
        }

        public bool TryGetPartControlSnapValue(PartControl self, double left, out double snapPosition, out double snapValue, double margin = 8d)
        {
            var result = false;
            snapPosition = double.PositiveInfinity;
            snapValue = double.PositiveInfinity;
            foreach (var partControl in this.partControls.Values.Where(pc => pc != self))
            {
                var otherLeft = Canvas.GetLeft(partControl);
                var difference = Math.Abs(otherLeft - left);
                if (difference < margin && difference < snapPosition)
                {
                    snapPosition = otherLeft;
                    snapValue = partControl.Part.Start;
                    result = true;
                }
                var otherRight = otherLeft + partControl.Width;
                difference = Math.Abs(otherRight - left);
                if (difference < margin && difference < snapPosition)
                {
                    snapPosition = otherRight;
                    snapValue = partControl.Part.Start + partControl.Part.Length;
                    result = true;
                }
            }

            return result;
        }


        private void AddPartControl(Part part)
        {
            var trackControl = this.trackControls[part.TrackId];
            var partControl = PartControl.Create(part, trackControl);
            trackControl.Children.Add(partControl);
            this.partControls[part.Id] = partControl;
            
            //SetPartControlProperties(part, partControl);


            partControl.PreviewMouseDown += (sender, e) => ChangeZIndex(sender, Int32.MaxValue);
            partControl.PreviewMouseUp += (sender, e) => ChangeZIndex(sender, this.zIndex++);
            partControl.MouseDown += (sender, e) => SelectPartControl(partControl);
        }

        private void ChangeZIndex(object sender, int zIndex)
        {
            if (sender is UIElement uiElement)
            {
                Panel.SetZIndex(uiElement, zIndex);
            }
        }

        public void SelectPartControl(PartControl partControl)
        {
            if (!partControl.IsSelected)
            {
                foreach (var otherControl in this.selectedParts)
                {
                    otherControl.IsSelected = false;
                }

                this.selectedParts.Clear();

                partControl.IsSelected = true;
                this.selectedParts.Add(partControl);
            }
        }

        public void DeleteSelectedParts()
        {
            if (this.selectedParts.Count() > 0)
            {
                if (MessageBox.Show($"Are you sure you want to delete the selected {this.selectedParts.Count()} part(s)?", "Delete part(s)", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    foreach (var partControl in this.selectedParts.ToArray())
                    {
                        Env.Song.DeletePart(partControl.Part);
                    }
                }
            }
        }

        private void SelectTrackControl(TrackHeadControl trackHeadControl, TrackControl trackControl)
        {
            foreach (var partControl in this.selectedParts)
            {
                partControl.IsSelected = false;
            }
            this.selectedParts.Clear();

            if (!trackControl.IsSelected)
            {
                foreach (var otherControl in this.selectedTracks)
                {
                    otherControl.IsSelected = false;
                    otherControl.TrackHeadControl.IsSelected = false;
                }
                this.selectedTracks.Clear();


                trackControl.IsSelected = true;
                trackHeadControl.IsSelected = true;
                this.selectedTracks.Add(trackControl);
            }
        }


        //private void PartChanged(Part part)
        //{
        //    var partControl = this.partControls[part.Id];
        //    SetPartControlProperties(part, partControl);
        //}

        //private void SetPartControlProperties(Part part, PartControl partControl)
        //{
        //    var left = part.Start / (double)Env.TimePerPixel;
        //    partControl.Width = part.Length / (double)Env.TimePerPixel;
        //    Canvas.SetLeft(partControl, left);
        //    partControl.Title = part.Title;
        //}

        private void PartDeleted(Part part)
        {
            var trackControl = this.trackControls[part.TrackId];
            var partControl = this.partControls[part.Id];
            trackControl.Children.Remove(partControl);
        }
    }
}
