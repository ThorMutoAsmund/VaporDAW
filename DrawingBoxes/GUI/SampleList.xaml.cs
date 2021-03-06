﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VaporDAW
{
    /// <summary>
    /// Interaction logic for SampleList.xaml
    /// </summary>
    public partial class SampleList : UserControl
    {
        private Point startPoint;

        public SampleList()
        {
            InitializeComponent();

            Env.Watchers.SamplesListChanged += stringList =>
            {
                this.DataContext = stringList;
            };
            Song.ProjectLoaded += loaded =>
            {
                if (!loaded)
                {
                    this.DataContext = null;
                }
            };
        }

        private void SamplesListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                AudioPlaybackEngine.Instance.StopPlayback();

                var sampleName = this.samplesListView.SelectedItem as string;
                AudioPlaybackEngine.Instance.PlaySound(Path.Combine(Env.Song.SamplesPath, sampleName));
            }

            this.startPoint = e.GetPosition(null);
        }

        private void SamplesListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the current mouse position
                var mousePos = e.GetPosition(null);
                var diff = startPoint - mousePos;

                this.samplesListView.CheckDragDropMove<string>(diff, e.OriginalSource, "sample");
            }
        }
    }
}
