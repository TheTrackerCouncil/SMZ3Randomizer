using System.Windows;
using Randomizer.Data.Services;
using Randomizer.Data.ViewModels;

namespace Randomizer.App.Windows;

public partial class OptionsWindow : Window
{
    private OptionsWindowService _service;
    private OptionsWindowViewModel _model;
    private SpriteDownloaderWindow? _spriteDownloaderWindow;

    public OptionsWindow(OptionsWindowService service)
    {
        _service = service;
        InitializeComponent();
        DataContext = _model = _service.GetViewModel();

        service.TwitchError += (sender, handler) =>
        {
            MessageBox.Show(this, handler.Error, "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        service.SpriteDownloadStarted += (sender, args) =>
        {
            _spriteDownloaderWindow = new SpriteDownloaderWindow();
            _spriteDownloaderWindow.Show();
        };

        service.SpriteDownloadEnded += (sender, args) =>
        {
            if (_spriteDownloaderWindow?.IsVisible == true)
            {
                _spriteDownloaderWindow?.Close();
            }
        };
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        _service.SaveViewModel();
        Close();
    }

}

