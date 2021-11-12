using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Randomizer.App.Controls
{
    /// <summary>
    /// Interaction logic for FileSystemInput.xaml
    /// </summary>
    public partial class FileSystemInput : UserControl
    {
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(FileSystemInput), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(FileSystemInput), new PropertyMetadata("All files (*.*)|*.*"));

        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register("DialogTitle", typeof(string), typeof(FileSystemInput), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsFolderPickerProperty =
            DependencyProperty.Register("IsFolderPicker", typeof(bool), typeof(FileSystemInput), new PropertyMetadata(false));

        public FileSystemInput()
        {
            InitializeComponent();
        }

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public string DialogTitle
        {
            get => (string)GetValue(DialogTitleProperty);
            set => SetValue(DialogTitleProperty, value);
        }

        public bool IsFolderPicker
        {
            get => (bool)GetValue(IsFolderPickerProperty);
            set => SetValue(IsFolderPickerProperty, value);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsFolderPicker)
                BrowseFolder();
            else
                BrowseFile();
        }

        private void BrowseFile()
        {
            var folderPath = System.IO.Path.GetDirectoryName(Path);
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = Filter,
                Title = DialogTitle,
                InitialDirectory = Directory.Exists(folderPath) ? folderPath : null,
                FileName = Path
            };

            var owner = Window.GetWindow(this);
            if (dialog.ShowDialog(owner) == true)
            {
                Path = dialog.FileName;
            }
        }

        private void BrowseFolder()
        {
            using var dialog = new CommonOpenFileDialog
            {
                EnsurePathExists = true,
                Title = DialogTitle,
                InitialDirectory = Directory.Exists(Path) ? Path : null,
                IsFolderPicker = true
            };

            var owner = Window.GetWindow(this);
            if (dialog.ShowDialog(owner) == CommonFileDialogResult.Ok)
            {
                Path = dialog.FileName;
            }
        }
    }
}
