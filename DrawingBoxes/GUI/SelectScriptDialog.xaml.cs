using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VaporDAW
{
    /// <summary>
    /// Interaction logic for EditStringDialog.xaml
    /// </summary>
    public partial class SelectScriptDialog : Window
    {
        public ScriptRef SelectedScript { get; set; }

        public SelectScriptDialog()
        {
            InitializeComponent();

            this.DataContext = Env.Song.Scripts;

            this.okButton.Click += (sender, e) => SelectItem();
            this.MouseDoubleClick += (sender, e) => SelectItem();
        }

        private void SelectScriptDialog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SelectItem()
        {
            if (this.SelectedScript != null)
            {
                this.DialogResult = true;
            }
        }

        public static SelectScriptDialog Create(Window owner, ScriptRef script)
        {
            var dialog = new SelectScriptDialog()
            {
                Owner = owner,
                SelectedScript = script
            };

            return dialog;
        }
    }
}
