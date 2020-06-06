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
    public partial class EditTrackDialog : Window
    {
        private Track track;
        public Track Track 
        { 
            get => this.track;
            private set
            {
                this.track = value;

                this.titleTextBox.Text = this.Track.Title;
                this.audibleCheckBox.IsChecked = this.Track.IsAudible;
                this.mutedCheckBox.IsChecked = this.Track.IsMuted;
                this.soloCheckBox.IsChecked = this.Track.IsSolo;

                UpdateDataContext();
            }
        }

        public EditTrackDialog()
        {
            InitializeComponent();


            this.okButton.Click += (sender, e) => OK();
            this.deleteTrackGeneratorMenuItem.Click += (sender, e) => 
                DeleteTrackGenerator((this.trackGeneratorsListView.SelectedItem as NamedObject<Generator>)?.Object);
            this.editTrackGeneratorMenuItem.Click += (sender, e) =>
                EditGenerator((this.trackGeneratorsListView.SelectedItem as NamedObject<Generator>)?.Object);

            this.trackGeneratorsListView.ItemDoubleClicked += (sender, e) => EditGenerator((e as NamedObject<Generator>)?.Object);
            
            Song.TrackChanged += track => { if (track == this.Track) UpdateDataContext(); };

            Env.Song.AddScriptListToMenuItem(this.addGeneratorMenuItem, AddTrackGenerator);

            this.titleTextBox.Focus();
        }

        private void OK()
        {
            this.Track.Title = this.titleTextBox.Text;
            this.Track.IsAudible = this.audibleCheckBox.IsChecked ?? false;
            this.Track.IsMuted = this.mutedCheckBox.IsChecked ?? false;
            this.Track.IsSolo = this.soloCheckBox.IsChecked ?? false;

            this.DialogResult = true;
        }

        public static EditTrackDialog Create(Window owner, Track track)
        {
            var dialog = new EditTrackDialog()
            {
                Owner = owner,
                Track = track
            };

            return dialog;
        }

        private void UpdateDataContext()
        {
            this.DataContext = this.track.TrackGenerators.Select(g => new NamedObject<Generator>(g, Env.Song.GetScriptRef(g.ScriptId)?.Name ?? "(illegal script)", this.track.TrackGenerators.IndexOf(g)));
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

        private void DeleteTrackGenerator(Generator generator)
        {
            if (generator == null)
            {
                return;
            }

            this.track.DeleteTrackGenerator(generator);
        }

        private void AddTrackGenerator(ScriptRef scriptRef)
        {
            if (scriptRef == null)
            {
                scriptRef = Dialogs.AddNewScript(this);
            }
            if (scriptRef != null)
            {
                this.Track.AddTrackGenerator(scriptRef);
                Env.Song.OnTrackChanged(this.Track);
            }
        }
    }
}
