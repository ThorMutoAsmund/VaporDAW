using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VaporDAW
{
    public partial class TrackControl : UserControl
    {
        public Action Selected;
        public TrackHeadControl TrackHeadControl { get; set; }

        private Point contextMousePosition;
        private Panel ParentPanel { get; set; }

        private Track track;
        public Track Track 
        {
            get => this.track;
            private set
            {
                this.track = value;
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    this.grid.Background = new SolidColorBrush(this.IsSelected ? Colors.TrackSelected : Colors.Track );
                }
            }
        }

        public UIElementCollection Children => this.grid.Children;        

        public TrackControl()
        {
            InitializeComponent();

            this.grid.Background = new SolidColorBrush(Colors.Track);
            this.border.BorderBrush = new SolidColorBrush(Colors.TrackBorder);

            Song.TrackChanged += changedTrack =>
            {
                if (changedTrack == this.Track)
                {
                    this.Track = changedTrack;
                }
            };

            // Store mouse down on context menu click
            this.MouseDown += (sender, e) =>
                this.contextMousePosition = e.ChangedButton == MouseButton.Right ? e.GetPosition(this.grid) : this.contextMousePosition;

            // Context menu
            this.addPartMenuItem.Click += (sender, e) => AddPart(point: this.contextMousePosition);
            this.propertiesMenuItem.Click += (sender, e) => ShowProperties();
        }

        public static TrackControl Create(Panel panel, TrackHeadControl trackHeadControl, Track track)
        {
            var trackControl = new TrackControl()
            {
                ParentPanel = panel,
                TrackHeadControl = trackHeadControl,
                Track = track,
                Width = Env.Song.SongLength / (double)Env.TimePerPixel
            };
            panel.Children.Add(trackControl);
            trackHeadControl.TrackControl = trackControl;

            return trackControl;
        }

        private Part AddPart(Point? point = null, string title = null)
        {
            if (Env.Song == null)
            {
                return null;
            }

            Part part = Env.Song.AddPart(this.track, point, title);

            // Add control
            var partControl = PartControl.Create(this, part);

            return part;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            Select();
        }

        public void Select()
        {
            foreach (var otherControl in Song.SelectedParts)
            {
                otherControl.IsSelected = false;
            }
            Song.SelectedParts.Clear();

            if (!this.IsSelected)
            {
                foreach (var otherControl in Song.SelectedTracks)
                {
                    otherControl.IsSelected = false;
                    otherControl.TrackHeadControl.IsSelected = false;
                }
                Song.SelectedTracks.Clear();


                this.IsSelected = true;
                Song.SelectedTracks.Add(this);
                if (this.TrackHeadControl != null)
                {
                    this.TrackHeadControl.IsSelected = true;
                }

                this.Selected?.Invoke();
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            ShowProperties();
        }

        private void ShowProperties()
        { 
            var dialog = EditTrackDialog.Create(Env.MainWindow, this.Track);
            if (dialog.ShowDialog() ?? false)
            {
                Env.Song.OnTrackChanged(this.Track);
            }
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

            var point = e.GetPosition(this.grid);

            if (e.Data.GetDataPresent("sample"))
            {
                var sampleName = e.Data.GetData("sample") as string;

                var part = AddPart(point: point, title: sampleName);
                part.AddSample(sampleName);
                
            }
            else if (e.Data.GetDataPresent("script"))
            {
                var scriptName = e.Data.GetData("script") as string;

                var part = AddPart(point: point, title: scriptName); 
                part.AddScript(scriptName);
            }
        }
    }
}
