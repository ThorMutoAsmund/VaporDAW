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
using System.Windows.Shapes;

namespace DrawingBoxes
{
    /// <summary>
    /// Interaction logic for EditStringDialog.xaml
    /// </summary>
    public partial class EditStringDialog : Window
    {
        private string text;
        public string Text
        {
            get => this.text;
            set
            {
                this.text = value;
                this.mainTextBox.Text = value;
            }
        }


        public EditStringDialog()
        {
            InitializeComponent();

            this.okButton.Click += (_, __) => this.DialogResult = true;
        }

        public static EditStringDialog Create(Window owner, string windowTitle = "Enter text", string label = "Text", string text = "")
        {
            var dialog = new EditStringDialog()
            {
                Owner = owner,
                Title = windowTitle,
                Text = text,
            };

            dialog.mainLabel.Content = label;

            return dialog;
        }
    }
}
