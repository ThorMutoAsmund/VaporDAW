using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for TimeRulerControl.xaml
    /// </summary>
    public partial class TimeRulerControl : UserControl
    {
        public TimeRulerControl()
        {
            InitializeComponent();

            this.panel.Background = new SolidColorBrush(Colors.TimeRuler);

            Env.ViewChanged += Env_ViewChanged;
        }

        private void Env_ViewChanged(double startTime)
        {
            this.panel.StartTime = startTime;
        }
    }

    public class TimeRulerPanel : Panel
    {
        private double startTime = 0d;
        public double StartTime
        {
            get => this.startTime;
            set
            {
                this.startTime = value;
                InvalidateVisual();
            }
        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            base.OnRender(dc);

            var pen = new Pen(Brushes.Black, 1);
            Typeface typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);            
            
            var step = (double)Env.TimePerPixel * 100d;
            var firstTime = (Math.Truncate(this.StartTime / step) + 1) * step;

            var x = (firstTime - this.StartTime) / (double)Env.TimePerPixel;
            while (x < this.ActualWidth)
            {
                FormattedText text = new FormattedText(firstTime.ToString("0.##"), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12, Brushes.Black);
                dc.DrawLine(pen, new Point(x, this.ActualHeight - 2), new Point(x, this.ActualHeight / 2));
                dc.DrawText(text, new Point(x+2, 6));
                firstTime += step;
                x = (firstTime - this.StartTime) / (double)Env.TimePerPixel;
            }
        }
    }
}
