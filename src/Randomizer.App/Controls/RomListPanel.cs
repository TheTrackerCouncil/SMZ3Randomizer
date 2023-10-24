using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.App.Windows;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Infrastructure;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.App.Controls
{
    public abstract class RomListPanel : UserControl
    {
        private TrackerWindow? _trackerWindow;
        private RomLauncherService _romLauncherService;


        public RomListPanel(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            ILogger<RomListPanel> logger,
            RomGenerationService romGenerationService,
            IGameDbService gameDbService,
            RomLauncherService romLauncherService)
        {
            ServiceProvider = serviceProvider;
            Logger = logger;
            RomGenerationService = romGenerationService;
            GameDbService = gameDbService;
            Options = optionsFactory.Create();
            _romLauncherService = romLauncherService;
            CheckSpeechRecognition();
        }

        protected IGameDbService GameDbService;

        /// <summary>
        /// Verifies that speech recognition is working
        /// </summary>
        protected void CheckSpeechRecognition()
        {
            try
            {
                var installedRecognizers = System.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers();
                Logger.LogInformation("{Count} installed recognizer(s): {Recognizers}",
                    installedRecognizers.Count, string.Join(", ", installedRecognizers.Select(x => x.Description)));
                CanStartTracker = installedRecognizers.Count != 0;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error occurred while checking speech recognition capabilities.");
                CanStartTracker = false;
            }
        }

        protected virtual bool CanStartTracker { get; private set; }

        protected virtual RomGenerationService RomGenerationService { get; private set; }

        public RandomizerOptions Options { get; private set; }

        protected readonly IServiceProvider ServiceProvider;

        protected readonly ILogger<RomListPanel> Logger;

        /// <summary>
        /// Launches a rom with the set quick launch options
        /// </summary>
        /// <param name="rom">The rom to launch</param>
        public void QuickLaunchRom(GeneratedRom rom)
        {
            var launchButtonOptions = (LaunchButtonOptions)Options.GeneralOptions.LaunchButton;

            if (launchButtonOptions is LaunchButtonOptions.PlayAndTrack or LaunchButtonOptions.OpenFolderAndTrack or LaunchButtonOptions.TrackOnly)
            {
                LaunchTracker(rom);
            }

            if (launchButtonOptions is LaunchButtonOptions.OpenFolderAndTrack or LaunchButtonOptions.OpenFolderOnly)
            {
                OpenFolder(rom);
            }

            if (launchButtonOptions is LaunchButtonOptions.PlayAndTrack or LaunchButtonOptions.PlayOnly)
            {
                LaunchRom(rom);
            }
        }

        protected void TrackerWindowOnSavedState(object? sender, EventArgs e)
        {
            UpdateList();
        }

        /// <summary>
        /// Launches the tracker window for the given rom
        /// </summary>
        /// <param name="rom">The rom to open tracker for</param>
        public void LaunchTracker(GeneratedRom rom)
        {
            if (!CanStartTracker)
            {
                ShowWarningMessageBox($"No speech recognition capabilities detected. Please check Windows settings under Time & Language > Speech.");
                return;
            }

            if (_trackerWindow is { IsVisible: true })
            {
                ShowWarningMessageBox($"An instance of tracker is already open.");
                return;
            }

            if (!GeneratedRom.IsValid(rom))
            {
                ShowWarningMessageBox($"Selected rom is invalid. Please try generating a new rom.");
                return;
            }

            try
            {
                var scope = ServiceProvider.CreateScope();
                var trackerOptionsAccessor = scope.ServiceProvider.GetRequiredService<TrackerOptionsAccessor>();
                trackerOptionsAccessor.Options = Options.GeneralOptions.GetTrackerOptions();

                _trackerWindow = scope.ServiceProvider.GetRequiredService<TrackerWindow>();
                _trackerWindow.Closed += (_, _) => scope.Dispose();
                _trackerWindow.Rom = rom;
                _trackerWindow.SavedState += TrackerWindowOnSavedState;
                _trackerWindow.Show();
            }
            catch (YamlDotNet.Core.SemanticErrorException ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException?.Message,
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "An unhandled exception occurred when starting the tracker.");
            }
        }

        /// <summary>
        /// Opens the folder containing the rom
        /// </summary>
        /// <param name="rom">The rom to open the folder for</param>
        public void OpenFolder(GeneratedRom rom)
        {
            var path = Path.Combine(Options.RomOutputPath, rom.RomPath);
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else
            {
                ShowWarningMessageBox($"Could not find rom file at {path}");
            }
        }

        /// <summary>
        /// Launches the current rom in the default program
        /// </summary>
        /// <param name="rom">The rom to execute</param>
        public void LaunchRom(GeneratedRom rom)
        {
            var path = Path.Combine(Options.RomOutputPath, rom.RomPath);
            if (File.Exists(path))
            {
                try
                {
                    _romLauncherService.LaunchRom(path, Options.GeneralOptions.LaunchApplication,
                        Options.GeneralOptions.LaunchArguments);
                }
                catch (Win32Exception e)
                {
                    Logger.LogError(e, "Could not open rom file");
                    ShowErrorMessageBox($"Could not open rom file.\nVerify you have a default application for sfc files.");
                }
            }
            else
            {
                ShowWarningMessageBox($"Could not find rom file at {path}");
            }
        }

        public void CopyTextToClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                Clipboard.SetDataObject(text);
            }
        }

        protected void OpenProgressionLog(GeneratedRom rom)
        {
            if (rom.TrackerState == null)
            {
                ShowWarningMessageBox("There is no history for the selected rom.");
                return;
            }

            var history = GameDbService.GetGameHistory(rom);

            if (rom.TrackerState?.History == null || rom.TrackerState.History.Count == 0)
            {
                ShowWarningMessageBox("There is no history for the selected rom.");
                return;
            }

            var path = Path.Combine(Options.RomOutputPath, rom.SpoilerPath).Replace("Spoiler_Log", "Progression_Log");
            var historyText = HistoryService.GenerateHistoryText(rom, history.ToList());
            File.WriteAllText(path, historyText);
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }

        protected bool DeleteGeneratedRom(GeneratedRom rom)
        {
            if (ShowWarningMessageBox("Are you sure you want to delete this rom and tracker information? This cannot be undone.") == MessageBoxResult.No)
            {
                return false;
            }

            if (!GameDbService.DeleteGeneratedRom(rom, out var error))
            {
                ShowErrorMessageBox(error);
            }
            return true;
        }

        public void OpenSpoilerLog(GeneratedRom rom)
        {
            var path = Path.Combine(Options.RomOutputPath, rom.SpoilerPath);
            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            else
            {
                ShowWarningMessageBox($"Could not find spoiler file at {path}");
            }
        }

        public void CloseTracker()
        {
            _trackerWindow?.Close();
        }

        public abstract void UpdateList();

        public bool ShowGenerateRomWindow(PlandoConfig? plandoConfig, bool isMulti)
        {
            using var scope = ServiceProvider.CreateScope();
            var generateWindow = scope.ServiceProvider.GetRequiredService<GenerateRomWindow>();
            generateWindow.Owner = Window.GetWindow(this);
            generateWindow.Options = Options;
            generateWindow.PlandoConfig = plandoConfig;
            generateWindow.MultiplayerMode = isMulti;
            var successful = generateWindow.ShowDialog();
            return successful.HasValue && successful.Value;
        }

        protected MessageBoxResult ShowWarningMessageBox(string message)
        {
            return MessageBox.Show(Window.GetWindow(this)!, message, "SMZ3 Cas’ Randomizer", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }

        protected MessageBoxResult ShowErrorMessageBox(string message)
        {
            return MessageBox.Show(Window.GetWindow(this)!, message, "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
