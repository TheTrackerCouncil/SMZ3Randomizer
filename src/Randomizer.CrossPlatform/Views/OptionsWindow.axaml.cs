using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using Randomizer.Data.Services;
using Randomizer.Data.ViewModels;

namespace Randomizer.CrossPlatform.Views;

public partial class OptionsWindow : ScalableWindow
{
    private OptionsWindowService? _service;
    private SpriteDownloadWindow? _spriteDownloadWindow;

    public OptionsWindow()
    {
        InitializeComponent();
        DataContext = new OptionsWindowViewModel();
    }

    public OptionsWindow(OptionsWindowService service)
    {
        InitializeComponent();
        _service = service;
        DataContext = _service.GetViewModel();

        var initialMessageParent = MessageWindow.GlobalParentWindow;
        MessageWindow.GlobalParentWindow = this;
        Closed += (sender, args) => MessageWindow.GlobalParentWindow = initialMessageParent;

        service.SpriteDownloadStarted += (sender, args) =>
        {
            _spriteDownloadWindow = new SpriteDownloadWindow();
            _spriteDownloadWindow.Closed += (o, eventArgs) =>
            {
                _spriteDownloadWindow = null;
            };
            _spriteDownloadWindow.ShowDialog(this);
        };

        service.SpriteDownloadEnded += (sender, args) =>
        {
            _spriteDownloadWindow?.Close();
        };

        service.TwitchError += (sender, handler) =>
        {
            var messageWindow = new MessageWindow(new MessageWindowRequest()
            {
                Message = handler.Error,
                Buttons = MessageWindowButtons.OK,
                Icon = MessageWindowIcon.Error,
                Title = "SMZ3 Cas’ Randomizer"
            });
            messageWindow.ShowDialog(this);
        };
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SaveViewModel();
        Close();
    }
}

