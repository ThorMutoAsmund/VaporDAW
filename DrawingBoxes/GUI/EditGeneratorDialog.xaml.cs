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
    public partial class EditGeneratorDialog : Window
    {
        private Generator generator;
        public Generator Generator 
        { 
            get => this.generator;
            private set
            {
                this.generator = value;

                this.scriptSelectControl.Script = Env.Song.GetScriptRef(this.Generator.ScriptId);
                //this.titleTextBox.Text = this.Part.Title;
                //this.startTextBox.DoubleValue = this.Part.Start;
                //this.lengthTextBox.DoubleValue = this.Part.Length;

                //this.scriptSelectControl.IsReadOnly = this.Part.IsReference;
                //this.titleTextBox.IsReadOnly = this.Part.IsReference;
                //this.lengthTextBox.IsReadOnly = this.Part.IsReference;
                //if (this.Part.IsReference)
                //{
                //    this.titleTextBox.Background = SystemColors.ControlBrush;
                //    this.lengthTextBox.Background = SystemColors.ControlBrush;
                //}

                //this.DataContext = this.part.Generators.Select(g => new NamedObject<Generator>(g, Env.Song.GetScriptRef(g.ScriptId)?.Name ?? "(illegal script)"));
            }
        }

        public EditGeneratorDialog()
        {
            InitializeComponent();

            this.okButton.Click += (_, __) => OK();
            //this.titleTextBox.Focus();
        }

        public static EditGeneratorDialog Create(Window owner, Generator generator)
        {
            var dialog = new EditGeneratorDialog()
            {
                Owner = owner,
                Generator = generator
            };

            return dialog;
        }

        private void OK()
        {
            //this.Generator.Title = this.titleTextBox.Text;
            //this.Generator.Start = this.startTextBox.DoubleValue;
            //this.Generator.Length = this.lengthTextBox.DoubleValue;
            this.Generator.ScriptId = this.scriptSelectControl.Script.Id;

            this.DialogResult = true;
        }
    }
}
