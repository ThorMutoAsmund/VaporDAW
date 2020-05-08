using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private bool mouseDownInLeftHalf;
        private Point mouseDownTrackPosition;
        private bool isSelected;
        private MouseMoveAction mouseMoveAction = MouseMoveAction.None;
        private bool didMove;
        private double mouseDownLeft;
        private double mouseDownWidth;
        private TrackControl mouseDownTrackControl;
        private int mouseMoveTrackNo;

        private double newStart;
        private double newLength;

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
                ParentTrackControl = parentTrackControl,
                Height = Env.TrackHeight
            };

            return partControl;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Env.Song == null)
            {
                return;
            }

            var mouseDownPosition = e.GetPosition(this);
            this.mouseDownInLeftHalf = mouseDownPosition.X < this.Width / 2;
            this.mouseDownTrackPosition = e.GetPosition(this.ParentTrackControl);
            this.mouseMoveAction = e.OriginalSource == this.grid ? MouseMoveAction.Move : (e.OriginalSource == this.rightHandle? MouseMoveAction.Resize : MouseMoveAction.None);
            this.mouseDownLeft = Canvas.GetLeft(this);
            this.mouseDownWidth = this.Width;
            this.mouseDownTrackControl = this.ParentTrackControl;
            this.newLength = this.Part.Length;
            this.newStart = this.Part.Start;

            var trackPanelPosition = e.GetPosition(Env.TrackPanel);
            this.mouseMoveTrackNo =  (int)(trackPanelPosition.Y / Env.TrackHeight);
            
            this.didMove = false;

            Env.TrackPanel.MouseMove += TrackPanelMouseMove;
            GuiManager.Instance.EscapePressed += EscapePressed;
        }

        private void EscapePressed()
        {
            this.didMove = false;
            Canvas.SetLeft(this, this.mouseDownLeft);
            this.Width = this.mouseDownWidth;
            if (this.ParentTrackControl != this.mouseDownTrackControl)
            {
                this.ParentTrackControl.Children.Remove(this);
                this.mouseDownTrackControl.Children.Add(this);
                this.ParentTrackControl = this.mouseDownTrackControl;
            }

            MouseHookMouseUp();
        }


        private void TrackPanelMouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseMoveAction == MouseMoveAction.Move)
            {
                // Position in track
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    var trackPosition = e.GetPosition(this.ParentTrackControl);
                    var newLeft = this.mouseDownLeft + (trackPosition.X - this.mouseDownTrackPosition.X);
                    this.newStart = newLeft * (double)Env.TimePerPixel;

                    // Snap
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        if (GuiManager.Instance.TryGetPartControlSnapValue(this, this.mouseDownInLeftHalf ? newLeft : newLeft + this.Width, out var snapPosition, out var snapValue))
                        {
                            newLeft = this.mouseDownInLeftHalf ? snapPosition : snapPosition - this.Width;
                            this.newStart = mouseDownInLeftHalf ? snapValue : snapValue - this.Part.Length;
                        }
                    }

                    // Don't go under 0
                    if (newLeft < 0d || this.newStart < 0d)
                    {
                        newLeft = 0d;
                        this.newStart = 0d;
                    }

                    Canvas.SetLeft(this, newLeft);
                }

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
                this.newLength = newWidth * (double)Env.TimePerPixel;

                // Snap
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (GuiManager.Instance.TryGetPartControlSnapValue(this, this.mouseDownLeft + newWidth, out var snapPosition, out var snapValue))
                    {
                        newWidth = snapPosition - this.mouseDownLeft;
                        this.newLength = snapValue - this.Part.Start;
                    }
                }

                if (newWidth < 12d)
                {
                    newWidth = 12d;
                    this.newLength = newWidth * (double)Env.TimePerPixel;
                }

                this.Width = newWidth;
            }
            this.didMove = true;
        }

        private void MouseHookMouseUp()
        {
            if (this.mouseMoveAction != MouseMoveAction.None)
            {
                GuiManager.Instance.EscapePressed -= EscapePressed;
                Env.TrackPanel.MouseMove -= TrackPanelMouseMove;

                if (this.didMove)
                {
                    if (this.mouseMoveAction == MouseMoveAction.Move)
                    {
                        // Position in track
                        this.Part.Start = this.newStart;

                        // Track
                        this.Part.TrackId = this.ParentTrackControl.Track.Id;

                        Env.Song.OnPartChanged(this.Part);
                    }
                    else if (this.mouseMoveAction == MouseMoveAction.Resize)
                    {
                        Part.Length = this.newLength;
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
