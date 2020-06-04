﻿using System;
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

                this.DataContext = this.track.TrackGenerators.Select(g => new NamedObject<Generator>(g, Env.Song.GetScriptRef(g.ScriptId)?.Name ?? "(illegal script)"));
            }
        }

        public EditTrackDialog()
        {
            InitializeComponent();

            this.okButton.Click += (_, __) => OK();
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

        private void TrackGeneratorsListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowGeneratorProperties((this.trackGeneratorsListView.SelectedItem as NamedObject<Generator>).Object);
            }
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
