using System;
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

using Randomizer.App.ViewModels;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Generation;

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

        public RomListWindow(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            ILogger<RomListWindow> logger,
            RandomizerContext dbContext,
            RomGenerator romGenerator)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = dbContext;
            _romGenerator = romGenerator;
            InitializeComponent();
            CheckSpeechRecognition();
            Options = optionsFactory.Create();

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
        private void QuickGenerate_Click(object sender, RoutedEventArgs e)
        {
            var successful = _romGenerator.GenerateRom(Options, out var romPath, out var error);

            if (successful)
            {
                UpdateRomList();
            }
        }

        /// <summary>
        /// Opens the page to generate a custom rom with new settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            var generateWindow = new GenerateRomWindow(_serviceProvider);
            generateWindow.Owner = this;
            generateWindow.Options = Options;
            var successful = generateWindow.ShowDialog();

            if (successful.HasValue && successful.Value)
            {
                UpdateRomList();
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

        private void UpdateRomList()
        {
            var models = _dbContext.GeneratedRoms
                    .Include(x => x.TrackerState)
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

        /// <summary>
        /// Launches the tracker window with no rom selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartTracker_Click(object sender, RoutedEventArgs e)
        {
            LaunchTracker(null);
        }

        /// <summary>
        /// Launches the about menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var optionsDialog = new AboutWindow();
            optionsDialog.ShowDialog();
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

                _trackerWindow.SavedState += _trackerWindow_SavedState;

                _trackerWindow.Show();
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
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
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

            Clipboard.SetText(rom.Seed);
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
    }
}
