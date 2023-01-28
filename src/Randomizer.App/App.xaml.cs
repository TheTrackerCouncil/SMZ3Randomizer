using System;
using System.Windows;
using System.Windows.Controls;

using BunLabs.IO;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Randomizer.App.Controls;
using Randomizer.App.Windows;
using Randomizer.Data.Configuration;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Multiplayer.Client;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.AutoTracking;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using Randomizer.SMZ3.Twitch;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private const string BaseRegistryKey = "Software\\SMZ3 Cas Randomizer";
        private const string WindowPositionKey = "Windows";

        private IHost? _host;
        private ILogger<App>? _logger;

        public static void SaveWindowPositionAndSize<TWindow>(TWindow window)
            where TWindow : Window
        {
            using var baseKey = Registry.CurrentUser.CreateSubKey(BaseRegistryKey);
            using var windowKey = baseKey.CreateSubKey(WindowPositionKey);
            using var key = windowKey.CreateSubKey(typeof(TWindow).Name);
            key.SetValue("Width", window.Width, RegistryValueKind.DWord);
            key.SetValue("Height", window.Height, RegistryValueKind.DWord);
            key.SetValue("Left", window.Left, RegistryValueKind.DWord);
            key.SetValue("Top", window.Top, RegistryValueKind.DWord);
        }

        public static void RestoreWindowPositionAndSize<TWindow>(TWindow window)
            where TWindow : Window
        {
            try
            {
                using var baseKey = Registry.CurrentUser.OpenSubKey(BaseRegistryKey);
                if (baseKey == null)
                    return;

                using var windowKey = baseKey.OpenSubKey(WindowPositionKey);
                if (windowKey == null)
                    return;

                using var key = windowKey.OpenSubKey(typeof(TWindow).Name);
                if (key == null)
                    return;

                var vScreenWidth = SystemParameters.VirtualScreenWidth;
                var vScreenHeight = SystemParameters.VirtualScreenHeight;
                var vScreenTop = SystemParameters.VirtualScreenTop;
                var vScreenLeft = SystemParameters.VirtualScreenLeft;
                
                window.Width = (int)key.GetValue("Width", window.Width)!;
                window.Height = (int)key.GetValue("Height", window.Height)!;
                window.Left = (int)key.GetValue("Left", window.Left)!;
                window.Top = (int)key.GetValue("Top", window.Top)!;

                if (window.Left < vScreenLeft)
                {
                    window.Left = vScreenLeft;
                }
                else if (window.Left > vScreenLeft + vScreenWidth - window.Width)
                {
                    window.Left = vScreenLeft + vScreenWidth - window.Width;
                }

                if (window.Top < vScreenTop)
                {
                    window.Top = vScreenTop;
                }
                else if (window.Top > vScreenTop + vScreenHeight - window.Height)
                {
                    window.Top = vScreenTop + vScreenHeight - window.Height;
                }
            }
            catch (Exception)
            {
                // ¯\_(ツ)_/¯
            }
        }

        protected static void ConfigureServices(IServiceCollection services)
        {
            // Randomizer + Tracker
            services.AddConfigs();
            services.AddSmz3Randomizer();
            services.AddPlandomizer();
            services.AddGeneratedRomLoader();
            services.AddSmz3MultiplayerRomGenerator();
            services.AddTracker()
                .AddOptionalModule<PegWorldModeModule>()
                .AddOptionalModule<SpoilerModule>()
                .AddOptionalModule<AutoTrackerModule>()
                .AddOptionalModule<MapModule>()
                .AddOptionalModule<GameService>();

            services.AddSingleton<MsuGeneratorService>();
            services.AddSingleton<RomGenerator>();
            services.AddScoped<TrackerLocationSyncer>();
            services.AddScoped<AutoTracker>();
            services.AddSingleton<ITrackerStateService, TrackerStateService>();
            services.AddMultiplayerServices();

            // Chat
            services.AddSingleton<IChatApi, TwitchChatAPI>();
            services.AddScoped<IChatClient, TwitchChatClient>();
            services.AddSingleton<IChatAuthenticationService, TwitchAuthenticationService>();

            // WPF
            services.AddSingleton<OptionsFactory>();
            services.AddSingleton<RomListWindow>();
            services.AddTransient<SoloRomListPanel>();
            services.AddTransient<MultiRomListPanel>();
            services.AddWindows<App>();

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureLogging(logging =>
                {
                    logging.AddFile($"%LocalAppData%\\SMZ3CasRandomizer\\smz3-cas-{DateTime.UtcNow:yyyyMMdd}.log", options =>
                    {
                        options.Append = true;
                        options.FileSizeLimitBytes = FileSize.MB(20);
                        options.MaxRollingFiles = 5;
                    });
                })
                .ConfigureServices((context, services) => ConfigureServices(services))
                .Start();

            _logger = _host.Services.GetRequiredService<ILogger<App>>();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

            var mainWindow = _host.Services.GetRequiredService<RomListWindow>();
            mainWindow.Show();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                _logger?.LogCritical(ex, "[CRASH] Uncaught {exceptionType}: ", ex.GetType().Name);
            else
                _logger?.LogCritical("Unhandled exception in current domain but exception object is not an exception ({obj})", e.ExceptionObject);
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogCritical(e.Exception, "[CRASH] Uncaught {exceptionType} in Dispatcher: ", e.Exception.GetType().Name);

            var logFileLocation = Environment.ExpandEnvironmentVariables("%LocalAppData%\\SMZ3CasRandomizer");
            MessageBox.Show("An unexpected problem occurred and the SMZ3 Cas’ Randomizer needs to shut down.\n\n" +
                $"For technical details, please see the log files in '{logFileLocation}' and " +
                "post them in Discord or on GitHub at https://github.com/Vivelin/SMZ3Randomizer/issues.", "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Stop);

            e.Handled = true;
            Environment.FailFast("Uncaught exception in Dispatcher", e.Exception);
        }
    }
}
