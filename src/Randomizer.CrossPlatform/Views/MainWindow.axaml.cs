using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.Data.Options;

namespace Randomizer.CrossPlatform.Views;

public partial class MainWindow : RestorableWindow
{
    private MainWindowService? _service;
    private MainWindowViewModel _model;
    private IServiceProvider? _serviceProvider;
    private SpriteDownloadWindow? _spriteDownloadWindow;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _model = new MainWindowViewModel();
    }

    public MainWindow(MainWindowService service, IServiceProvider? serviceProvider, OptionsFactory options)
    {
        _service = service;
        _serviceProvider = serviceProvider;
        GlobalScaleFactor = Math.Clamp(options.Create().GeneralOptions.UIScaleFactor, 1, 3);
        InitializeComponent();
        DataContext = _model = _service.InitializeModel(this);

        _service.SpriteDownloadStart += (sender, args) =>
        {
            _spriteDownloadWindow = new SpriteDownloadWindow();
            _spriteDownloadWindow.Closed += (o, eventArgs) =>
            {
                _spriteDownloadWindow = null;
            };
            _spriteDownloadWindow.ShowDialog(this);
        };

        _service.SpriteDownloadEnd += (sender, args) => _spriteDownloadWindow?.Close();
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "main-window.json");
    protected override int DefaultWidth => 800;
    protected override int DefualtHeight => 600;

    private void CloseUpdateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _model.DisplayNewVersionBanner = false;
    }

    private void GitHubUrlLink_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl(_model.NewVersionGitHubUrl);
    }

    private void IgnoreVersionLink_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.IgnoreUpdate();
    }

    private void DisableUpdatesLink_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.DisableUpdates();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_model.HasInvalidOptions)
        {
            var messageWindow = new MessageWindow(new MessageWindowRequest()
            {
                Message = "If this is your first time using the randomizer, there are some required options you need to configure before you can start playing randomized SMZ3 games. Please do so now.",
                Title = "SMZ3 Cas’ Randomizer",
                Icon = MessageWindowIcon.Info,
                Buttons = MessageWindowButtons.OK
            });
            messageWindow.ShowDialog(this);
            messageWindow.Closed += (o, args) =>
            {
                _serviceProvider?.GetRequiredService<OptionsWindow>().ShowDialog(this);
            };
        }

        if (_service == null)
        {
            return;
        }

        ITaskService.Run(_service.ValidateTwitchToken);
        ITaskService.Run(_service.DownloadConfigsAsync);
        ITaskService.Run(_service.DownloadSpritesAsync);
    }

    private void OptionsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _serviceProvider?.GetRequiredService<OptionsWindow>().ShowDialog(this);
    }

    private void AboutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _serviceProvider?.GetRequiredService<AboutWindow>().ShowDialog(this);
    }
}
