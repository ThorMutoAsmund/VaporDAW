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
    /// <summary>
    /// Interaction logic for NewProjectDialog.xaml
    /// </summary>
    public partial class NewProjectDialog : Window
    {
        private string projectPath;
        public string ProjectPath
        {
            get => this.projectPath;
            set
            {
                this.projectPath = value;
                this.projectPathTextBox.Text = value;
            }
        }

        public string SongName => this.songNameTextBox.Text;
        public int NumberOfTracks => int.TryParse(this.numberOfTracksTextBox.Text, out int numberOfTracks) ? numberOfTracks : 0;
        public double SampleFrequency => double.TryParse(this.sampleFrequencyTextBox.Text, out double sampleFrequency) ? sampleFrequency : 0d;
        public double SongLength => double.TryParse(this.songLengthTextBox.Text, out double songLength) ? songLength : 0d;
        public NewProjectDialog()
        {
            InitializeComponent();

            this.okButton.Click += (_, __) => this.DialogResult = true;
        }

        public static NewProjectDialog Create(Window owner, string projectPath)
        {
            var dialog = new NewProjectDialog()
            {
                Owner = owner,
                ProjectPath = projectPath
            };

            return dialog;
        }
    }
}
