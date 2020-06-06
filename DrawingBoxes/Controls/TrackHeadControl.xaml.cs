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

    public partial class TrackHeadControl : UserControl
    {
        public TrackControl TrackControl { get; set; }

        public Track Track 
        {
            get => this.track;
            private set
            {
                this.track = value;
                SetProperties();
            }
        }

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

        private Track track;
        private bool isSelected;

        public TrackHeadControl()
        {
            InitializeComponent();

            this.header.Background = new SolidColorBrush(Colors.TrackHead);
            this.border.BorderBrush = new SolidColorBrush(Colors.TrackHeadBorder);

            Song.TrackChanged += track => { if (track == this.Track) SetProperties(); };
        }

        public static TrackHeadControl Create(Track track)
        {
            var trackHeadControl = new TrackHeadControl()
            {
                Track = track,
                Height = Env.TrackHeight
            };

            return trackHeadControl;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            GuiManager.Instance.SelectTrackControl(this, this.TrackControl);
        }

        private void SetProperties()
        {
            this.titleLabel.Content = this.Track.Title;
        }
    }
}
