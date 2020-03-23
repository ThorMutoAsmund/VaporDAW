using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace VaporDAW
{
    /// <summary>
    /// Interaction logic for SampleList.xaml
    /// </summary>
    public partial class ScriptList : UserControl
    {
        public ScriptRef SelectedScript { get; set; }

        private Point startPoint;

        public ScriptList()
        {
            InitializeComponent();

            Env.Watchers.ScriptsListChanged += stringList =>
            {
                this.DataContext = stringList;
            };
        }

        private void ScriptsListView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Env.Song == null)
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                var fileName = (sender as TextBlock).Text;
                Env.Song.EditScript(fileName);
            }
        }

        private void ScriptsListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.startPoint = e.GetPosition(null);
        }

        private void ScriptsListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            var mousePos = e.GetPosition(null);
            var diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.scriptsListView.CheckDragDropMove<string>(diff, e.OriginalSource, "script");
            }
        }
    }
}
