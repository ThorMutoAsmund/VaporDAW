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

    public partial class TrackHeadControl : UserControl
    {
        public TrackControl TrackControl { get; set; }

        private Color selectedHeaderColor = Color.FromRgb(112, 112, 122);
        private Color headerColor = Color.FromRgb(80, 80, 80);

        private Panel ParentPanel { get; set; }
        private Track track;
        public Track Track 
        {
            get => this.track;
            private set
            {
                this.track = value;
                this.titleLabel.Content = track.Title;
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
                    this.header.Background = new SolidColorBrush(this.IsSelected ? this.selectedHeaderColor : this.headerColor );
                }
            }
        }

        public TrackHeadControl()
        {
            InitializeComponent();

            Song.TrackChanged += changedTrack =>
            {
                if (changedTrack == this.Track)
                {
                    this.Track = changedTrack;
                }
            };
        }

        public static TrackHeadControl Create(Panel panel, Track track)
        {
            var trackHeadControl = new TrackHeadControl()
            {
                ParentPanel = panel,
                Track = track
            };
            panel.Children.Add(trackHeadControl);

            return trackHeadControl;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            this.TrackControl.Select();
            //Select(true);
        }
    }
}
