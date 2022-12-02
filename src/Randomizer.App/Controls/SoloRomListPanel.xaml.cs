using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.App.ViewModels;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Generation;
using SharpYaml;

namespace Randomizer.App.Controls
{
    /// <summary>
    /// Interaction logic for SoloRomListPanel.xaml
    /// </summary>
    public partial class SoloRomListPanel : RomListPanel
    {

        public SoloRomListPanel(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            ILogger<SoloRomListPanel> logger,
            RandomizerContext dbContext,
            RomGenerator romGenerator) : base (serviceProvider, optionsFactory, logger, dbContext, romGenerator)
        {
            Model = new GeneratedRomsViewModel();
            DataContext = Model;
            InitializeComponent();
            UpdateList();
        }

        public GeneratedRomsViewModel Model { get; }

        public sealed override void UpdateList()
        {
            var models = DbContext.GeneratedRoms
                    .Include(x => x.MultiplayerGameDetails)
                    .Include(x => x.TrackerState)
                    .ThenInclude(x => x!.History)
                    .Where(x => x.MultiplayerGameDetails == null)
                    .OrderByDescending(x => x.Id)
                    .ToList();
            Model.UpdateList(models);
            RomsList.SelectedIndex = 0;
        }

        /// <summary>
        /// Opens the page to generate a custom rom with new settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShowGenerateRomWindow(null, false)) UpdateList();
        }

        private async void StartPlandoButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = ServiceProvider.CreateScope();
            var window = Window.GetWindow(this);

            var plandoBrowser = new OpenFileDialog
            {
                Title = "Open plando configuration - SMZ3 Cas’ Randomizer",
                Filter = "YAML files (*.yml; *.yaml)|*.yml;*.yaml|All files (*.*)|*.*"
            };
            if (plandoBrowser.ShowDialog(window) != true)
                return;

            try
            {
                var plandoConfigLoader = scope.ServiceProvider.GetRequiredService<IPlandoConfigLoader>();
                var plandoConfig = await plandoConfigLoader.LoadAsync(plandoBrowser.FileName);
                if (ShowGenerateRomWindow(plandoConfig, false)) UpdateList();
            }
            catch (PlandoConfigurationException ex)
            {
                Logger.LogWarning(ex, "Plando config '{FileName}' contains errors.", plandoBrowser.FileName);
                ShowWarningMessageBox("The selected plando configuration contains errors:\n\n" + ex.Message);
            }
            catch (YamlException ex)
            {
                Logger.LogWarning(ex, "Plando config '{FileName}' is malformed.", plandoBrowser.FileName);
                ShowWarningMessageBox($"The selected plando configuration contains errors in line {ex.Start.Line}, col {ex.Start.Column}:\n\n{ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "An unknown exception occurred while trying to generate a plando.");
                ShowErrorMessageBox("Oops. Something went wrong. Please try again.");
            }
        }

        /// <summary>
        /// The user has clicked on a quick launch button for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: GeneratedRom rom })
                return;

            QuickLaunchRom(rom);
        }

        /// <summary>
        /// Right click menu to play a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: GeneratedRom rom })
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
            if (sender is not MenuItem { Tag: GeneratedRom rom })
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
            if (sender is not MenuItem { Tag: GeneratedRom rom })
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
            if (sender is not MenuItem { Tag: GeneratedRom rom })
                return;

            OpenSpoilerLog(rom);
        }

        /// <summary>
        /// Menu item for editing the label for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditLabelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Parent: ContextMenu { PlacementTarget: Grid grid } })
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
                DbContext.SaveChanges();
                UpdateList();
            }

        }

        /// <summary>
        /// Menu item for copying the seed for a rom to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopySeedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: GeneratedRom rom })
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
            if (sender is not MenuItem { Tag: GeneratedRom rom })
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
            if (sender is not MenuItem { Tag: GeneratedRom rom })
                return;

            DeleteGeneratedRom(rom);
            UpdateList();
        }

        /// <summary>
        /// Updates the label for a rom when clicking away from the text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditLabelTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox { Parent: Grid grid })
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

            if (sender is not TextBox { Parent: Grid grid })
                return;

            UpdateName(grid);
        }

        private void ProgressionLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: GeneratedRom rom })
                return;

            OpenProgressionLog(rom);
        }

        /// <summary>
        /// Generates a rom based on te previous settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void QuickPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var rom = await RomGenerator.GenerateRandomRomAsync(Options);
            if (!GeneratedRom.IsValid(rom)) return;
            UpdateList();
            QuickLaunchRom(rom);
        }
    }
}
