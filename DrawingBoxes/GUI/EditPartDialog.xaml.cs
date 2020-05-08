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

namespace VaporDAW
{
    public partial class EditPartDialog : Window
    {
        private Part part;
        public Part Part 
        { 
            get => this.part;
            private set
            {
                if (Env.Song == null)
                {
                    return;
                }

                this.part = value;

                this.scriptSelectControl.Script = Env.Song.GetScriptRef(this.Part.ScriptId);
                this.titleTextBox.Text = this.Part.Title;
                this.startTextBox.DoubleValue = this.Part.Start;
                this.lengthTextBox.DoubleValue = this.Part.Length;
            }
        }

        public EditPartDialog()
        {
            InitializeComponent();

            this.okButton.Click += (_, __) => OK();
            this.titleTextBox.Focus();
        }

        private void OK()
        {
            this.Part.Title = this.titleTextBox.Text;
            this.Part.Start = this.startTextBox.DoubleValue;
            this.Part.Length = this.lengthTextBox.DoubleValue;
            this.Part.ScriptId = this.scriptSelectControl.Script.Id;

            this.DialogResult = true;
        }

        public static EditPartDialog Create(Window owner, Part part)
        {
            var dialog = new EditPartDialog()
            {
                Owner = owner,
                Part = part
            };

            return dialog;
        }
    }
}
