using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MSURandomizerLibrary.Configs;
using MSURandomizerUI.Controls;
using Randomizer.Abstractions;
using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App.Windows;

public partial class MsuTrackWindow : Window, IDisposable
{
    private readonly DoubleAnimation _marquee = new();
    private CancellationTokenSource _cts = new();
    private TrackerBase? _tracker;
    private RandomizerOptions? _options;
    private Track? _currentTrack;
    private Msu? _currentMsu;
    private string? _outputText;
    private bool _shuttingDown;

    public MsuTrackWindow()
    {
        InitializeComponent();
        App.RestoreWindowPositionAndSize(this);
    }

    public void Init(MsuCurrentPlayingTrackControl panel)
    {
        MainDockPanel.Children.Add(panel);
    }


    public void Close(bool isShuttingDown)
    {
        _shuttingDown = isShuttingDown;
        Close();
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}

