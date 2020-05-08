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

        public GuiManager(StackPanel trackHeadPanel, StackPanel trackPanel)
        {
            this.trackHeadPanel = trackHeadPanel;
            this.trackPanel = trackPanel;
            Song.ProjectLoaded += loaded => RedrawSong();
            Song.TrackAdded += TrackAdded;
            Song.TrackChanged += TrackChanged;
            Song.PartAdded += PartAdded;
            Song.PartChanged += PartChanged;
            Song.PartDeleted += PartDeleted;
        }

        public void RedrawSong()
        {
            this.trackHeadPanel.Children.Clear();
            this.trackPanel.Children.Clear();
            this.trackControls.Clear();
            this.partControls.Clear();

            if (Env.Song == null)
            {
                return;
            }

            Env.Song.Tracks.ForEach(track => TrackAdded(track));
        }

        private Dictionary<string, TrackControl> trackControls = new Dictionary<string, TrackControl>();
        private Dictionary<string, PartControl> partControls = new Dictionary<string, PartControl>();
        private static int Zindex = 0;

        private void TrackAdded(Track track)
        {
            // Add control
            var trackHeadControl = TrackHeadControl.Create( track);
            this.trackHeadPanel.Children.Add(trackHeadControl);
            var trackControl = TrackControl.Create(trackHeadControl, track);
            this.trackPanel.Children.Add(trackControl);
            this.trackControls[track.Id] = trackControl;                
            
            // Add parts
            foreach (var part in Env.Song.Parts.Where(p => p.TrackId == track.Id))
            {
                PartAdded(part);
            }
        }

        private void TrackChanged(Track track)
        {
        }

        private void PartAdded(Part part)
        {
            var trackControl = this.trackControls[part.TrackId];
            var partControl = PartControl.Create(part, trackControl);
            trackControl.Children.Add(partControl);
            this.partControls[part.Id] = partControl;

            var left = part.Start / (double)Env.TimePerPixel;
            partControl.Width = part.Length / (double)Env.TimePerPixel;
            Canvas.SetLeft(partControl, left);

            void ChangeZIndex(object sender, int zIndex)
            {
                if (sender is UIElement uiElement)
                {
                    Panel.SetZIndex(uiElement, zIndex);
                }
            }

            partControl.PreviewMouseDown += (sender, e) => ChangeZIndex(sender, Int32.MaxValue);
            partControl.PreviewMouseUp += (sender, e) => ChangeZIndex(sender, Zindex++);
        }


        private void PartChanged(Part part)
        {
            var partControl = this.partControls[part.Id];
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
