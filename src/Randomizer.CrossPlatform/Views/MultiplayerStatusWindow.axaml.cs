using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.Shared.Models;

namespace Randomizer.CrossPlatform.Views;

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
    protected override int DefaultWidth => 800;
    protected override int DefualtHeight => 600;

    private void CopyUrlButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void UpdateConfigButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void ForfeitButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void ReconnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void StartGameButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OpenTrackerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void LaunchOptions_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void PlayMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OpenFolderMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OpenTrackerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void ViewSpoilerLogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}

