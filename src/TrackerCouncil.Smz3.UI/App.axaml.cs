using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AppImageManager;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaControls.Controls;
using Microsoft.Extensions.DependencyInjection;
using SharpHook;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI;

public partial class App : Application
{
    public const string AppId = "org.trackercouncil.smz3";
    public const string AppName = "SMZ3 Cas' Randomizer";

    private IGlobalHook? _hook;
    private Task? _hookRunner;
    private static readonly string? s_versionOverride = null;

    public static string Version
    {
        get
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location);
            return s_versionOverride ?? (version.ProductVersion ?? "").Split("+")[0];
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && Program.MainHost != null)
        {
            if (OperatingSystem.IsWindows())
            {
                _hook = Program.MainHost.Services.GetRequiredService<IGlobalHook>();
                _hookRunner = _hook.RunAsync();
            }

            var mainWindow = Program.MainHost.Services.GetService<MainWindow>();
            MessageWindow.GlobalParentWindow = mainWindow;
            desktop.MainWindow = mainWindow;
            desktop.Exit += DesktopOnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DesktopOnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_hook != null)
        {
            _hook.Dispose();
            _hookRunner?.GetAwaiter().GetResult();
        }
    }

    private void NativeMenuItem_OnClick(object? sender, EventArgs e)
    {
        Program.MainHost?.Services.GetService<AboutWindow>()?.Show();
    }

    [SupportedOSPlatform("linux")]
    internal static CreateDesktopFileResponse BuildLinuxDesktopFile()
    {
        return new DesktopFileBuilder(AppId, AppName)
            .AddUninstallAction(Directories.AppDataFolder)
            .Build();
    }
}
