using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public TrackHeadControl TrackHeadControl { get; set; }

        private Point contextMousePosition;

        public Track Track
        {
            get => this.track;
            private set
            {
                this.track = value;
                SetProperties();
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
                    SetProperties();
                }
            }
        }

        public UIElementCollection Children => this.grid.Children;

        private Track track;
        public TrackControl()
        {
            InitializeComponent();

            this.grid.Background = new SolidColorBrush(Colors.Track);
            this.border.BorderBrush = new SolidColorBrush(Colors.TrackBorder);

            // Context menu
            this.addPartMenuItem.Click += (sender, e) => AddPart(point: this.contextMousePosition);
            this.propertiesMenuItem.Click += (sender, e) => ShowProperties();

            // React to changes
            Song.TrackChanged += track =>
            {
                if (track == this.Track)
                {
                    SetProperties();
                }
            };
        }

        public static TrackControl Create(TrackHeadControl trackHeadControl, Track track)
        {
            var trackControl = new TrackControl()
            {
                TrackHeadControl = trackHeadControl,
                Track = track,
                Width = (int)(Env.Song.SongSampleLength / Env.SamplesPerPixel),
                Height = Env.TrackHeight
            };
            trackHeadControl.TrackControl = trackControl;

            return trackControl;
        }

        private Part AddPart(Point? point = null, string title = null)
        {
            return Env.Song.AddPart(this.Track, point, title);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.contextMousePosition = e.ChangedButton == MouseButton.Right ? e.GetPosition(this.grid) : this.contextMousePosition;
            GuiManager.Instance.SelectTrackControl(this.TrackHeadControl, this);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            e.Handled = true;
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
            var point = e.GetPosition(this.grid);

            if (e.Data.GetDataPresent("sample"))
            {
                var sampleName = e.Data.GetData("sample") as string;

                var part = AddPart(point: point, title: sampleName);
                var generator = part.AddSample(sampleName);

                // Set length
                if (generator.Settings.ContainsKey(Tags.SampleId))
                {
                    var sampleId = generator.Settings[Tags.SampleId] as string;
                    part.SampleLength = SampleDataProcessor.GetSampleLength(sampleId);
                    Env.Song.OnPartChanged(part);
                }
            }
            else if (e.Data.GetDataPresent("script"))
            {
                var scriptName = e.Data.GetData("script") as string;

                var part = AddPart(point: point, title: scriptName); 
                part.AddScript(scriptName);
            }
        }

        private void SetProperties()
        {
            this.grid.Background = new SolidColorBrush(this.Track.IsAudible ?
                (this.IsSelected ? Colors.TrackSelected : Colors.Track) :
                (this.IsSelected ? Colors.InaudibleTrackSelected : Colors.InaudibleTrack));
        }
    }
}
