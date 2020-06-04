using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace VaporDAW
{
    public class EditPartDataContext
    {
        public IEnumerable<NamedObject<Generator>> PartGenerators { get; set; }
        public IEnumerable<NamedObject<Generator>> Generators { get; set; }
    }
    public partial class EditPartDialog : Window
    {
        private Part part;
        public Part Part 
        { 
            get => this.part;
            private set
            {
                this.part = value;

                this.titleTextBox.Text = this.Part.Title;
                this.startTextBox.Value = this.Part.SampleStart;
                this.lengthTextBox.Value = this.Part.SampleLength;

                this.titleTextBox.IsReadOnly = this.Part.IsReference;
                this.lengthTextBox.IsReadOnly = this.Part.IsReference;
                if (this.Part.IsReference)
                {
                    this.titleTextBox.Background = SystemColors.ControlBrush;
                    this.lengthTextBox.Background = SystemColors.ControlBrush;
                }

                this.DataContext = new EditPartDataContext()
                {
                    PartGenerators = this.part.PartGenerators.Select(g => new NamedObject<Generator>(g, Env.Song.GetScriptRef(g.ScriptId)?.Name ?? "(illegal script)")),
                    Generators = this.part.Generators.Select(g => new NamedObject<Generator>(g, Env.Song.GetScriptRef(g.ScriptId)?.Name ?? "(illegal script)"))
                };
            }
        }

        public EditPartDialog()
        {
            InitializeComponent();

            this.startTextBox.TextChanged += (sender, e) => IntegerValueChanged(this.startTextBox, this.startTimeTextBox);
            this.lengthTextBox.TextChanged += (sender, e) => IntegerValueChanged(this.lengthTextBox, this.lengthTimeTextBox);
            this.startTimeTextBox.TextChanged += (sender, e) => DoubleValueChanged(this.startTextBox, this.startTimeTextBox);
            this.lengthTimeTextBox.TextChanged += (sender, e) => DoubleValueChanged(this.lengthTextBox, this.lengthTimeTextBox);

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

        private void PartGeneratorsListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowGeneratorProperties((this.partGeneratorsListView.SelectedItem as NamedObject<Generator>).Object);
            }
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
            this.Part.SampleStart = (int)(this.startTextBox.Value * Env.Song.SampleRate);
            this.Part.SampleLength = (int)(this.lengthTextBox.Value * Env.Song.SampleRate);

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

        private bool integerTextBoxesDisabled;
        private bool doubleTextBoxesDisabled;
        private void IntegerValueChanged(IntegerTextBox integerTextBox, DobuleTextBox doubleTextBox)
        {
            if (integerTextBoxesDisabled)
            {
                return;
            }
            this.doubleTextBoxesDisabled = true;
            doubleTextBox.Value = integerTextBox.Value / Env.Song.SampleRate;
            Console.WriteLine(integerTextBox.Value / Env.Song.SampleRate);
            this.doubleTextBoxesDisabled = false;
        }
        private void DoubleValueChanged(IntegerTextBox integerTextBox, DobuleTextBox doubleTextBox)
        {
            if (doubleTextBoxesDisabled)
            {
                return;
            }
            this.integerTextBoxesDisabled = true;
            integerTextBox.Value = (int)(doubleTextBox.Value * Env.Song.SampleRate);
            this.integerTextBoxesDisabled = false;
        }
    }
}
