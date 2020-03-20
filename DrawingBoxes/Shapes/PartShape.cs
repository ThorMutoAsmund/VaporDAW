using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DrawingBoxes
{
    public class PartShape : Shape
    {
        private enum MouseDownMode
        {
            ResizeStart,
            ResizeEnd,
            Move,
            ChangeTrack
        }

        private const double cornerSize = 8;

        public Part Part { get; private set; }

        //public static readonly DependencyProperty TrackNoProperty = DependencyProperty.Register("TrackNo", typeof(int), typeof(PartShape));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PartShape));

        //public int TrackNo
        //{
        //    get { return (int)this.GetValue(TrackNoProperty); }
        //    set
        //    {
        //        this.SetValue(TrackNoProperty, value);
        //        Canvas.SetTop(this, this.TrackNo * Env.TrackHeight);
        //    }
        //}
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set
            {
                this.SetValue(TitleProperty, value);
            }
        }

        private Brush lineBrush = new SolidColorBrush(Color.FromRgb(80,80,80));
        private Brush lineSelectedBrush = new SolidColorBrush(Color.FromRgb(20, 20, 20));
        private Brush defaultBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private Brush activeBrush = new SolidColorBrush(Color.FromRgb(235, 235, 255));
        private bool mouseDown;
        private Point mouseDownPoint;
        private double leftAtMouseDown;
        private double widthAtMouseDown;

        private MouseDownMode currentMouseDownMode = MouseDownMode.Move;

        private MouseDownMode CurrentMouseDownMode
        {
            get => this.currentMouseDownMode;
            set
            {
                if (value != this.currentMouseDownMode)
                {
                    this.currentMouseDownMode = value;
                    switch (this.currentMouseDownMode)
                    {
                        case MouseDownMode.ResizeStart:
                            this.Cursor = Cursors.ScrollW;
                            break;
                        case MouseDownMode.ResizeEnd:
                            this.Cursor = Cursors.ScrollE;
                            break;
                        case MouseDownMode.ChangeTrack:
                            this.Cursor = Cursors.ScrollNS;
                            break;
                        case MouseDownMode.Move:
                            this.Cursor = Cursors.Hand;
                            break;
                    }
                }
            }
        }

        public static PartShape Create(Canvas canvas, Part part)
        {
            var partShape = new PartShape(part);
            canvas.Children.Add(partShape);
            return partShape;
        }

        public PartShape(Part part)
        {
            this.Part = part;
            this.AllowDrop = true;
            this.Width = part.Length / Env.CanvasTimePerPixel;
            var left = part.Start / Env.CanvasTimePerPixel - Env.CanvasStartTime;
            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, Env.Song.GetPartTrackNo(this.Part) * Env.TrackHeight);

            this.Fill = this.defaultBrush;
            this.Height = Env.TrackHeight;
            this.Stroke = this.lineBrush;
            this.StrokeThickness = 1;
            this.MouseEnter += (s, e) =>
            {
                this.Stroke = this.lineSelectedBrush;
                this.StrokeThickness = 2;
            };
            this.MouseLeave += (s, e) =>
            {
                this.Stroke = this.lineBrush;
                this.StrokeThickness = 1;
            };
            this.MouseDown += (s, e) =>
            {
                Env.Canvas.MouseMove += CanvasMouseMove;
                Env.Canvas.MouseUp += CanvasMouseUp;
                Env.Canvas.MouseLeave += CanvasMouseLeave;

                if (e.ChangedButton == MouseButton.Left)
                {
                    var p = e.GetPosition(Env.Canvas);
                    this.mouseDown = true;
                    this.mouseDownPoint = p;
                    this.widthAtMouseDown = this.Width;
                    this.leftAtMouseDown = Canvas.GetLeft(this);
                }
            };
            this.MouseMove += (s, e) =>
            {
                if (!this.mouseDown)
                {
                    var p = e.GetPosition(this);

                    if (p.X <= cornerSize && p.Y >= Height - cornerSize)
                    {
                        this.CurrentMouseDownMode = MouseDownMode.ResizeStart;
                    }
                    else if (p.X >= Width - cornerSize && p.Y >= Height - cornerSize)
                    {
                        this.CurrentMouseDownMode = MouseDownMode.ResizeEnd;
                    }
                    else if (p.X >= Width / 2 - cornerSize && p.X <= Width / 2 + cornerSize && p.Y >= Height - cornerSize)
                    {
                        this.CurrentMouseDownMode = MouseDownMode.ChangeTrack;
                    }
                    else
                    {
                        this.CurrentMouseDownMode = MouseDownMode.Move;
                    }
                }
            };
            this.DragEnter += Shape_DragEnterOver;
            this.DragOver += Shape_DragEnterOver;
            this.DragLeave += Shape_DragLeave;

            this.Drop += Shape_Drop;
        }

        private void CanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            Env.Canvas.MouseMove -= CanvasMouseMove;
            Env.Canvas.MouseUp -= CanvasMouseUp;
            Env.Canvas.MouseLeave -= CanvasMouseLeave;
            this.mouseDown = false;
        }
        private void CanvasMouseLeave(object sender, MouseEventArgs e)
        {
            Env.Canvas.MouseMove -= CanvasMouseMove;
            Env.Canvas.MouseUp -= CanvasMouseUp;
            Env.Canvas.MouseLeave -= CanvasMouseLeave;
            this.mouseDown = false;
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(Env.Canvas);
            switch (this.CurrentMouseDownMode)
            {
                case MouseDownMode.Move:
                    Move(p);
                    break;
                case MouseDownMode.ResizeStart:
                    ResizeStart(p);
                    break;
                case MouseDownMode.ResizeEnd:
                    ResizeEnd(p);
                    break;
                case MouseDownMode.ChangeTrack:
                    ChangeTrack(p);
                    break;
            }
        }

        private void Shape_DragEnterOver(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetDataPresent("sample") || e.Data.GetDataPresent("script")))
            {
                return;
            }
            this.Fill = this.activeBrush;
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void Shape_DragLeave(object sender, DragEventArgs e)
        {
            this.Fill = this.defaultBrush;
        }

        private void Shape_Drop(object sender, DragEventArgs e)
        {
            if (Env.Song == null)
            {
                return;
            }

            if (e.Data.GetDataPresent("sample"))
            {
                var sampleName = e.Data.GetData("sample") as string;
                Env.Song.AddSampleToPart(this.Part, sampleName);
            }
            else if (e.Data.GetDataPresent("script"))
            {
                var scriptName = e.Data.GetData("script") as string;

                Env.Song.AddScriptToPart(this.Part, scriptName);
            }

            this.Fill = this.defaultBrush;
            e.Handled = true;
        }

        private void Move(Point p)
        {
            var x = Math.Max(0d, this.leftAtMouseDown + p.X - this.mouseDownPoint.X);
            Canvas.SetLeft(this, x);
        }
        private void ResizeStart(Point p)
        {
            var d = this.mouseDownPoint.X - p.X;
            var x = Math.Max(0d, this.leftAtMouseDown - d);
            this.Width = this.widthAtMouseDown + this.leftAtMouseDown - x;

            Canvas.SetLeft(this, x);
        }
        private void ResizeEnd(Point p)
        {
            var d = p.X - this.mouseDownPoint.X;
            this.Width = this.widthAtMouseDown + d;
        }
        private void ChangeTrack(Point p)
        {
            var trackNo = ((int)p.Y / (int)Env.TrackHeight);
            Env.Song.ChangePartTrack(this.Part, trackNo);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var p1 = new Point(0.0d, 0.0d);
                var p2 = new Point(this.Width, 0.0d);
                var p3 = new Point(this.Width, this.Height);
                var p4 = new Point(0, this.Height);
                var cpl1 = new Point(0.0d + cornerSize, Height);
                var cpl2 = new Point(0.0d, Height - cornerSize);
                var cpr1 = new Point(Width, Height - cornerSize);
                var cpr2 = new Point(Width - cornerSize, Height);
                var cpm1 = new Point(Width / 2 - cornerSize / 2, Height);
                var cpm2 = new Point(Width / 2 - cornerSize / 2, Height - cornerSize);
                var cpm3 = new Point(Width / 2 + cornerSize / 2, Height - cornerSize);
                var cpm4 = new Point(Width / 2 + cornerSize / 2, Height);

                var box = new PathFigure(p1, new List<PathSegment>()
                {
                    new LineSegment(p1, false),
                    new LineSegment(p2, true),
                    new LineSegment(p3, true),
                    new LineSegment(p4, true),
                },
                true);
                var lcorner = new PathFigure(cpl1, new List<PathSegment>()
                {
                    new LineSegment(cpl2, true),
                },
                false);
                var rcorner = new PathFigure(cpr1, new List<PathSegment>()
                {
                    new LineSegment(cpr2, true),
                },
                false);
                var handle = new PathFigure(cpm1, new List<PathSegment>()
                {
                    new LineSegment(cpm2, true),
                    new LineSegment(cpm3, true),
                    new LineSegment(cpm4, true),
                },
                false);

                return new PathGeometry(new List<PathFigure>() { box, lcorner, rcorner, handle }, FillRule.Nonzero, null);
            }
        }

    }

}
