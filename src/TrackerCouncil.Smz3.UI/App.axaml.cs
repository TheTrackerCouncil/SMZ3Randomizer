using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaControls.Controls;
using Microsoft.Extensions.DependencyInjection;
using SharpHook;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI;

public partial class App : Application
{
    private IGlobalHook? _hook;
    private Task? _hookRunner;

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
}
