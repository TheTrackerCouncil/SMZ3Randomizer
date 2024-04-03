using System;
using System.Windows;
using MSURandomizerUI.Controls;
using Randomizer.Data.Options;

namespace Randomizer.App.Windows;

public partial class MsuTrackWindow : Window, IDisposable
{
    private MsuCurrentPlayingTrackControl? _panel;
    private RandomizerOptions? _options;

    public MsuTrackWindow()
    {
        InitializeComponent();
        App.RestoreWindowPositionAndSize(this);
    }

    public void Init(MsuCurrentPlayingTrackControl panel, RandomizerOptions options)
    {
        _panel = panel;
        MainDockPanel.Children.Add(panel);
        _options = options;
        _options.GeneralOptions.DisplayMsuTrackWindow = true;
        _options.Save();
    }

    public void Close(bool isShuttingDown)
    {
        if (!isShuttingDown && _options != null)
        {
            _options.GeneralOptions.DisplayMsuTrackWindow = false;
            _options.Save();
        }
        Close();
    }

    public void Dispose()
    {
        _panel?.Dispose();
    }
}

