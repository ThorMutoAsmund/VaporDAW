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
    /// <summary>
    /// Interaction logic for TrackControl.xaml
    /// </summary>
    public partial class TrackControl : UserControl
    {
        public Color SelectedHeaderColor = Color.FromRgb(112, 112, 122);
        public Color HeaderColor = Color.FromRgb(80, 80, 80);
        public Color SelectedGridColor = Color.FromRgb(64, 64, 74);
        public Color GridColor = Color.FromRgb(32, 32, 32);

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
                    this.header.Background = new SolidColorBrush(this.IsSelected ? this.SelectedHeaderColor : this.HeaderColor );
                    this.grid.Background = new SolidColorBrush(this.IsSelected ? this.SelectedGridColor : this.GridColor );
                }
            }
        }

        public Action Selected;

        public TrackControl()
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

        public static TrackControl Create(Panel panel, Track track)
        {
            var trackControl = new TrackControl()
            {
                Track = track
            };
            panel.Children.Add(trackControl);

            return trackControl;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            this.IsSelected = true;
            this.Selected?.Invoke();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            var dialog = EditTrackDialog.Create(Env.MainWindow, this.Track);
            if (dialog.ShowDialog() ?? false)
            {
                Env.Song.OnTrackChanged(this.Track);
            }
        }
    }
}
