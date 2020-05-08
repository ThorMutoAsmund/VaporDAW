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
    /// <summary>
    /// Interaction logic for ScriptSelectControl.xaml
    /// </summary>
    public partial class ScriptSelectControl : UserControl
    {
        private ScriptRef script;
        public ScriptRef Script
        {
            get => this.script;
            set
            {
                this.script = value;
                this.scriptTextBox.Text = script?.Name;
            }
        }

        public Action<ScriptRef> ScriptChanged;

        public ScriptSelectControl()
        {
            InitializeComponent();

            // Subscribe to events
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            var dialog = SelectScriptDialog.Create(Env.MainWindow, this.Script);
            if (dialog.ShowDialog() ?? false)
            {
                this.Script = dialog.SelectedScript;
                ScriptChanged?.Invoke(this.Script);
            }
        }
    }
}
