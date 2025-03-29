using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Extensions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI;

public partial class TrackerSpeechWindow : RestorableWindow
{
    private readonly TrackerSpeechWindowService? _service;
    private bool _isShuttingDown;

    public TrackerSpeechWindow()
    {
        InitializeComponent();
        DataContext = new TrackerSpeechWindowViewModel();
    }

    public TrackerSpeechWindow(TrackerSpeechWindowService service)
    {
        _service = service;
        InitializeComponent();
        DataContext = service.GetViewModel();
    }

    public void Close(bool isShuttingDown)
    {
        _isShuttingDown = isShuttingDown;
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _service?.StopTimer();
        if (!_isShuttingDown && e.CloseReason != WindowCloseReason.OwnerWindowClosing && e.CloseReason != WindowCloseReason.ApplicationShutdown && e.CloseReason != WindowCloseReason.OSShutdown)
        {
            _service?.SaveOpenStatus(false);
        }
        base.OnClosing(e);
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "speech-window.json");

    protected override int DefaultWidth => 250;

    protected override int DefaultHeight => 250;

    private async void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        await this.SetClipboardAsync(_service?.GetBackgroundHex());
    }
}
