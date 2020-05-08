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
        enum MouseMoveAction
        {
            None,
            Move,
            Resize
        }

        public Part Part { get; private set; }

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

        private TrackControl ParentTrackControl { get; set; }

        private Point mouseDownTrackPosition;
        private bool isSelected;
        private MouseMoveAction mouseMoveAction = MouseMoveAction.None;
        private bool didMove;
        private double mouseDownLeft;
        private double mouseDownWidth;
        private int mouseMoveTrackNo;

        public PartControl()
        {
            InitializeComponent();

            this.border.BorderBrush = new SolidColorBrush(Colors.PartBorder);
            this.grid.Background = new SolidColorBrush(Colors.Part);
            this.rightHandle.Background = new SolidColorBrush(Colors.PartHandle);

            // Context menu
            this.propertiesMenuItem.Click += (sender, e) => ShowProperties();
            this.deleteMenuItem.Click += (sender, e) => DeletePart();

            MouseHook.OnMouseUp += (sender, p) => MouseHookMouseUp();
        }

        public static PartControl Create(Part part, TrackControl parentTrackControl)
        {
            var partControl = new PartControl()
            {
                Part = part,
                ParentTrackControl = parentTrackControl
            };

            return partControl;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Env.Song == null)
            {
                return;
            }

            this.mouseDownTrackPosition = e.GetPosition(this.ParentTrackControl);
            this.mouseMoveAction = e.OriginalSource == this.grid ? MouseMoveAction.Move : (e.OriginalSource == this.rightHandle? MouseMoveAction.Resize : MouseMoveAction.None);
            this.mouseDownLeft = Canvas.GetLeft(this);
            this.mouseDownWidth = this.Width;
            
            var trackPanelPosition = e.GetPosition(Env.TrackPanel);
            this.mouseMoveTrackNo =  (int)(trackPanelPosition.Y / Env.TrackHeight);
            
            this.didMove = false;
            Env.TrackPanel.MouseMove += TrackPanelMouseMove;
        }

        private void TrackPanelMouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseMoveAction == MouseMoveAction.Move)
            {
                // Position in track
                var trackPosition = e.GetPosition(this.ParentTrackControl);
                var newLeft = this.mouseDownLeft + (trackPosition.X - this.mouseDownTrackPosition.X);
                if (newLeft < 0d)
                {
                    newLeft = 0d;
                }
                Canvas.SetLeft(this, newLeft);

                // Track
                var trackPanelPosition = e.GetPosition(Env.TrackPanel);
                var trackNo = (int)(trackPanelPosition.Y / Env.TrackHeight);
                if (trackNo != this.mouseMoveTrackNo)
                {
                    this.mouseMoveTrackNo = trackNo;
                    var newTrackControl = GuiManager.Instance.GetTrackControl(trackNo);
                    this.ParentTrackControl.Children.Remove(this);
                    newTrackControl.Children.Add(this);
                    this.ParentTrackControl = newTrackControl;
                }
            }
            else
            {
                var position = e.GetPosition(this.ParentTrackControl);
                var newWidth = this.mouseDownWidth + (position.X - this.mouseDownTrackPosition.X);
                if (newWidth < 12d)
                {
                    newWidth = 12d;
                }
                this.Width = newWidth;
            }
            this.didMove = true;
        }

        private void MouseHookMouseUp()
        {
            if (this.mouseMoveAction != MouseMoveAction.None)
            {
                Env.TrackPanel.MouseMove -= TrackPanelMouseMove;

                if (this.didMove)
                {
                    if (this.mouseMoveAction == MouseMoveAction.Move)
                    {
                        // Position in track
                        var newLeft = Canvas.GetLeft(this);
                        this.Part.Start = newLeft * (double)Env.TimePerPixel;

                        // Track
                        this.Part.TrackId = this.ParentTrackControl.Track.Id;

                        Env.Song.OnPartChanged(this.Part);
                    }
                    else if (this.mouseMoveAction == MouseMoveAction.Resize)
                    {
                        var newWidth = this.Width;
                        Part.Length = newWidth * (double)Env.TimePerPixel;
                        Env.Song.OnPartChanged(this.Part);
                    }
                }

                this.mouseMoveAction = MouseMoveAction.None;
            }
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            ShowProperties();
        }

        private void ShowProperties()
        {
            if (Env.Song == null)
            {
                return;
            }

            var dialog = EditPartDialog.Create(Env.MainWindow, this.Part);
            if (dialog.ShowDialog() ?? false)
            {
                Env.Song.OnPartChanged(this.Part);
            }
        }

        private void DeletePart()
        {
            if (Env.Song == null)
            {
                return;
            }

            Env.Song.DeletePart(this.Part);
        }
    }
}
