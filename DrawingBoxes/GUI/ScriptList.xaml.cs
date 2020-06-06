using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VaporDAW
{
    /// <summary>
    /// Interaction logic for SampleList.xaml
    /// </summary>
    public partial class ScriptList : UserControl
    {
        private Point startPoint;

        public ScriptList()
        {
            InitializeComponent();

            Env.Watchers.ScriptsListChanged += stringList =>
            {
                this.DataContext = stringList;
            };
            Song.ProjectLoaded += loaded =>
            {
                if (!loaded)
                {
                    this.DataContext = null;
                }
            };

            this.scriptsListView.PreviewMouseLeftButtonDown += ScriptsListView_PreviewMouseLeftButtonDown;
            this.scriptsListView.PreviewMouseMove += ScriptsListView_PreviewMouseMove;
        }

        private void ScriptsListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var fileName = this.scriptsListView.SelectedItem as string;
                Env.Song.EditScript(fileName);
            }

            this.startPoint = e.GetPosition(null);
        }

        private void ScriptsListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the current mouse position
                var mousePos = e.GetPosition(null);
                var diff = startPoint - mousePos;

                this.scriptsListView.CheckDragDropMove<string>(diff, e.OriginalSource, "script");
            }
        }
    }
}
