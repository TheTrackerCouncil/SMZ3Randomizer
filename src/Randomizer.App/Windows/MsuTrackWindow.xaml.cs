using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MSURandomizerLibrary.Configs;
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
        _marquee.Completed += MarqueeOnCompleted;
        App.RestoreWindowPositionAndSize(this);
    }

    public void Init(TrackerBase tracker, RandomizerOptions options)
    {
        _tracker = tracker;
        _options = options;
        _tracker.TrackChanged += TrackerOnTrackChanged;
    }

    private void TrackerOnTrackChanged(object? sender, TrackChangedEventArgs e)
    {
        _currentTrack = e.Track;
        _currentMsu = e.Msu;
        _outputText = e.OutputText;
        DisplayText();
    }

    private void MsuTrackWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_options == null) return;
        _options.GeneralOptions.DisplayMsuTrackWindow = true;
        _options.Save();
    }

    private void DisplayText()
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(DisplayText);
            return;
        }

        if (_options == null || _currentTrack == null || _currentMsu == null) return;

        if (_options.GeneralOptions.MsuTrackDisplayStyle == MsuTrackDisplayStyle.Vertical)
        {
            MsuPanel.Visibility = Visibility.Visible;
            var creator = string.IsNullOrEmpty(_currentTrack.MsuCreator)
                ? _currentMsu.DisplayCreator
                : _currentTrack.MsuCreator;
            var msu = string.IsNullOrEmpty(_currentTrack.MsuName)
                ? _currentMsu.DisplayName
                : _currentTrack.MsuName;
            MsuTextBlock.Text = string.IsNullOrEmpty(creator)
                ? msu
                : $"{msu} by {creator}";

            AlbumPanel.Visibility = string.IsNullOrEmpty(_currentTrack.DisplayAlbum)
                ? Visibility.Collapsed
                : Visibility.Visible;
            AlbumTextBlock.Text = _currentTrack.DisplayAlbum ?? "";

            ArtistPanel.Visibility = string.IsNullOrEmpty(_currentTrack.DisplayArtist)
                ? Visibility.Collapsed
                : Visibility.Visible;
            ArtistTextBlock.Text = _currentTrack.DisplayArtist ?? "";

            SongPanel.Visibility = Visibility.Visible;
            SongTextBlock.Text = _currentTrack.SongName;

            HorizontalTextBlock.Visibility = Visibility.Collapsed;
            InfoTextBlock.Visibility = Visibility.Collapsed;
        }
        else
        {
            MsuPanel.Visibility = Visibility.Collapsed;
            AlbumPanel.Visibility = Visibility.Collapsed;
            ArtistPanel.Visibility = Visibility.Collapsed;
            SongPanel.Visibility = Visibility.Collapsed;
            HorizontalTextBlock.Visibility = Visibility.Visible;
            InfoTextBlock.Visibility = Visibility.Collapsed;
            HorizontalTextBlock.Text = _outputText;
        }

        _cts.Cancel();
        MarqueeReset();
        _ = StartMarquee();
    }

    private void MarqueeStart()
    {

        var outerWidth = outerCanvas.ActualWidth;
        var innerWidth = innerPanel.ActualWidth;
        if (innerWidth < outerWidth)
            return;
        _marquee.From = 0;
        _marquee.To = outerWidth - innerWidth;
        _marquee.Duration = new Duration(TimeSpan.FromSeconds((innerWidth - outerWidth) / 50));
        innerPanel.BeginAnimation(Canvas.LeftProperty, _marquee);
    }

    private void MarqueeReset()
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(MarqueeReset);
            return;
        }
        innerPanel.BeginAnimation(Canvas.LeftProperty, null);
    }

    private void MarqueeOnCompleted(object? sender, EventArgs e)
    {
        _ = RestartMarquee();
    }

    private async Task StartMarquee()
    {
        _cts = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromSeconds(3), _cts.Token);
        if (_cts.IsCancellationRequested) return;
        MarqueeStart();
    }

    private async Task RestartMarquee()
    {
        if (_cts.IsCancellationRequested) return;
        await Task.Delay(TimeSpan.FromSeconds(3), _cts.Token);
        if (_cts.IsCancellationRequested) return;
        MarqueeReset();
        _ = StartMarquee();
    }

    private void MsuTrackWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        App.SaveWindowPositionAndSize(this);
        if (!_shuttingDown && _options != null)
        {
            _options.GeneralOptions.DisplayMsuTrackWindow = false;
            _options.Save();
        }
    }

    public void Close(bool isShuttingDown)
    {
        _shuttingDown = isShuttingDown;
        Close();
    }

    public void Dispose()
    {
        _cts.Dispose();
        if (_tracker != null)
        {
            _tracker.TrackChanged -= TrackerOnTrackChanged;
        }
    }
}

