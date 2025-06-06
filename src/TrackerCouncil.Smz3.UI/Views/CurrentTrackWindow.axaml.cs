using System.IO;
using Avalonia.Controls;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.UI.Services;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class CurrentTrackWindow : RestorableWindow
{
    private readonly CurrentTrackWindowService? _service;
    private bool _isShuttingDown;

    public CurrentTrackWindow()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            _service = IControlServiceFactory.GetControlService<CurrentTrackWindowService>();
            _service?.Save(true);
        }
    }

    public void Close(bool isShuttingDown)
    {
        _isShuttingDown = isShuttingDown;
        Close();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_isShuttingDown || e.CloseReason == WindowCloseReason.OwnerWindowClosing) return;
        _service?.Save(false);
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "msu-monitor.json");
    protected override int DefaultWidth => 600;
    protected override int DefaultHeight => 400;
}

