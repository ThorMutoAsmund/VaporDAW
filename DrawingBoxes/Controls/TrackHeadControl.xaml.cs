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

        private Track track;
        public Track Track 
        {
            get => this.track;
            private set
            {
                this.track = value;
                SetTrackProperties();
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
                    this.header.Background = new SolidColorBrush(this.IsSelected ? Colors.TrackHeadSelected : Colors.TrackHead );
                }
            }
        }

        public TrackHeadControl()
        {
            InitializeComponent();

            this.header.Background = new SolidColorBrush(Colors.TrackHead);
            this.border.BorderBrush = new SolidColorBrush(Colors.TrackHeadBorder);

            Song.TrackChanged += track => { if (track == this.Track) SetTrackProperties(); };
        }

        public static TrackHeadControl Create(Track track)
        {
            var trackHeadControl = new TrackHeadControl()
            {
                Track = track
            };

            return trackHeadControl;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            this.TrackControl.Select();
            //Select(true);
        }

        private void SetTrackProperties()
        {
            this.titleLabel.Content = this.track.Title;
        }
    }
}
