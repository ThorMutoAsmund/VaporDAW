using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VaporDAW
{
    /// <summary>
    /// Interaction logic for SampleList.xaml
    /// </summary>
    public partial class SampleList : UserControl
    {
        public SampleRef SelectedSample { get; set; }

        private Point startPoint;

        public SampleList()
        {
            InitializeComponent();

            Env.Watchers.SamplesListChanged += stringList =>
            {
                this.DataContext = stringList;
            };
        }

        private void SamplesListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.startPoint = e.GetPosition(null);
        }

        private void SamplesListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            var mousePos = e.GetPosition(null);
            var diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.samplesListView.CheckDragDropMove<string>(diff, e.OriginalSource, "sample");
            }
        }
    }
}
