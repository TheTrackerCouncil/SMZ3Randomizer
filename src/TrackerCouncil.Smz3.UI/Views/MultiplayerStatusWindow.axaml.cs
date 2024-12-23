using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.Shared.Models;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class MultiplayerStatusWindow : RestorableWindow
{
    private MultiplayerStatusWindowViewModel _model;
    private MultiplayerStatusWindowService? _service;

    public MultiplayerStatusWindow() : this(new MultiplayerRomViewModel(new MultiplayerGameDetails()))
    {

    }

    public MultiplayerStatusWindow(MultiplayerRomViewModel rom)
    {
        if (Design.IsDesignMode)
        {
            _model = new MultiplayerStatusWindowViewModel();
        }
        else
        {
            _service = IControlServiceFactory.GetControlService<MultiplayerStatusWindowService>();
            _model = _service?.GetViewModel(this, rom) ?? new MultiplayerStatusWindowViewModel();
        }

        InitializeComponent();
        DataContext = _model;
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "multiplayer-status-window.json");
    protected override int DefaultWidth => 500;
    protected override int DefaultHeight => 250;

    private async void CopyUrlButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Clipboard == null)
        {
            return;
        }
        await Clipboard.SetTextAsync(_model.GameUrl);
    }

    private async void UpdateConfigButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
        {
            return;
        }
        await _service.SubmitConfig();
    }

    private void ForfeitButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: MultiplayerPlayerStateViewModel player })
            return;
        _service?.Forfeit(player);
    }

    private void ReconnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.Reconnect();
    }

    private void StartGameButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.StartGame();
    }

    private void LaunchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = _service?.LaunchRom();
    }

    private void LaunchOptions_OnClick(object? sender, RoutedEventArgs e)
    {
        LaunchOptions.ContextMenu?.Open();
    }

    private void PlayMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.PlayRom();
    }

    private void OpenFolderMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenFolder();
    }

    private void OpenTrackerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = _service?.LaunchTracker();
    }

    private void ViewSpoilerLogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenSpoilerLog();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _ = _service?.Connect();
    }

    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        _service?.Dispose();
    }
}

