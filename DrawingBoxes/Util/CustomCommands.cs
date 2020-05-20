using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Input;

namespace VaporDAW
{
    static class CustomCommands
    {
        public static RoutedCommand PlaySong = new RoutedCommand();
        public static RoutedCommand ExitApp = new RoutedCommand();
        public static RoutedCommand CloseTab = new RoutedCommand();
    }
}
