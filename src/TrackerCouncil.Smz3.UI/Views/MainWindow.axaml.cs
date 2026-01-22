using System;
using System.IO;
using System.Threading.Tasks;
using AppImageManager;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using Microsoft.Extensions.DependencyInjection;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class MainWindow : RestorableWindow
{
    private readonly MainWindowService? _service;
    private readonly MainWindowViewModel _model;
    private readonly IServiceProvider? _serviceProvider;
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

        try
        {
            InitializeComponent();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        DataContext = _model = _service.InitializeModel(this);

        _service.SpriteDownloadStart += (_, _) =>
        {
            _spriteDownloadWindow = new SpriteDownloadWindow();
            _spriteDownloadWindow.Closed += (_, _) =>
            {
                _spriteDownloadWindow = null;
            };
            _spriteDownloadWindow.ShowDialog(this);
        };

        _service.SpriteDownloadEnd += (_, _) => _spriteDownloadWindow?.Close();
    }

    public void Reload()
    {
        SoloRomListPanel.Reload();
        MultiRomListPanel.Reload();
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "main-window.json");
    protected override int DefaultWidth => 800;
    protected override int DefaultHeight => 600;

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
        if (_service == null)
        {
            return;
        }

        _ = ITaskService.Run(_service.ValidateTwitchToken);
        _ = ITaskService.Run(_service.DownloadConfigsAsync);
        _ = ITaskService.Run(_service.DownloadSpritesAsync);

        if (_model.OpenSetupWindow || _model.OpenDesktopFileWindow)
        {
            _ = Dispatcher.UIThread.InvokeAsync(OpenStartingWindows);
        }
    }

    private async Task OpenOptionsWindow()
    {
        using var scope = _serviceProvider?.CreateScope();
        if (scope == null)
        {
            return;
        }

        var window = scope.ServiceProvider.GetRequiredService<OptionsWindow>();
        await window.ShowDialog(this);
    }

    private async Task OpenStartingWindows()
    {
        await Task.Delay(TimeSpan.FromSeconds(0.5));
        if (_model.OpenSetupWindow && _serviceProvider != null)
        {
            var result = await _serviceProvider.GetRequiredService<SetupWindow>()
                .ShowDialog<SetupWindowCloseBehavior>(this, SetupWindowStep.Roms);
            if (result == SetupWindowCloseBehavior.OpenSettingsWindow)
            {
                _ = OpenOptionsWindow();
            }
            else if (result == SetupWindowCloseBehavior.OpenGenerationWindow)
            {
                _ = SoloRomListPanel.OpenGenerationWindow();
            }
        }
        else if (_model.OpenDesktopFileWindow)
        {
            _model.OpenDesktopFileWindow = false;
            var response = await MessageWindow.ShowYesNoDialog(
                "Would you like to add SMZ3 to your menu by creating a desktop file?",
                "SMZ3 Cas' Randomizer", this);
            _service!.HandleUserDesktopResponse(response);
        }
    }

    private void OptionsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = OpenOptionsWindow();
    }

    private void AboutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _serviceProvider?.GetRequiredService<AboutWindow>().ShowDialog(this);
    }

    private async void DownloadReleaseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_model.NewVersionDownloadUrl) || _service == null)
            {
                return;
            }

            if (OperatingSystem.IsLinux())
            {
                var downloadResult = await AppImage.DownloadAsync(new DownloadAppImageRequest
                {
                    Url = _model.NewVersionDownloadUrl
                });

                if (downloadResult.Success)
                {
                    Close();
                }
                else if (downloadResult.DownloadedSuccessfully)
                {
                    await MessageWindow.ShowErrorDialog("AppImage was downloaded, but it could not be launched.");
                }
                else
                {
                    await MessageWindow.ShowErrorDialog("Failed downloading AppImage");
                }
            }
            else
            {
                var result = await _service.InstallWindowsUpdate(_model.NewVersionDownloadUrl);
                if (!string.IsNullOrEmpty(result))
                {
                    await MessageWindow.ShowErrorDialog(result);
                }
                else
                {
                    Close();
                }
            }
        }
        catch
        {
            await MessageWindow.ShowErrorDialog("Failed downloading update");
        }
    }
}
