using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Win32;

namespace Randomizer.App
{
    public class BrowseFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public string InitialDirectory { get; }

        public string Filter { get; }

        public Window Owner { get; }

        public BrowseFileCommand(Window owner, string filter, string initialDirectory = null)
        {
            Owner = owner;
            Filter = filter;
            InitialDirectory = initialDirectory;
        }

        public bool CanExecute(object parameter)
            => parameter is TextBox;

        public void Execute(object parameter)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = Filter,
                InitialDirectory = InitialDirectory
            };

            if (dialog.ShowDialog(Owner) == true)
            {
                ((TextBox)parameter).Text = dialog.FileName;
            }
        }
    }
}