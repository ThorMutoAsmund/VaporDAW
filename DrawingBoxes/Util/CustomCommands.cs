using System.Windows.Input;

namespace VaporDAW
{
    public static class CustomCommands
    {
        public static RoutedCommand PlaySong = new RoutedCommand();
        public static RoutedCommand StopSong = new RoutedCommand();
        public static RoutedCommand ExitApp = new RoutedCommand();
        public static RoutedCommand CloseTab = new RoutedCommand();
        public static RoutedCommand NewScript = new RoutedCommand();
        public static RoutedCommand ImportSamples = new RoutedCommand();
        public static RoutedCommand About = new RoutedCommand();
        public static RoutedCommand CheckForUpdates = new RoutedCommand();
        public static RoutedCommand Settings = new RoutedCommand();
        public static RoutedCommand AddTrack = new RoutedCommand();
        public static RoutedCommand ZoomIn = new RoutedCommand();
        public static RoutedCommand ZoomOut = new RoutedCommand();
    }
}
