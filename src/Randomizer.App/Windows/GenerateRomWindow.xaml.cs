using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Microsoft.Extensions.DependencyInjection;

using Randomizer.App.ViewModels;
using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for GenerateRomWindow.xaml
    /// </summary>
    public partial class GenerateRomWindow : Window
    {
        private readonly Task _loadSpritesTask;
        private readonly IServiceProvider _serviceProvider;
        private readonly RomGenerator _romGenerator;
        private readonly SMZ3.Tracking.Configuration.ConfigFiles.LocationConfig _locations;
        private RandomizerOptions _options;
        private IItemService _itemService;

        public GenerateRomWindow(IServiceProvider serviceProvider,
            RomGenerator romGenerator,
            SMZ3.Tracking.Configuration.ConfigFiles.LocationConfig locations,
            IItemService itemService)
        {
            _serviceProvider = serviceProvider;
            _romGenerator = romGenerator;
            _locations = locations;
            _itemService = itemService;
            InitializeComponent();

            SamusSprites.Add(Sprite.DefaultSamus);
            LinkSprites.Add(Sprite.DefaultLink);
            ShipSprites.Add(ShipSprite.DefaultShip);
            _loadSpritesTask = Task.Run(() => LoadSprites())
                .ContinueWith(_ => Trace.WriteLine("Finished loading sprites."));
        }

#nullable enable
        public PlandoConfig? PlandoConfig { get; set; }
#nullable disable

        public bool PlandoMode => PlandoConfig != null;

        /// <summary>
        /// Gets the visibility of controls which should be hidden when plando
        /// mode is active.
        /// </summary>
        public Visibility InvisibleInPlando => PlandoMode ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Gets the visibility of controls which should be shown only when
        /// plando mode is active.
        /// </summary>
        public Visibility VisibleInPlando => PlandoMode ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets the IsEnabled value for controls which should be disabled when
        /// plando mode is active.
        /// </summary>
        public bool DisabledInPlando => !PlandoMode;

        public ObservableCollection<Sprite> SamusSprites { get; } = new();

        public ObservableCollection<Sprite> LinkSprites { get; } = new();

        public ObservableCollection<ShipSprite> ShipSprites { get; } = new();

        public RandomizerOptions Options
        {
            get => _options;
            set
            {
                DataContext = value;
                _options = value;
                PopulateItemOptions();
                PopulateLogicOptions();
                PopulateLocationOptions();
                UpdateRaceCheckBoxes();
            }
        }

        /// <summary>
        /// Populates the options for early items
        /// </summary>
        public void PopulateItemOptions()
        {
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (_itemService.GetOrDefault(itemType)?.IsProgression(null) != true)
                {
                    continue;
                }

                var itemTypeField = itemType.GetType().GetField(itemType.ToString());
                var description = itemTypeField.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault().Description;
                var checkBox = new CheckBox
                {
                    Name = itemTypeField.Name,
                    Content = description,
                    Tag = itemType,
                    IsChecked = Options.SeedOptions.EarlyItems.Contains(itemType),
                    ToolTip = description
                };
                checkBox.Checked += EarlyItemCheckbox_Checked;
                checkBox.Unchecked += EarlyItemCheckbox_Checked;
                EarlyItemsGrid.Children.Add(checkBox);
            }
        }

        /// <summary>
        /// Populates the two grids with the logic option controls using
        /// reflection
        /// </summary>
        public void PopulateLogicOptions()
        {
            var type = Options.LogicConfig.GetType();

            foreach (var property in type.GetProperties())
            {
                var name = property.Name;
                var displayName = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? name;
                var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? displayName;
                var category = property.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Logic";

                var parent = category switch
                {
                    "Logic" => LogicGrid,
                    "Tricks" => TricksGrid,
                    "Patches" => PatchesGrid,
                    _ => LogicGrid
                };

                if (property.PropertyType == typeof(bool))
                {
                    var checkBox = new CheckBox
                    {
                        Name = name,
                        Content = displayName,
                        IsChecked = (bool)property.GetValue(Options.LogicConfig),
                        ToolTip = description
                    };
                    checkBox.Checked += LogicCheckBox_Checked;
                    checkBox.Unchecked += LogicCheckBox_Checked;
                    parent.Children.Add(checkBox);
                }
            }
        }

        /// <summary>
        /// Populates the grid with all of the locations and the item options
        /// </summary>
        public void PopulateLocationOptions()
        {
            var world = new World(new(), "", 0, "");

            // Populate the regions filter dropdown
            LocationsRegionFilter.Items.Add("");
            foreach (var region in world.Regions.OrderBy(x => x is Z3Region))
            {
                var name = $"{(region is Z3Region ? "Zelda" : "Metroid")} - {region.Name}";
                LocationsRegionFilter.Items.Add(name);
            }

            // Create rows for each location to be able to specify the items at
            // that location
            var row = 0;
            foreach (var location in world.Locations.OrderBy(x => x.Room == null ? "" : x.Room.Name).ThenBy(x => x.Name))
            {
                var locationDetails = _locations.Single(x => x.LocationNumber == location.Id); //TODO: Refactor into IWorldService
                var name = locationDetails.ToString();
                var toolTip = "";
                if (locationDetails.Name.Count > 1)
                {
                    toolTip = "AKA: " + string.Join(", ", locationDetails.Name.Where(x => x.Text != name).Select(x => x.Text)) + "\n";
                }
                toolTip += $"Vanilla item: {location.VanillaItem}";

                var textBlock = new TextBlock
                {
                    Text = name,
                    ToolTip = toolTip,
                    Tag = location,
                    Margin = new(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Visibility = Visibility.Collapsed
                };

                LocationsGrid.Children.Add(textBlock);
                LocationsGrid.RowDefinitions.Add(new RowDefinition());
                Grid.SetColumn(textBlock, 0);
                Grid.SetRow(textBlock, row);

                var comboBox = CreateLocationComboBox(location);
                LocationsGrid.Children.Add(comboBox);
                Grid.SetColumn(comboBox, 1);
                Grid.SetRow(comboBox, row);

                row++;
            }
        }

        public void LoadSprites()
        {
            var spritesPath = Path.Combine(
                Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "Sprites");
            var sprites = Directory.EnumerateFiles(spritesPath, "*.rdc", SearchOption.AllDirectories)
                .Select(x => Sprite.LoadSprite(x))
                .OrderBy(x => x.Name);

            var shipSpritesPath = Path.Combine(AppContext.BaseDirectory, "Sprites", "Ships");
            var shipSprites = Directory.EnumerateFiles(shipSpritesPath, "*.ips", SearchOption.AllDirectories)
                .Select(x => new ShipSprite(Path.GetFileNameWithoutExtension(x), Path.GetRelativePath(shipSpritesPath, x)));

            Dispatcher.Invoke(() =>
            {
                foreach (var sprite in sprites)
                {
                    switch (sprite.SpriteType)
                    {
                        case SpriteType.Samus:
                            SamusSprites.Add(sprite);
                            break;

                        case SpriteType.Link:
                            LinkSprites.Add(sprite);
                            break;
                    }
                }

                foreach (var ship in shipSprites)
                {
                    ShipSprites.Add(ship);
                }
            }, DispatcherPriority.Loaded);
        }

        private static bool IsScam(ItemType itemType) => itemType.IsInCategory(ItemCategory.Scam);

        /// <summary>
        /// Creates a combo box for the item options for a location
        /// </summary>
        /// <param name="location">
        /// The location to generate the combo box for
        /// </param>
        /// <returns>The generated combo box</returns>
        private ComboBox CreateLocationComboBox(Location location)
        {
            var comboBox = new ComboBox
            {
                Tag = location,
                Visibility = Visibility.Collapsed,
                Margin = new(0, 2, 5, 2),
            };

            var prevValue = 0;
            if (Options.SeedOptions.LocationItems.ContainsKey(location.Id))
            {
                prevValue = Options.SeedOptions.LocationItems[location.Id];
            }

            var curIndex = 0;
            var selectedIndex = 0;

            // Add generic item placement options (Any, Progressive Items, Junk)
            foreach (var itemPlacement in Enum.GetValues(typeof(ItemPool)))
            {
                if ((int)itemPlacement == prevValue)
                {
                    selectedIndex = curIndex;
                }

                var itemPlacementField = itemPlacement.GetType().GetField(itemPlacement.ToString());
                var description = itemPlacementField.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();
                comboBox.Items.Add(new LocationItemOption { Value = (int)itemPlacement, Text = description == null ? itemPlacementField.Name : description.Description });
                curIndex++;
            }

            // Add specific progressive items
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (_itemService.GetOrDefault(itemType)?.IsProgression(null) != true)
                {
                    continue;
                }

                if ((int)itemType == prevValue)
                {
                    selectedIndex = curIndex;
                }

                var itemTypeField = itemType.GetType().GetField(itemType.ToString());
                var description = itemTypeField.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();
                comboBox.Items.Add(new LocationItemOption { Value = (int)itemType, Text = description == null ? itemTypeField.Name : description.Description });
                curIndex++;
            }

            comboBox.SelectedIndex = selectedIndex;
            comboBox.SelectionChanged += LocationsItemDropdown_SelectionChanged;

            return comboBox;
        }

        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            string error;
            var successful = PlandoMode
                ? _romGenerator.GeneratePlandoRom(Options, PlandoConfig, out _, out error, out _)
                : _romGenerator.GenerateRandomRom(Options, out _, out error, out _);
            if (!successful)
            {
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(this, error, "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Options.Save();
            }
            catch
            {
                // Oh well
            }
        }

        private void GenerateStatsButton_Click(object sender, RoutedEventArgs e)
        {
            var config = Options.ToConfig();
            var randomizer = _serviceProvider.GetRequiredService<Smz3Randomizer>();

            const int numberOfSeeds = 1000;
            var progressDialog = new ProgressDialog(this, $"Generating {numberOfSeeds} seeds...");
            var stats = InitStats();
            var itemCounts = new ConcurrentDictionary<(int itemId, int locationId), int>();
            var ct = progressDialog.CancellationToken;
            var finished = false;
            var genTask = Task.Run(() =>
            {
                var i = 0;
                Parallel.For(0, numberOfSeeds, (iteration, state) =>
                {
                    try
                    {
                        ct.ThrowIfCancellationRequested();
                        var seed = randomizer.GenerateSeed(config.SeedOnly(), null, ct);

                        ct.ThrowIfCancellationRequested();
                        GatherStats(stats, seed);
                        AddToMegaSpoilerLog(itemCounts, seed);
                    }
                    catch (Exception)
                    {
                    }

                    var seedsGenerated = Interlocked.Increment(ref i);
                    progressDialog.Report(seedsGenerated / (double)numberOfSeeds);
                });

                finished = true;
                progressDialog.Dispatcher.Invoke(progressDialog.Close);
            }, ct);

            progressDialog.StartTimer();
            var result = progressDialog.ShowDialog();
            try
            {
                genTask.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) { }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Any(x => !x.GetType().IsAssignableTo(typeof(OperationCanceledException))))
                    throw;
            }

            if (finished)
            {
                ReportStats(stats, itemCounts, numberOfSeeds);
                WriteMegaSpoilerLog(itemCounts);
            }
        }

        private void AddToMegaSpoilerLog(ConcurrentDictionary<(int itemId, int locationId), int> itemCounts, SeedData seed)
        {
            foreach (var location in seed.Worlds[0].World.Locations)
            {
                itemCounts.Increment(((int)location.Item.Type, location.Id));
            }
        }

        private ConcurrentDictionary<string, int> InitStats()
        {
            var stats = new ConcurrentDictionary<string, int>();
            stats.TryAdd("Successfully generated", 0);
            stats.TryAdd("Shaktool betrays you", 0);
            stats.TryAdd("Zora is a scam", 0);
            stats.TryAdd("Catfish is a scamfish", 0);
            stats.TryAdd("\"I want to go on something more thrilling than Peg World.\"", 0);
            stats.TryAdd("The Morph Ball is in its original location", 0);
            stats.TryAdd("The GT Moldorm chest contains a Metroid item", 0);
            return stats;
        }

        private void GatherStats(ConcurrentDictionary<string, int> stats, SeedData seed)
        {
            var world = seed.Worlds.Single();

            stats.Increment("Successfully generated");

            if (IsScam(world.World.InnerMaridia.ShaktoolItem.Item.Type))
                stats.Increment("Shaktool betrays you");

            if (IsScam(world.World.LightWorldNorthEast.ZorasDomain.Zora.Item.Type))
                stats.Increment("Zora is a scam");

            if (IsScam(world.World.DarkWorldNorthEast.Catfish.Item.Type))
                stats.Increment("Catfish is a scamfish");

            if (IsScam(world.World.DarkWorldNorthWest.PegWorld.Item.Type))
                stats.Increment("\"I want to go on something more thrilling than Peg World.\"");

            if (world.World.BlueBrinstar.MorphBall.Item.Type == ItemType.Morph)
                stats.Increment("The Morph Ball is in its original location");

            if (world.World.GanonsTower.MoldormChest.Item.Type.IsInCategory(ItemCategory.Metroid))
                stats.Increment("The GT Moldorm chest contains a Metroid item");
        }

        private void WriteMegaSpoilerLog(ConcurrentDictionary<(int itemId, int locationId), int> itemCounts)
        {
            var items = Enum.GetValues<ItemType>().ToDictionary(x => (int)x);
            var locations = new SMZ3.World(new Config(), "", 0, "").Locations;

            var itemLocations = items.Values
                .Where(item => itemCounts.Keys.Any(x => x.itemId == (int)item))
                .ToDictionary(
                    keySelector: item => item.GetDescription(),
                    elementSelector: item => itemCounts.Where(x => x.Key.itemId == (int)item)
                        .OrderByDescending(x => x.Value)
                        .ThenBy(x => locations.Single(y => y.Id == x.Key.locationId).ToString())
                        .ToDictionary(
                            keySelector: x => locations.Single(y => y.Id == x.Key.locationId).ToString(),
                            elementSelector: x => x.Value)
            );

            // Area > region > location
            var locationItems = locations.Select(location => new
            {
                Area = location.Region.Area,
                Name = location.ToString(),
                Items = itemCounts.Where(x => x.Key.locationId == location.Id)
                        .OrderByDescending(x => x.Value)
                        .ThenBy(x => x.Key.itemId)
                        .ToDictionary(
                            keySelector: x => items[x.Key.itemId].GetDescription(),
                            elementSelector: x => x.Value)
            })
                .GroupBy(x => x.Area, x => new { x.Name, x.Items })
                .ToDictionary(x => x.Key, x => x.ToList());

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(new
            {
                ByItem = itemLocations,
                ByLocation = locationItems
            }, options);

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SMZ3CasRandomizer", "item_generation_stats.json");
            File.WriteAllText(path, json);

            var startInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }

        private void ReportStats(IDictionary<string, int> stats,
            ConcurrentDictionary<(int itemId, int locationId), int> itemCounts, int total)
        {
            var message = new StringBuilder();
            message.AppendLine($"If you were to play {total} seeds:");
            foreach (var key in stats.Keys)
            {
                var number = stats[key];
                var percentage = number / (double)total;
                message.AppendLine($"- {key} {number} time(s) ({percentage:P1})");
            }
            message.AppendLine();
            message.AppendLine($"Morph ball is in {UniqueLocations(ItemType.Morph)} unique locations.");

            MessageBox.Show(this, message.ToString(), Title, MessageBoxButton.OK, MessageBoxImage.Information);

            int UniqueLocations(ItemType item)
            {
                return itemCounts.Keys
                    .Where(x => x.itemId == (int)item)
                    .Select(x => x.locationId)
                    .Count();
            }
        }

        /// <summary>
        /// Updates the LogicOptions based on when a checkbox is
        /// checked/unchecked using reflection
        /// </summary>
        /// <param name="sender">The checkbox that was checked</param>
        /// <param name="e">The event object</param>
        private void LogicCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var type = Options.LogicConfig.GetType();
            var property = type.GetProperty(checkBox.Name);
            property.SetValue(Options.LogicConfig, checkBox.IsChecked ?? false);
        }

        /// <summary>
        /// Updates to the dropdown to filter locations to specific regions
        /// </summary>
        /// <param name="sender">The dropdown that was updated</param>
        /// <param name="e">The event object</param>
        private void LocationsRegionFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var selectedRegion = comboBox.SelectedItem as string;
            foreach (FrameworkElement obj in LocationsGrid.Children)
            {
                if (obj.Tag is Location location)
                {
                    obj.Visibility = selectedRegion.Contains(location.Region.Name) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Handles updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationsItemDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var option = comboBox.SelectedItem as LocationItemOption;
            var location = comboBox.Tag as Location;
            if (option.Value > 0)
            {
                Options.SeedOptions.LocationItems[location.Id] = option.Value;
            }
            else
            {
                Options.SeedOptions.LocationItems.Remove(location.Id);
            }
        }

        /// <summary>
        /// Resets all locations to be any item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetAllLocationsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (FrameworkElement obj in LocationsGrid.Children)
            {
                if (obj is not ComboBox comboBox) continue;
                var location = comboBox.Tag as Location;
                comboBox.SelectedIndex = 0;
                Options.SeedOptions.LocationItems.Remove(location.Id);
            }
        }

        /// <summary>
        /// Updates the EarlyItems based on when a checkbox is checked/unchecked
        /// using reflection
        /// </summary>
        /// <param name="sender">The checkbox that was checked</param>
        /// <param name="e">The event object</param>
        private void EarlyItemCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var itemType = (ItemType)checkBox.Tag;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
                Options.SeedOptions.EarlyItems.Add(itemType);
            else
                Options.SeedOptions.EarlyItems.Remove(itemType);
        }

        private void RaceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateRaceCheckBoxes();
        }

        private void RaceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateRaceCheckBoxes();
        }

        private void UpdateRaceCheckBoxes()
        {
            DisableSpoilerLogCheckBox.IsEnabled = !Options?.SeedOptions.Race ?? true;
            DisableTrackerHintsCheckBox.IsEnabled = !Options?.SeedOptions.Race ?? true;
            DisableTrackerSpoilersCheckBox.IsEnabled = !Options?.SeedOptions.Race ?? true;
            DisableCheatsCheckBox.IsEnabled = !Options?.SeedOptions.Race ?? true;
        }

        /// <summary>
        /// Internal class for the location item option combo box
        /// </summary>
        private class LocationItemOption
        {
            public int Value { get; set; }
            public string Text { get; set; }

            public override string ToString() => Text;
        }
    }
}
