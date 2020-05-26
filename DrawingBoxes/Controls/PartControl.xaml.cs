using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VaporDAW
{
    public partial class PartControl : UserControl
    {
        enum MouseMoveAction
        {
            None,
            Move,
            Resize,
            Reset
        }

        public Part Part
        {
            get => this.part;
            private set
            {
                this.part = value;
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
                    this.grid.Background = new SolidColorBrush(this.IsSelected ? Colors.PartSelected : (this.Part.IsReference ? Colors.RefPart : Colors.Part));
                }
            }
        }

        private TrackControl ParentTrackControl { get; set; }

        private Part part;
        private bool mouseDownInLeftHalf;
        private Point mouseDownTrackPosition;
        private bool isSelected;
        private MouseMoveAction mouseMoveAction = MouseMoveAction.None;
        private bool didMove;
        private double mouseDownLeft;
        private double mouseDownWidth;
        private TrackControl mouseDownTrackControl;
        private int mouseMoveTrackNo;
        private bool mouseDownCopyMade;

        private double newStart;
        private double newLength;

        public PartControl()
        {
            InitializeComponent();

            this.border.BorderBrush = new SolidColorBrush(Colors.PartBorder);
            this.rightHandle.Background = new SolidColorBrush(Colors.PartHandle);

            // Context menu
            this.propertiesMenuItem.Click += (sender, e) => ShowProperties();
            this.deleteMenuItem.Click += (sender, e) => DeletePart();

            // React to changes
            Song.PartChanged += part => 
            { 
                if (part == this.Part || part.Id == this.Part.RefId)
                {
                    SetProperties();
                }
            };
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

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Env.TrackPanel.MouseMove += TrackPanelMouseMove;
                Env.TrackPanel.MouseUp += TrackPanelMouseUp;
                Env.SongPanel.MouseLeave += SongPanelMouseLeave;
                GuiManager.Instance.EscapePressed += EscapePressed;

                this.mouseMoveAction = e.OriginalSource == this.rightHandle && !this.Part.IsReference ? MouseMoveAction.Resize : MouseMoveAction.Move;

                var mouseDownPosition = e.GetPosition(this);
                this.mouseDownInLeftHalf = mouseDownPosition.X < this.Width / 2;
                this.mouseDownTrackPosition = e.GetPosition(this.ParentTrackControl);
                this.mouseDownLeft = Canvas.GetLeft(this);
                this.mouseDownWidth = this.Width;
                this.mouseDownTrackControl = this.ParentTrackControl;
                this.mouseDownCopyMade = false;
                this.newLength = this.Part.Length;
                this.newStart = this.Part.Start;
                var trackPanelPosition = e.GetPosition(Env.TrackPanel);
                this.mouseMoveTrackNo = (int)(trackPanelPosition.Y / Env.TrackHeight);
                this.didMove = false;
            }
        }

        public void ForceMoveStart(bool mouseDownInLeftHalf, Point mouseDownTrackPosition, double mouseDownLeft, double mouseDownWidth, 
            TrackControl mouseDownTrackControl, int mouseMoveTrackNo)
        {
            Env.TrackPanel.MouseMove += TrackPanelMouseMove;
            Env.TrackPanel.MouseUp += TrackPanelMouseUp;
            Env.SongPanel.MouseLeave += SongPanelMouseLeave;
            GuiManager.Instance.EscapePressed += EscapePressed;

            this.mouseMoveAction = MouseMoveAction.Move;
            this.mouseDownInLeftHalf = mouseDownInLeftHalf;
            this.mouseDownTrackPosition = mouseDownTrackPosition;
            this.mouseDownLeft = mouseDownLeft;
            this.mouseDownWidth = mouseDownWidth;
            this.mouseDownTrackControl = mouseDownTrackControl;
            this.mouseMoveTrackNo = mouseMoveTrackNo;
            this.didMove = true;
            this.mouseDownCopyMade = true;
        }

        private void EscapePressed()
        {
            CompleteMouseMoveAction(MouseMoveAction.Reset);
        }

        private void TrackPanelMouseUp(object sender, MouseEventArgs e)
        {
            CompleteMouseMoveAction(this.mouseMoveAction);
        }

        private void SongPanelMouseLeave(object sender, MouseEventArgs e)
        {
            CompleteMouseMoveAction(MouseMoveAction.Reset);
        }

        private void TrackPanelMouseMove(object sender, MouseEventArgs e)
        {
            this.didMove = true;

            if (this.mouseMoveAction == MouseMoveAction.Move)
            {
                // Position in track
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    var trackPosition = e.GetPosition(this.ParentTrackControl);
                    var newLeft = this.mouseDownLeft + (trackPosition.X - this.mouseDownTrackPosition.X);
                    this.newStart = newLeft * (double)Env.TimePerPixel;

                    // Snap
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
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

                // Create copy
                if (!this.mouseDownCopyMade && !this.Part.IsReference && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    this.mouseDownCopyMade = true;
                    Part clonedPart;
                    if (Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        clonedPart = Env.Song.ClonePart(this.Part, this.newStart, this.ParentTrackControl.Track);
                    }
                    else
                    {
                        clonedPart = Env.Song.RefClonePart(this.Part, this.newStart, this.ParentTrackControl.Track);
                    }

                    // Reset original control
                    EscapePressed();

                    // Get cloned control
                    var clonedPartControl = GuiManager.Instance.GetPartControl(clonedPart.Id);

                    // Force action on new control
                    clonedPartControl?.ForceMoveStart(this.mouseDownInLeftHalf, this.mouseDownTrackPosition, this.mouseDownLeft, this.mouseDownWidth,
                        this.mouseDownTrackControl, this.mouseMoveTrackNo);

                    GuiManager.Instance.SelectPartControl(clonedPartControl);
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
            else if (this.mouseMoveAction == MouseMoveAction.Resize)
            {
                var position = e.GetPosition(this.ParentTrackControl);
                var newWidth = this.mouseDownWidth + (position.X - this.mouseDownTrackPosition.X);
                this.newLength = newWidth * (double)Env.TimePerPixel;

                // Snap
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
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
        }

        private void CompleteMouseMoveAction(MouseMoveAction mouseMoveAction)
        {
            Env.TrackPanel.MouseMove -= TrackPanelMouseMove;
            Env.TrackPanel.MouseUp -= TrackPanelMouseUp;
            Env.SongPanel.MouseLeave -= SongPanelMouseLeave;
            GuiManager.Instance.EscapePressed -= EscapePressed;

            if (this.didMove)
            {
                if (mouseMoveAction == MouseMoveAction.Move)
                {
                    this.Part.Start = this.newStart;
                    this.Part.TrackId = this.ParentTrackControl.Track.Id;
                    Env.Song.OnPartChanged(this.Part);
                }
                else if (mouseMoveAction == MouseMoveAction.Resize)
                {
                    Part.Length = this.newLength;
                    Env.Song.OnPartChanged(this.Part);
                }
                else if (mouseMoveAction == MouseMoveAction.Reset)
                {
                    Canvas.SetLeft(this, this.mouseDownLeft);
                    this.Width = this.mouseDownWidth;
                    if (this.ParentTrackControl != this.mouseDownTrackControl)
                    {
                        this.ParentTrackControl.Children.Remove(this);
                        this.mouseDownTrackControl.Children.Add(this);
                        this.ParentTrackControl = this.mouseDownTrackControl;
                    }
                }
            }
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            CompleteMouseMoveAction(MouseMoveAction.Reset);
            ShowProperties();
        }

        private void ShowProperties()
        {
            var dialog = EditPartDialog.Create(Env.MainWindow, this.Part);
            if (dialog.ShowDialog() ?? false)
            {
                Env.Song.OnPartChanged(this.Part);
            }
        }

        private void DeletePart()
        {
            Env.Song.DeletePart(this.Part);
        }

        private void SetProperties()
        {
            this.grid.Background = new SolidColorBrush(this.Part.IsReference ? Colors.RefPart : Colors.Part);
            this.rightHandle.Cursor = this.Part.IsReference ? Cursors.SizeAll : Cursors.SizeWE;

            var left = part.Start / (double)Env.TimePerPixel;
            this.Width = part.Length / (double)Env.TimePerPixel;
            Canvas.SetLeft(this, left);
            this.titleLabel.Content = this.Part.Title;
        }
    }
}
