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

        private TrackControl ParentTrackControl 
        {
            get => this.parentTrackControl;
            set
            {
                if (this.parentTrackControl != value)
                {
                    //if (this.parentTrackControl != null)
                    //{
                    //    this.parentTrackControl.MouseUp -= (sender, e) => TrackMouseUp();
                    //}
                    this.parentTrackControl = value;
                    //this.parentTrackControl.MouseUp += (sender, e) => TrackMouseUp();
                }
            }
        }

        private TrackControl parentTrackControl;
        private Point contextMousePosition;
        private bool isSelected;
        private bool isMouseDown;
        private bool didMove;
        private double mouseDownLeft;

        public PartControl()
        {
            InitializeComponent();

            this.grid.Background = new SolidColorBrush(Colors.Part);
            this.border.BorderBrush = new SolidColorBrush(Colors.PartBorder);
                
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

            this.contextMousePosition = e.GetPosition(this.ParentTrackControl);
            this.isMouseDown = true;
            this.mouseDownLeft = Canvas.GetLeft(this);
            this.didMove = false;
            Env.TrackPanel.MouseMove += TrackPanelMouseMove;

            Select();
        }

        private void TrackPanelMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this.ParentTrackControl);
            Canvas.SetLeft(this, this.mouseDownLeft + (position.X - this.contextMousePosition.X));
            this.didMove = true;
        }

        private void MouseHookMouseUp()
        {
            if (this.isMouseDown)
            {
                this.isMouseDown = false;
                Env.TrackPanel.MouseMove -= TrackPanelMouseMove;

                if (this.didMove)
                {
                    var newLeft = Canvas.GetLeft(this);
                    Part.Start = newLeft * (double)Env.TimePerPixel;
                    Env.Song.OnPartChanged(this.Part);
                }
            }
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
