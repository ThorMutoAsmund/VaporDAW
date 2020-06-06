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
    public partial class EditStringDialog : Window
    {
        public string Text
        {
            get => this.mainTextBox.Text;
            set
            {
                this.mainTextBox.Text = value;
                this.mainTextBox.SelectAll();
            }
        }


        public EditStringDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => this.DialogResult = true;
            this.mainTextBox.Focus();
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
