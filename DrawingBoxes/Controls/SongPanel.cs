using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace VaporDAW
{
    public delegate void ZoomDelegate(bool zoomIn);
    public class SongPanel : DockPanel
    {
        public event ZoomDelegate Zoom;

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                this.Zoom?.Invoke(e.Delta > 0);
            }
        }
    }
}
