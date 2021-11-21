using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;

using Randomizer.App.ViewModels;
using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Generation;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for GenerateRomWindow.xaml
    /// </summary>
    public partial class GenerateRomWindow : Window
    {
        private readonly Task _loadSpritesTask;
        private readonly IServiceProvider _serviceProvider;
        private readonly Smz3Randomizer _randomizer;
        private readonly RomGenerator _romGenerator;
        private RandomizerOptions _options;

        public GenerateRomWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _randomizer = serviceProvider.GetService<Smz3Randomizer>();
            _romGenerator = serviceProvider.GetService<RomGenerator>();
            InitializeComponent();

            SamusSprites.Add(Sprite.DefaultSamus);
            LinkSprites.Add(Sprite.DefaultLink);
            _loadSpritesTask = Task.Run(() => LoadSprites())
                .ContinueWith(_ => Trace.WriteLine("Finished loading sprites."));
        }

        public ObservableCollection<Sprite> SamusSprites { get; } = new();

        public ObservableCollection<Sprite> LinkSprites { get; } = new();

        public RandomizerOptions Options
        {
            get => _options;
            set
            {
                DataContext = value;
                _options = value;
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
            }, DispatcherPriority.Loaded);
        }

        private static bool IsScam(ItemType itemType) => itemType.IsInCategory(ItemCategory.Scam);

        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            bool successful = _romGenerator.GenerateRom(Options, out var romPath, out var error, out var rom);
            if (!successful && !string.IsNullOrEmpty(error))
            {
                MessageBox.Show(this, error, "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                Options.Save(OptionsFactory.GetFilePath());
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

            const int numberOfSeeds = 10000;
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
                    ct.ThrowIfCancellationRequested();
                    var seed = randomizer.GenerateSeed(config.SeedOnly(), null, ct);

                    ct.ThrowIfCancellationRequested();
                    GatherStats(stats, seed);
                    AddToMegaSpoilerLog(itemCounts, seed);

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
    }
}
