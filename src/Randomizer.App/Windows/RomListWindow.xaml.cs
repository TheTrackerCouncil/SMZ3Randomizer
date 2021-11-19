using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Randomizer.App.ViewModels;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Generation;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RomListWindow : Window
    {
        private readonly Task _loadSpritesTask;
        private readonly IServiceProvider _serviceProvider;
        private readonly Smz3Randomizer _randomizer;
        private readonly ILogger<RomListWindow> _logger;
        private readonly RandomizerContext _dbContext;
        private readonly RomGenerator _romGenerator;
        private TrackerWindow _trackerWindow;

        public RomListWindow(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            Smz3Randomizer randomizer,
            ILogger<RomListWindow> logger,
            RandomizerContext dbContext,
            RomGenerator romGenerator)
        {
            _serviceProvider = serviceProvider;
            _randomizer = randomizer;
            _logger = logger;
            _dbContext = dbContext;
            _romGenerator = romGenerator;
            InitializeComponent();
            CheckSpeechRecognition();
            Options = optionsFactory.Create();

            var models = dbContext.GeneratedRoms
                .Include(x => x.TrackerState)
                .OrderByDescending(x => x.Id)
                .ToList();

            Model = new GeneratedRomsViewModel(models);
            DataContext = Model;
        }

        public GeneratedRomsViewModel Model { get; }
        public RandomizerOptions Options { get; }

        private void CheckSpeechRecognition()
        {
            try
            {
                var installedRecognizers = System.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers();
                _logger.LogInformation("{count} installed recognizer(s): {recognizers}",
                    installedRecognizers.Count, string.Join(", ", installedRecognizers.Select(x => x.Description)));
                if (installedRecognizers.Count == 0)
                {
                    StartTracker.IsEnabled = false;
                    StartTracker.ToolTip = "No speech recognition capabilities detected. Please check Windows settings under Time & Language > Speech.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while checking speech recognition capabilities.");
                StartTracker.IsEnabled = false;
                StartTracker.ToolTip = "An error occurred while checking speech recognition capabilities. Please check the randomizer log file and ensure the Windows settings under Time & Language > Speech are correct.";
            }
        }

        private void QuickGenerate_Click(object sender, RoutedEventArgs e)
        {
            var successful = _romGenerator.GenerateRom(Options, out var romPath, out var error);

            if (successful)
            {
                var models = _dbContext.GeneratedRoms
                    .Include(x => x.TrackerState)
                    .OrderByDescending(x => x.Id)
                    .ToList();
                Model.UpdateList(models);
            }
        }

        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            //_romGenerator.GenerateRom(Options, out var romPath, out var error);
            var generateWindow = new GenerateRomWindow(_serviceProvider);
            generateWindow.Owner = this;
            generateWindow.Options = Options;
            var successful = generateWindow.ShowDialog();

            if (successful.HasValue && successful.Value)
            {
                var models = _dbContext.GeneratedRoms
                    .Include(x => x.TrackerState)
                    .OrderByDescending(x => x.Id)
                    .ToList();
                Model.UpdateList(models);
            }

            RomsList.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Options.GeneralOptions.Validate())
            {
                MessageBox.Show(this, "If this is your first time using the randomizer," +
                    " there are some required options you need to configure before you " +
                    "can start playing randomized SMZ3 games. Please do so now.",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Information);
                OptionsMenuItem_Click(this, new RoutedEventArgs());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var optionsDialog = new OptionsWindow(Options.GeneralOptions);
            optionsDialog.ShowDialog();

            try
            {
                Options.Save(OptionsFactory.GetFilePath());
            }
            catch
            {
                // Oh well
            }

        }

        private void StartTracker_Click(object sender, RoutedEventArgs e)
        {
            LaunchTracker(null);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = _serviceProvider.GetRequiredService<AboutWindow>();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button launchButton)
                return;

            if (launchButton.Tag is not GeneratedRom rom)
                return;

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

        private void LaunchTracker(GeneratedRom rom)
        {
            try
            {
                var scope = _serviceProvider.CreateScope();
                _trackerWindow = scope.ServiceProvider.GetRequiredService<TrackerWindow>();
                _trackerWindow.Options = Options;
                _trackerWindow.Closed += (_, _) => scope.Dispose();

                if (rom != null)
                {
                    _trackerWindow.Rom = rom;
                }

                _trackerWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unhandled exception occurred when starting the tracker.");
            }
        }

        private void LaunchRom(GeneratedRom rom)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = rom.RomPath,
                UseShellExecute = true
            });
        }

        private void OpenFolder(GeneratedRom rom)
        {
            Process.Start("explorer.exe", $"/select,\"{rom.RomPath}\"");
        }
    }
}
