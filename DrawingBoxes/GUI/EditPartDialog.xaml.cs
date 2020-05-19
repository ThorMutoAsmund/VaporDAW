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
    public partial class EditPartDialog : Window
    {
        private Part part;
        public Part Part 
        { 
            get => this.part;
            private set
            {
                this.part = value;

                this.scriptSelectControl.Script = Env.Song.GetScriptRef(this.Part.ScriptId);
                this.titleTextBox.Text = this.Part.Title;
                this.startTextBox.DoubleValue = this.Part.Start;
                this.lengthTextBox.DoubleValue = this.Part.Length;

                this.scriptSelectControl.IsReadOnly = this.Part.IsReference;
                this.titleTextBox.IsReadOnly = this.Part.IsReference;
                this.lengthTextBox.IsReadOnly = this.Part.IsReference;
                if (this.Part.IsReference)
                {
                    this.titleTextBox.Background = SystemColors.ControlBrush;
                    this.lengthTextBox.Background = SystemColors.ControlBrush;
                }

                this.DataContext = this.part.Generators.Select(g => new NamedObject<Generator>(g, Env.Song.GetScriptRef(g.ScriptId)?.Name ?? "(illegal script)"));
            }
        }

        public EditPartDialog()
        {
            InitializeComponent();

            this.okButton.Click += (_, __) => OK();
            this.titleTextBox.Focus();
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

        private void GeneratorsListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowGeneratorProperties((this.generatorsListView.SelectedItem as NamedObject<Generator>).Object);
            }
        }

        private void OK()
        {
            this.Part.Title = this.titleTextBox.Text;
            this.Part.Start = this.startTextBox.DoubleValue;
            this.Part.Length = this.lengthTextBox.DoubleValue;
            this.Part.ScriptId = this.scriptSelectControl.Script.Id;

            this.DialogResult = true;
        }

        private void ShowGeneratorProperties(Generator generator)
        {
            var dialog = EditGeneratorDialog.Create(Env.MainWindow, generator);
            if (dialog.ShowDialog() ?? false)
            {
                Env.Song.OnGeneratorChanged(generator);
            }
        }
    }
}
