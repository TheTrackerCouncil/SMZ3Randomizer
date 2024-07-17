using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Extensions;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class SpriteDownloadWindow : Window
{
    private SpriteDownloadWindowService? _service;

    public SpriteDownloadWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new SpriteDownloadWindowViewModel
            {
                NumCompleted = 5, NumTotal = 10
            };
        }
        else
        {
            _service = this.GetControlService<SpriteDownloadWindowService>();
            DataContext = _service?.InitializeModel() ?? new SpriteDownloadWindowViewModel();
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _service?.CancelDownload();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

