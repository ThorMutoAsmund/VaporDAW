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
    public partial class PartControl : UserControl
    {
        public Action Selected;
        private Point contextMousePosition;
        private TrackControl ParentTrackControl { get; set; }

        private Part part;
        public Part Part 
        {
            get => this.part;
            private set
            {
                this.part = value;
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
                    this.grid.Background = new SolidColorBrush(this.IsSelected ? Colors.PartSelected : Colors.Part);
                }
            }
        }

        public PartControl()
        {
            InitializeComponent();

            this.grid.Background = new SolidColorBrush(Colors.Part);
            this.border.BorderBrush = new SolidColorBrush(Colors.PartBorder);

            Song.PartChanged += changedPart =>
            {
                if (changedPart == this.Part)
                {
                    this.Part = changedPart;
                }
            };

            // Store mouse down on context menu click
            this.MouseDown += (sender, e) =>
                this.contextMousePosition = e.ChangedButton == MouseButton.Right ? e.GetPosition(this.grid) : this.contextMousePosition;

        }

        public static PartControl Create(TrackControl trackControl, Part part)
        {
            var partControl = new PartControl()
            {
                ParentTrackControl = trackControl,
                Part = part,
                Width = part.Length / (double)Env.TimePerPixel
            };
            trackControl.Children.Add(partControl);
            var left = part.Start / (double)Env.TimePerPixel;
            Canvas.SetLeft(partControl, left);

            return partControl;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Select();
        }

        public void Select()
        {
            if (!this.IsSelected)
            {
                foreach (var otherControl in Song.SelectedParts)
                {
                    otherControl.IsSelected = false;
                }

                Song.SelectedParts.Clear();

                this.IsSelected = true;
                Song.SelectedParts.Add(this);

                this.Selected?.Invoke();
            }
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            MessageBox.Show("Edit part not supported yet");
            //var dialog = EditTrackDialog.Create(Env.MainWindow, this.Track);
            //if (dialog.ShowDialog() ?? false)
            //{
            //    Env.Song.OnTrackChanged(this.Track);
            //}
        }
    }
}
