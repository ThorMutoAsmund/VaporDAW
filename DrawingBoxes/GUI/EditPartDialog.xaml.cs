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

                UpdateDataContext();
            }
        }

        private Part part;
        private bool integerTextBoxesDisabled;
        private bool doubleTextBoxesDisabled;

        public EditPartDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => OK();

            this.deletePartGeneratorMenuItem.Click += (sender, e) =>
                DeletePartGenerator((this.partGeneratorsListView.SelectedItem as NamedObject<Generator>)?.Object);
            this.editPartGeneratorMenuItem.Click += (sender, e) =>
                EditGenerator((this.partGeneratorsListView.SelectedItem as NamedObject<Generator>)?.Object);

            this.deleteGeneratorMenuItem.Click += (sender, e) =>
                DeleteGenerator((this.generatorsListView.SelectedItem as NamedObject<Generator>)?.Object);
            this.editGeneratorMenuItem.Click += (sender, e) =>
                EditGenerator((this.generatorsListView.SelectedItem as NamedObject<Generator>)?.Object);

            this.startTextBox.TextChanged += (sender, e) => IntegerValueChanged(this.startTextBox, this.startTimeTextBox);
            this.lengthTextBox.TextChanged += (sender, e) => IntegerValueChanged(this.lengthTextBox, this.lengthTimeTextBox);
            this.startTimeTextBox.TextChanged += (sender, e) => DoubleValueChanged(this.startTextBox, this.startTimeTextBox);
            this.lengthTimeTextBox.TextChanged += (sender, e) => DoubleValueChanged(this.lengthTextBox, this.lengthTimeTextBox);

            this.partGeneratorsListView.ItemDoubleClicked += PartGeneratorsListView_ItemDoubleClicked;
            this.generatorsListView.ItemDoubleClicked += GeneratorsListView_ItemDoubleClicked;

            Song.PartChanged += part => { if (part == this.Part) UpdateDataContext(); };

            Env.Song.AddScriptListToMenuItem(this.addGeneratorMenuItem, AddGenerator);
            Env.Song.AddScriptListToMenuItem(this.addPartGeneratorMenuItem, AddPartGenerator);

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

        private void UpdateDataContext()
        {
            this.DataContext = new EditPartDataContext()
            {
                PartGenerators = this.part.PartGenerators.Select(g => new NamedObject<Generator>(g, Env.Song.GetScriptRef(g.ScriptId)?.Name ?? "(illegal script)", this.part.PartGenerators.IndexOf(g))),
                Generators = this.part.Generators.Select(g => new NamedObject<Generator>(g, Env.Song.GetScriptRef(g.ScriptId)?.Name ?? "(illegal script)", this.part.Generators.IndexOf(g)))
            };
        }

        private void PartGeneratorsListView_ItemDoubleClicked(object sender, object item)
        {
            EditGenerator((item as NamedObject<Generator>)?.Object);
        }

        private void GeneratorsListView_ItemDoubleClicked(object sender, object item)
        {
            EditGenerator((item as NamedObject<Generator>)?.Object);
        }

        private void OK()
        {
            this.Part.Title = this.titleTextBox.Text;
            this.Part.SampleStart = (int)(this.startTextBox.Value * Env.Song.SampleRate);
            this.Part.SampleLength = (int)(this.lengthTextBox.Value * Env.Song.SampleRate);

            this.DialogResult = true;
        }

        private void EditGenerator(Generator generator)
        {
            if (generator == null)
            {
                return;
            }

            var dialog = EditGeneratorDialog.Create(Env.MainWindow, generator);
            if (dialog.ShowDialog() ?? false)
            {
                Env.Song.OnGeneratorChanged(generator);
            }
        }

        private void IntegerValueChanged(IntegerTextBox integerTextBox, DobuleTextBox doubleTextBox)
        {
            if (integerTextBoxesDisabled)
            {
                return;
            }
            this.doubleTextBoxesDisabled = true;
            doubleTextBox.Value = integerTextBox.Value / Env.Song.SampleRate;
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

        private void DeletePartGenerator(Generator generator)
        {
            if (generator == null)
            {
                return;
            }

            this.part.DeletePartGenerator(generator);
        }

        private void DeleteGenerator(Generator generator)
        {
            if (generator == null)
            {
                return;
            }

            this.part.DeleteGenerator(generator);
        }

        private void AddPartGenerator(ScriptRef scriptRef)
        {
            if (scriptRef == null)
            {
                scriptRef = Dialogs.AddNewScript(this);
            }
            if (scriptRef != null)
            {
                this.Part.AddPartGenerator(scriptRef);
                Env.Song.OnPartChanged(this.Part);
            }
        }

        private void AddGenerator(ScriptRef scriptRef)
        {
            if (scriptRef == null)
            {
                scriptRef = Dialogs.AddNewScript(this);
            }
            if (scriptRef != null)
            {
                this.Part.AddGenerator(scriptRef);
                Env.Song.OnPartChanged(this.Part);
            }
        }

        private void partGeneratorsListView_ItemDoubleClicked_1(object sender, object e)
        {

        }

        private void generatorsListView_ButtonClick(object sender, EventArgs e)
        {

        }
    }
}
