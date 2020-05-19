using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Input;

namespace VaporDAW
{
    static class CustomCommands
    {
        static CustomCommands()
        {
            exitCommand = new RoutedCommand("Exit", typeof(CustomCommands));
            closeTabCommand = new RoutedCommand("CloseTab", typeof(CustomCommands));
        }

        public static RoutedCommand Exit
        {
            get
            {
                return (exitCommand);
            }
        }
        static RoutedCommand exitCommand;

        public static RoutedCommand CloseTab
        {
            get
            {
                return (closeTabCommand);
            }
        }
        static RoutedCommand closeTabCommand;
    }
}
