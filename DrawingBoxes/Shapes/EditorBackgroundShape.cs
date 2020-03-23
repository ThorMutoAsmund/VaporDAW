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

namespace VaporDAW
{
    public class EditorBackgroundShape : Shape
    {
        private Brush lineBrush = new SolidColorBrush(Color.FromRgb(150,150,150));

        public EditorBackgroundShape()
        {
            this.Stroke = this.lineBrush;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var c = Env.Song == null ? 0 : Env.Song.Tracks.Count;
                var figures = new List<PathFigure>();
                var y = Env.TrackHeight;
                while (y < this.Height && c > 0)
                {
                    var p1 = new Point(0.0d, y);
                    var p2 = new Point(this.Width, y);

                    figures.Add(new PathFigure(p1, new List<PathSegment>()
                    {
                        new LineSegment(p1, false),
                        new LineSegment(p2, true),
                    },true));

                    y += Env.TrackHeight;
                    c--;
                }

                return new PathGeometry(figures, FillRule.Nonzero, null);
            }
        }
    }

}
