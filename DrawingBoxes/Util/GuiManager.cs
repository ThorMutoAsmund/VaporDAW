using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VaporDAW
{
    public class GuiManager
    {
        private StackPanel trackHeadPanel;
        private StackPanel trackPanel;
        private Dictionary<string, TrackControl> trackControls = new Dictionary<string, TrackControl>();
        private Dictionary<string, PartControl> partControls = new Dictionary<string, PartControl>();
        private List<TrackControl> trackControlsByIndex = new List<TrackControl>();
        private List<PartControl> selectedParts = new List<PartControl>();
        private List<TrackControl> selectedTracks = new List<TrackControl>();
        private int zIndex = 0;

        public static GuiManager Instance { get; private set; }
        
        private GuiManager(StackPanel trackHeadPanel, StackPanel trackPanel)
        {
            Instance = this;
            
            this.trackHeadPanel = trackHeadPanel;
            this.trackPanel = trackPanel;

            Song.ClearVolatile += ClearVolatile;
            Song.ProjectLoaded += loaded => RedrawSong();
            Song.TrackAdded += AddTrackControl;
            Song.TrackChanged += TrackChanged;
            Song.PartAdded += AddPartControl;
            Song.PartChanged += PartChanged;
            Song.PartDeleted += PartDeleted;
        }

        public static void Create(StackPanel trackHeadPanel, StackPanel trackPanel)
        {
            new GuiManager(trackHeadPanel, trackPanel);
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

        private void TrackChanged(Track track)
        {
        }

        private void AddPartControl(Part part)
        {
            var trackControl = this.trackControls[part.TrackId];
            var partControl = PartControl.Create(part, trackControl);
            trackControl.Children.Add(partControl);
            this.partControls[part.Id] = partControl;
            
            SetPartControlProperties(part, partControl);


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

        private void SelectPartControl(PartControl partControl)
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


        private void PartChanged(Part part)
        {
            var partControl = this.partControls[part.Id];
            SetPartControlProperties(part, partControl);
        }

        private void SetPartControlProperties(Part part, PartControl partControl)
        {
            var left = part.Start / (double)Env.TimePerPixel;
            partControl.Width = part.Length / (double)Env.TimePerPixel;
            Canvas.SetLeft(partControl, left);
        }

        private void PartDeleted(Part part)
        {
            var trackControl = this.trackControls[part.TrackId];
            var partControl = this.partControls[part.Id];
            trackControl.Children.Remove(partControl);
        }
    }
}
