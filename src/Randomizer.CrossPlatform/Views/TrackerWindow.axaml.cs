using System.IO;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using Randomizer.CrossPlatform.Services;
using Randomizer.Shared.Models;

namespace Randomizer.CrossPlatform.Views;

public partial class TrackerWindow : RestorableWindow
{
    private TrackerWindowService? _service;
    private GeneratedRom? _generatedRom;

    public TrackerWindow()
    {
        InitializeComponent();
    }

    public TrackerWindow(TrackerWindowService service)
    {
        _service = service;
        DataContext = _service.GetViewModel(this);
        InitializeComponent();
    }

    public GeneratedRom? Rom
    {
        get => _generatedRom;
        set
        {
            _generatedRom = value;
            if (value != null)
            {
                _service?.SetRom(value);
            }
        }
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "tracker-window.json");
    protected override int DefaultWidth => 800;
    protected override int DefualtHeight => 600;

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _service?.StartTracker();
    }
}

