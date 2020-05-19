using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Input;

namespace VaporDAW
{
    public class SimpleCommand : ICommand
    {
        public event EventHandler<object> Executed;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.Executed?.Invoke(this, parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
