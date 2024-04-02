using System;
using System.Windows.Input;

namespace Randomizer.App
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

#pragma warning disable 67
        public event EventHandler? CanExecuteChanged;
#pragma warning restore 67

        public bool CanExecute(object? parameter)
            => _canExecute();

        public void Execute(object? parameter)
            => _execute();
    }
}
