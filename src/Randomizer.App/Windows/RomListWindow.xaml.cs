using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using Randomizer.App.ViewModels;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;

using SharpYaml;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for RomListWindow.xaml
    /// </summary>
    public partial class RomListWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RomListWindow> _logger;
        private readonly RandomizerContext _dbContext;
        private readonly RomGenerator _romGenerator;
        private TrackerWindow _trackerWindow;
        private readonly IHistoryService _historyService;

        public RomListWindow(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            ILogger<RomListWindow> logger,
            RandomizerContext dbContext,
            RomGenerator romGenerator,
            IHistoryService historyService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = dbContext;
            _romGenerator = romGenerator;
            InitializeComponent();
            CheckSpeechRecognition();
            Options = optionsFactory.Create();
            _historyService = historyService;

            Model = new GeneratedRomsViewModel();
            DataContext = Model;
            UpdateRomList();
        }

        public GeneratedRomsViewModel Model { get; }
        public RandomizerOptions Options { get; }

        protected bool CanStartTracker { get; set; }

        /// <summary>
        /// Verifies that speech recognition is working
        /// </summary>
        private void CheckSpeechRecognition()
        {
            try
            {
                var installedRecognizers = System.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers();
                _logger.LogInformation("{count} installed recognizer(s): {recognizers}",
                    installedRecognizers.Count, string.Join(", ", installedRecognizers.Select(x => x.Description)));
                if (installedRecognizers.Count == 0)
                {
                    CanStartTracker = false;
                }
                else
                {
                    CanStartTracker = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while checking speech recognition capabilities.");
                CanStartTracker = false;
            }
        }

        /// <summary>
        /// Generates a rom based on te previous settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var successful = _romGenerator.GenerateRandomRom(Options, out _, out var error, out var rom);

            if (!successful)
            {
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(this, error, "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                UpdateRomList();
                QuickLaunchRom(rom);
            }
        }

        /// <summary>
        /// Opens the page to generate a custom rom with new settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();

            var generateWindow = scope.ServiceProvider.GetRequiredService<GenerateRomWindow>();
            generateWindow.Owner = this;
            generateWindow.Options = Options;
            var successful = generateWindow.ShowDialog();

            if (successful.HasValue && successful.Value)
            {
                UpdateRomList();
            }

            RomsList.SelectedIndex = 0;
        }

        private async void StartPlandoButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();

            var plandoBrowser = new OpenFileDialog
            {
                Title = "Open plando configuration - SMZ3 Cas’ Randomizer",
                Filter = "YAML files (*.yml; *.yaml)|*.yml;*.yaml|All files (*.*)|*.*"
            };
            if (plandoBrowser.ShowDialog(this) != true)
                return;

            try
            {
                var plandoConfigLoader = scope.ServiceProvider.GetRequiredService<IPlandoConfigLoader>();
                var plandoConfig = await plandoConfigLoader.LoadAsync(plandoBrowser.FileName);

                var generateWindow = scope.ServiceProvider.GetRequiredService<GenerateRomWindow>();
                generateWindow.PlandoConfig = plandoConfig;
                generateWindow.Owner = this;
                generateWindow.Options = Options;
                if (generateWindow.ShowDialog() == true)
                {
                    UpdateRomList();
                }

                RomsList.SelectedIndex = 0;
            }
            catch (PlandoConfigurationException ex)
            {
                _logger.LogWarning(ex, "Plando config '{FileName}' contains errors.", plandoBrowser.FileName);
                MessageBox.Show(this, "The selected plando configuration contains errors:\n\n" + ex.Message,
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (YamlException ex)
            {
                _logger.LogWarning(ex, "Plando config '{FileName}' is malformed.", plandoBrowser.FileName);
                MessageBox.Show(this, $"The selected plando configuration contains errors in line {ex.Start.Line}, col {ex.Start.Column}:\n\n{ex.InnerException?.Message ?? ex.Message}",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unknown exception occurred while trying to generate a plando.");
                MessageBox.Show(this, "Oops. Something went wrong. Please try again.",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void UpdateRomList()
        {
            var models = _dbContext.GeneratedRoms
                    .Include(x => x.TrackerState)
                    .Include(x => x.TrackerState.History)
                    .OrderByDescending(x => x.Id)
                    .ToList();
            Model.UpdateList(models);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Options.Save(OptionsFactory.GetFilePath());
            }
            catch
            {
                // Oh well
            }
        }

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var optionsDialog = scope.ServiceProvider.GetRequiredService<OptionsWindow>();
            optionsDialog.Options = Options.GeneralOptions;
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

        /// <summary>
        /// Launches the about menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var aboutWindow = scope.ServiceProvider.GetRequiredService<AboutWindow>();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        /// <summary>
        /// The user has clicked on a quick launch button for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button launchButton)
                return;

            if (launchButton.Tag is not GeneratedRom rom)
                return;

            QuickLaunchRom(rom);
        }

        /// <summary>
        /// Launches a rom with the set quick launch options
        /// </summary>
        /// <param name="rom">The rom to launch</param>
        private void QuickLaunchRom(GeneratedRom rom)
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

        /// <summary>
        /// Launches the tracker window for the given rom
        /// </summary>
        /// <param name="rom">The rom to open tracker for</param>
        private void LaunchTracker(GeneratedRom rom)
        {
            if (!CanStartTracker)
            {
                MessageBox.Show(this, $"No speech recognition capabilities detected. Please check Windows settings under Time & Language > Speech.",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_trackerWindow != null && _trackerWindow.IsVisible)
            {
                MessageBox.Show(this, $"An instance of tracker is already open.",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var scope = _serviceProvider.CreateScope();
                var trackerOptionsAccessor = scope.ServiceProvider.GetRequiredService<TrackerOptionsAccessor>();
                trackerOptionsAccessor.Options = Options.GeneralOptions.GetTrackerOptions();

                _trackerWindow = scope.ServiceProvider.GetRequiredService<TrackerWindow>();
                _trackerWindow.Options = Options;
                _trackerWindow.Closed += (_, _) => scope.Dispose();

                if (rom != null)
                {
                    _trackerWindow.Rom = rom;
                }

                _trackerWindow.SavedState += _trackerWindow_SavedState;
                _trackerWindow.Show();
            }
            catch (YamlDotNet.Core.SemanticErrorException ex)
            {
                MessageBox.Show(this, ex.Message + "\n\n" + ex.InnerException.Message,
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unhandled exception occurred when starting the tracker.");
            }
        }

        /// <summary>
        /// Updates the rom list upon tracker's state being saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _trackerWindow_SavedState(object sender, EventArgs e)
        {
            UpdateRomList();
        }

        /// <summary>
        /// Launches the current rom in the default program
        /// </summary>
        /// <param name="rom">The rom to execute</param>
        private void LaunchRom(GeneratedRom rom)
        {
            var path = Path.Combine(Options.RomOutputPath, rom.RomPath);
            if (File.Exists(path))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
                catch (Win32Exception e)
                {
                    _logger.LogError(e, "Could not open rom file");
                    MessageBox.Show(this, $"Could not open rom file.\nVerify you have a default application for sfc files.",
                        "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(this, $"Could not find rom file at {path}",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Opens the folder containing the rom
        /// </summary>
        /// <param name="rom">The rom to open the folder for</param>
        private void OpenFolder(GeneratedRom rom)
        {
            var path = Path.Combine(Options.RomOutputPath, rom.RomPath);
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else
            {
                MessageBox.Show(this, $"Could not find rom file at {path}",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Right click menu to play a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not GeneratedRom rom)
                return;

            LaunchRom(rom);
        }

        /// <summary>
        /// Right click menu to open the folder for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not GeneratedRom rom)
                return;

            OpenFolder(rom);
        }

        /// <summary>
        /// Menu item for opening the tracker for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenTrackerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not GeneratedRom rom)
                return;

            LaunchTracker(rom);
        }

        /// <summary>
        /// Menu item for viewing the spoiler log for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewSpoilerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not GeneratedRom rom)
                return;

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
                MessageBox.Show(this, $"Could not find spoiler file at {path}",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Menu item for editing the label for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditLabelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Parent is not ContextMenu contextMenu)
                return;

            if (contextMenu.PlacementTarget is not Grid grid)
                return;

            ShowEditTextBox(grid);
        }

        /// <summary>
        /// Toggles the edit textbox and hides the textblock for a rom for editing the label
        /// </summary>
        /// <param name="grid">The grid that houses both the textbox and textblock for the rom</param>
        private void ShowEditTextBox(Grid grid)
        {
            if (grid.FindName("EditLabelTextBox") is not TextBox editLabelTextBox)
                return;

            if (grid.FindName("LabelTextBlock") is not TextBlock labelTextBlock)
                return;

            labelTextBlock.Visibility = Visibility.Collapsed;
            editLabelTextBox.Visibility = Visibility.Visible;
            editLabelTextBox.Focus();
        }

        /// <summary>
        /// Updates the name for a rom and toggles the visibility for the textbox and textblock
        /// </summary>
        /// <param name="grid">The grid that houses both the textbox and textblock for the rom</param>
        private void UpdateName(Grid grid)
        {
            if (grid.FindName("EditLabelTextBox") is not TextBox editLabelTextBox)
                return;

            if (grid.FindName("LabelTextBlock") is not TextBlock labelTextBlock)
                return;

            if (editLabelTextBox.Tag is not GeneratedRom rom)
                return;

            labelTextBlock.Visibility = Visibility.Visible;
            editLabelTextBox.Visibility = Visibility.Collapsed;

            var newName = editLabelTextBox.Text;
            if (rom.Label != newName)
            {
                rom.Label = newName;
                _dbContext.SaveChanges();
                UpdateRomList();
            }

        }

        /// <summary>
        /// Menu item for copying the seed for a rom to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopySeedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not GeneratedRom rom)
                return;

            CopyTextToClipboard(rom.Seed);
        }

        /// <summary>
        /// Menu item for copying the seed's config string for sending to someone else
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not GeneratedRom rom)
                return;

            CopyTextToClipboard(rom.Settings);
        }

        /// <summary>
        /// Menu item for deleting a rom from the db and filesystem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void DeleteRomMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not GeneratedRom rom)
                return;

            if (MessageBox.Show("Are you sure you want to delete this rom and tracker information? This cannot be undone.",
                "SMZ3 Cas’ Randomizer", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            // Delete the tracker info if it is available
            if (rom.TrackerState != null)
            {
                _dbContext.Entry(rom.TrackerState).Collection(x => x.ItemStates).Load();
                _dbContext.Entry(rom.TrackerState).Collection(x => x.LocationStates).Load();
                _dbContext.Entry(rom.TrackerState).Collection(x => x.RegionStates).Load();
                _dbContext.Entry(rom.TrackerState).Collection(x => x.DungeonStates).Load();
                _dbContext.Entry(rom.TrackerState).Collection(x => x.MarkedLocations).Load();
                _dbContext.Entry(rom.TrackerState).Collection(x => x.BossStates).Load();
                _dbContext.Entry(rom.TrackerState).Collection(x => x.History).Load();

                _dbContext.TrackerStates.Remove(rom.TrackerState);
            }

            // Remove the rom itself from the db and save the db
            _dbContext.GeneratedRoms.Remove(rom);
            _dbContext.SaveChanges();

            // Try to delete the folder
            try
            {
                var path = Path.GetDirectoryName(Path.Combine(Options.RomOutputPath, rom.RomPath));
                var directory = new DirectoryInfo(path);
                directory.Delete(true);
            }
            catch (Exception ex)
            {
                if (ex is not DirectoryNotFoundException)
                {
                    MessageBox.Show(this, "There was an error in trying to delete the rom directory.",
                        "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            UpdateRomList();
        }

        /// <summary>
        /// Updates the label for a rom when clicking away from the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditLabelTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            if (textBox.Parent is not Grid grid)
                return;

            UpdateName(grid);
        }

        /// <summary>
        /// Updates the label for a rom when pressing enter/return
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditLabelTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            if (sender is not TextBox textBox)
                return;

            if (textBox.Parent is not Grid grid)
                return;

            UpdateName(grid);
        }

        private void CopyTextToClipboard(string text)
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

        private void ProgressionLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not GeneratedRom rom)
                return;

            if (rom.TrackerState == null)
            {
                MessageBox.Show(this, "There is no history for the selected rom.",
                        "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _dbContext.Entry(rom.TrackerState).Collection(x => x.History).Load();

            if (rom.TrackerState.History == null || rom.TrackerState.History.Count == 0)
            {
                MessageBox.Show(this, "There is no history for the selected rom.",
                        "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var path = Path.Combine(Options.RomOutputPath, rom.SpoilerPath).Replace("Spoiler_Log", "Progression_Log");
            var historyText = _historyService.GenerateHistoryText(rom, rom.TrackerState.History.ToList(), true);
            File.WriteAllText(path, historyText);
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    }
}
