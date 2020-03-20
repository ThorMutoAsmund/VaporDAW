using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace DrawingBoxes
{
    /// <summary>
    /// Interaction logic for ScriptTabItem.xaml
    /// </summary>
    public partial class ScriptTabItem : TabItem
    {
        public ScriptRef Script { get; private set; }

        public bool HasChanges { get; private set; }

        public string ScriptContent => this.avEditor.Text;

        public event Action RequestClose;

        public ScriptTabItem(ScriptRef script, string content)
        {
            InitializeComponent();

            this.Script = script;
            this.Header = script.FileName;

            this.avEditor.Text = content;

            this.avEditor.TextChanged += (sender, e) =>
            {
                this.HasChanges = true;
                this.Header = $"{script.FileName} *";
            };

            this.closeTabMenuItem.Click += (object sender, System.Windows.RoutedEventArgs e) =>
            {
                this.RequestClose?.Invoke();
            };
        }


        public void ClearHasChanges()
        {
            this.HasChanges = false;
            this.Header = this.Script.FileName;
        }
    }
}
