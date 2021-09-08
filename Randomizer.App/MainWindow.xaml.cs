using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Randomizer.App.ViewModels;
using Randomizer.Shared.Contracts;
using Randomizer.Shared.Models;
using Randomizer.SMZ3;
using Randomizer.SMZ3.FileData;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const bool SCAM = true;

        private readonly string _optionsPath;
        private readonly Task _loadSpritesTask;

        public MainWindow()
        {
            InitializeComponent();
            SamusSprites.Add(Sprite.DefaultSamus);
            LinkSprites.Add(Sprite.DefaultLink);

            _loadSpritesTask = Task.Run(() => LoadSprites())
                .ContinueWith(_ => Trace.WriteLine("Finished loading sprites."));

            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SMZ3CasRandomizer");
            Directory.CreateDirectory(basePath);
            _optionsPath = Path.Combine(basePath, "options.json");

            try
            {
                if (!File.Exists(_optionsPath))
                    Options = new RandomizerOptions(this);

                Options = RandomizerOptions.Load(_optionsPath)
                    .WithOwner(this);
            }
            catch
            {
                Options = new RandomizerOptions(this);
            }

            DataContext = Options.Seed;
        }

        public ObservableCollection<Sprite> SamusSprites { get; } = new();

        public ObservableCollection<Sprite> LinkSprites { get; } = new();

        public RandomizerOptions Options { get; }

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

        public string SaveRomToFile()
        {
            var randomizer = new SMZ3.Randomizer();
            var rom = GenerateRom(randomizer, out var seed);

            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SMZ3CasRandomizer", "Seeds");
            Directory.CreateDirectory(folderPath);

            var romFileName = $"SMZ3_Cas_{DateTimeOffset.Now:yyyyMMdd-HHmmss}_{seed.Seed}.sfc";
            var romPath = Path.Combine(folderPath, romFileName);
            File.WriteAllBytes(romPath, rom);

            var spoilerLog = GetSpoilerLog(seed, randomizer);
            File.WriteAllText(Path.ChangeExtension(romPath, ".txt"), spoilerLog);

            return romPath;
        }

        private byte[] GenerateRom(SMZ3.Randomizer randomizer, out ISeedData seed)
        {
            var options = Options.ToDictionary();
            seed = randomizer.GenerateSeed(options, Options.Seed.Seed, CancellationToken.None);

            byte[] rom;
            using (var smRom = File.OpenRead(Options.General.SMRomPath))
            using (var z3Rom = File.OpenRead(Options.General.Z3RomPath))
            {
                rom = Rom.CombineSMZ3Rom(smRom, z3Rom);
            }

            using (var ips = GetType().Assembly.GetManifestResourceStream("Randomizer.App.zsm.ips"))
            {
                Rom.ApplyIps(rom, ips);
            }
            Rom.ApplySeed(rom, seed.Worlds[0].Patches);

            Options.Seed.SamusSprite.ApplyTo(rom);
            Options.Seed.LinkSprite.ApplyTo(rom);
            return rom;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var romPath = SaveRomToFile();
            Process.Start(new ProcessStartInfo
            {
                FileName = romPath,
                UseShellExecute = true
            });
        }

        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            var path = SaveRomToFile();
            Process.Start("explorer.exe", $"/select,\"{path}\"");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Options.General.Validate())
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
            try
            {
                Options.Save(_optionsPath);
            }
            catch
            {
                // Oh well
            }
        }

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var optionsDialog = new OptionsWindow(Options.General);
            optionsDialog.ShowDialog();
        }

        private string GetSpoilerLog(ISeedData seed, IRandomizer randomizer)
        {
            var log = new StringBuilder();

            for (var i = 0; i < seed.Playthrough.Count; i++)
            {
                if (seed.Playthrough[i].Count == 0)
                    continue;

                log.AppendLine(Underline($"Sphere {i + 1}"));
                log.AppendLine();
                foreach (var (location, item) in seed.Playthrough[i])
                    log.AppendLine($"{location}: {item}");
                log.AppendLine();
            }

            log.AppendLine(Underline("All items"));
            log.AppendLine();

            // Why the fuck is this so ass-backwards? This really needs to be simplified
            var items = randomizer.GetItems();
            var locations = randomizer.GetLocations();

            var world = seed.Worlds.Single();
            foreach (var location in world.Locations)
            {
                var itemName = items[location.ItemId].Name;
                var locationName = locations[location.LocationId].Name;
                log.AppendLine($"{locationName}: {itemName}");
            }

            return log.ToString();
        }

        private string Underline(string text, char line = '-')
            => text + "\n" + new string(line, text.Length);

        private static bool IsScam(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.ProgressiveTunic => false,
                ItemType.ProgressiveShield => SCAM,
                ItemType.ProgressiveSword => false,
                ItemType.Bow => false,
                ItemType.SilverArrows => false,
                ItemType.BlueBoomerang => SCAM,
                ItemType.RedBoomerang => SCAM,
                ItemType.Hookshot => false,
                ItemType.Mushroom => false,
                ItemType.Powder => false,
                ItemType.Firerod => false,
                ItemType.Icerod => false,
                ItemType.Bombos => false,
                ItemType.Ether => false,
                ItemType.Quake => false,
                ItemType.Lamp => false,
                ItemType.Hammer => false,
                ItemType.Shovel => false,
                ItemType.Flute => false,
                ItemType.Bugnet => SCAM,
                ItemType.Book => false,
                ItemType.Bottle => SCAM,
                ItemType.Somaria => false,
                ItemType.Byrna => SCAM,
                ItemType.Cape => false,
                ItemType.Mirror => false,
                ItemType.Boots => false,
                ItemType.ProgressiveGlove => false,
                ItemType.Flippers => false,
                ItemType.MoonPearl => false,
                ItemType.HalfMagic => false,
                ItemType.Missile => false,
                ItemType.Super => false,
                ItemType.PowerBomb => false,
                ItemType.Grapple => false,
                ItemType.XRay => SCAM,
                ItemType.Charge => false,
                ItemType.Ice => false,
                ItemType.Wave => false,
                ItemType.Spazer => false,
                ItemType.Plasma => false,
                ItemType.Varia => false,
                ItemType.Gravity => false,
                ItemType.Morph => false,
                ItemType.Bombs => false,
                ItemType.SpringBall => false,
                ItemType.ScrewAttack => false,
                ItemType.HiJump => false,
                ItemType.SpaceJump => false,
                ItemType.SpeedBooster => false,
                _ => SCAM
            };
        }

        private void GenerateStatsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var options = Options.ToDictionary();
            var randomizer = new SMZ3.Randomizer();

            const int numberOfSeeds = 10000;
            var progressDialog = new ProgressDialog(this, $"Generating {numberOfSeeds} seeds...");
            var stats = new ConcurrentDictionary<string, int>();
            var itemCounts = new ConcurrentDictionary<(int itemId, int locationId), int>();
            var ct = progressDialog.CancellationToken;
            var finished = false;
            var genTask = Task.Run(() =>
            {
                var i = 0;
                Parallel.For(0, numberOfSeeds, (iteration, state) =>
                {
                    ct.ThrowIfCancellationRequested();
                    var seed = randomizer.GenerateSeed(options, null, ct);

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
                ReportStats(stats, numberOfSeeds);
                WriteMegaSpoilerLog(itemCounts, randomizer);
            }
        }

        private void AddToMegaSpoilerLog(ConcurrentDictionary<(int itemId, int locationId), int> itemCounts, ISeedData seed)
        {
            foreach (var item in seed.Worlds[0].Locations)
            {
                itemCounts.Increment((item.ItemId, item.LocationId));
            }
        }

        private void GatherStats(ConcurrentDictionary<string, int> stats, ISeedData seed)
        {
            var world = seed.Worlds.Single();

            var shaktool = world.Locations.Single(x => x.LocationId == Locations.Shaktool);
            if (IsScam((ItemType)shaktool.ItemId))
                stats.Increment("Shaktool betrays you");

            var zora = world.Locations.Single(x => x.LocationId == Locations.Zora);
            if (IsScam((ItemType)zora.ItemId))
                stats.Increment("Zora is a scam");

            var scat = world.Locations.Single(x => x.LocationId == Locations.Scatfish);
            if (IsScam((ItemType)scat.ItemId))
                stats.Increment("Scatfish is a scamfish");
        }

        private void WriteMegaSpoilerLog(ConcurrentDictionary<(int itemId, int locationId), int> itemCounts, IRandomizer randomizer)
        {
            var items = randomizer.GetItems();
            var locations = randomizer.GetLocations();

            var itemLocations = items.Values
                .Where(item => itemCounts.Keys.Any(x => x.itemId == item.Id))
                .ToDictionary(
                    keySelector: item => item.Name,
                    elementSelector: item => itemCounts.Where(x => x.Key.itemId == item.Id)
                        .OrderByDescending(x => x.Value)
                        .ThenBy(x => locations[x.Key.locationId].Name)
                        .ToDictionary(
                            keySelector: x => locations[x.Key.locationId].Name,
                            elementSelector: x => x.Value)
            );

            // Area > region > location
            var locationItems = locations.Values.Select(location => new
            {
                Area = location.Area,
                Name = location.Name,
                Items = itemCounts.Where(x => x.Key.locationId == location.Id)
                        .OrderByDescending(x => x.Value)
                        .ThenBy(x => x.Key.itemId)
                        .ToDictionary(
                            keySelector: x => items[x.Key.itemId].Name,
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

        private void ReportStats(IDictionary<string, int> stats, int total)
        {
            var message = new StringBuilder();
            message.AppendLine($"If you were to play {total} seeds:");
            foreach (var key in stats.Keys)
            {
                var number = stats[key];
                var percentage = number / (double)total;
                message.AppendLine($"- {key} {number} time(s) ({percentage:P1})");
            }

            MessageBox.Show(this, message.ToString(), Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static class Locations
        {
            public const int Shaktool = 150;
            public const int Zora = 256 + 36;
            public const int Scatfish = 256 + 78;
        }
    }
}