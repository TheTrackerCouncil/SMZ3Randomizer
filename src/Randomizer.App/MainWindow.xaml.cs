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
using Microsoft.Extensions.Logging;

using Randomizer.App.ViewModels;
using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.FileData;
using Randomizer.SMZ3.Generation;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Task _loadSpritesTask;
        private readonly IServiceProvider _serviceProvider;
        private readonly Smz3Randomizer _randomizer;
        private readonly ILogger<MainWindow> _logger;
        private TrackerWindow _trackerWindow;

        public MainWindow(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            Smz3Randomizer randomizer,
            ILogger<MainWindow> logger)
        {
            _serviceProvider = serviceProvider;
            _randomizer = randomizer;
            _logger = logger;
            InitializeComponent();

            SamusSprites.Add(Sprite.DefaultSamus);
            LinkSprites.Add(Sprite.DefaultLink);
            _loadSpritesTask = Task.Run(() => LoadSprites())
                .ContinueWith(_ => Trace.WriteLine("Finished loading sprites."));

            Options = optionsFactory.Create();
            DataContext = Options;
            CheckSpeechRecognition();
        }

        public ObservableCollection<Sprite> SamusSprites { get; } = new();

        public ObservableCollection<Sprite> LinkSprites { get; } = new();

        public RandomizerOptions Options { get; }

        public string RomOutputPath
        {
            get => Directory.Exists(Options.GeneralOptions.RomOutputPath)
                    ? Options.GeneralOptions.RomOutputPath
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Seeds");
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

        public string SaveRomToFile()
        {
            var rom = GenerateRom(out var seed);

            var folderPath = Path.Combine(RomOutputPath, $"{DateTimeOffset.Now:yyyyMMdd-HHmmss}_{seed.Seed}");
            Directory.CreateDirectory(folderPath);

            var romFileName = $"SMZ3_Cas_{DateTimeOffset.Now:yyyyMMdd-HHmmss}_{seed.Seed}.sfc";
            var romPath = Path.Combine(folderPath, romFileName);
            EnableMsu1Support(rom, romPath);
            Rom.UpdateChecksum(rom);
            File.WriteAllBytes(romPath, rom);

            var spoilerLog = GetSpoilerLog(seed);
            File.WriteAllText(Path.ChangeExtension(romPath, ".txt"), spoilerLog);

            return romPath;
        }

        protected byte[] GenerateRom(out SeedData seed)
        {
            var config = Options.ToConfig();
            seed = _randomizer.GenerateSeed(config, Options.SeedOptions.Seed, CancellationToken.None);

            byte[] rom;
            using (var smRom = File.OpenRead(Options.GeneralOptions.SMRomPath))
            using (var z3Rom = File.OpenRead(Options.GeneralOptions.Z3RomPath))
            {
                rom = Rom.CombineSMZ3Rom(smRom, z3Rom);
            }

            using (var ips = GetType().Assembly.GetManifestResourceStream("Randomizer.App.zsm.ips"))
            {
                Rom.ApplyIps(rom, ips);
            }
            Rom.ApplySeed(rom, seed.Worlds[0].Patches);

            Options.PatchOptions.SamusSprite.ApplyTo(rom);
            Options.PatchOptions.LinkSprite.ApplyTo(rom);
            return rom;
        }

        private static string Underline(string text, char line = '-')
            => text + "\n" + new string(line, text.Length);

        private static bool IsScam(ItemType itemType) => itemType.IsInCategory(ItemCategory.Scam);

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

        private string GetSpoilerLog(SeedData seed)
        {
            var log = new StringBuilder();
            log.AppendLine(Underline($"SMZ3 Cas’ spoiler log", '='));
            log.AppendLine($"Generated on {DateTime.Now:F}");
            log.AppendLine($"Seed: {Options.SeedOptions.Seed} (actual: {seed.Seed})");
            log.AppendLine($"Sword: {Options.SeedOptions.SwordLocation}");
            log.AppendLine($"Morph: {Options.SeedOptions.MorphLocation}");
            log.AppendLine($"Bombs: {Options.SeedOptions.MorphBombsLocation}");
            log.AppendLine($"Shaktool: {Options.SeedOptions.ShaktoolItem}");
            log.AppendLine($"Peg World: {Options.SeedOptions.PegWorldItem}");
            log.AppendLine((Options.SeedOptions.Keysanity ? "[Keysanity] " : "")
                         + (Options.SeedOptions.Race ? "[Race] " : ""));
            if (File.Exists(Options.PatchOptions.Msu1Path))
                log.AppendLine($"MSU-1 pack: {Path.GetFileNameWithoutExtension(Options.PatchOptions.Msu1Path)}");
            log.AppendLine();

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

            var world = seed.Worlds.Single();
            foreach (var location in world.World.Locations)
            {
                log.AppendLine($"{location}: {location.Item}");
            }

            return log.ToString();
        }

        private bool EnableMsu1Support(byte[] rom, string romPath)
        {
            var msuPath = Options.PatchOptions.Msu1Path;
            if (!File.Exists(msuPath))
                return false;

            var romDrive = Path.GetPathRoot(romPath);
            var msuDrive = Path.GetPathRoot(msuPath);
            if (!romDrive.Equals(msuDrive, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, $"Due to technical limitations, the MSU-1 " +
                    $"pack and the ROM need to be on the same drive. MSU-1 " +
                    $"support cannot be enabled.\n\nPlease move or copy the MSU-1 " +
                    $"files to somewhere on {romDrive}, or change the ROM output " +
                    $"folder setting to be on the {msuDrive} drive.",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using (var ips = GetType().Assembly.GetManifestResourceStream("Randomizer.App.msu1-v6.ips"))
            {
                Rom.ApplyIps(rom, ips);
            }

            var romFolder = Path.GetDirectoryName(romPath);
            var msuFolder = Path.GetDirectoryName(msuPath);
            var romBaseName = Path.GetFileNameWithoutExtension(romPath);
            var msuBaseName = Path.GetFileNameWithoutExtension(msuPath);
            foreach (var msuFile in Directory.EnumerateFiles(msuFolder, $"{msuBaseName}*"))
            {
                var fileName = Path.GetFileName(msuFile);
                var suffix = fileName.Replace(msuBaseName, "");

                var link = Path.Combine(romFolder, romBaseName + suffix);
                NativeMethods.CreateHardLink(link, msuFile, IntPtr.Zero);
            }

            return true;
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
        }

        private void GenerateStatsMenuItem_Click(object sender, RoutedEventArgs e)
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

        private void StartTracker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var scope = _serviceProvider.CreateScope();
                _trackerWindow = scope.ServiceProvider.GetRequiredService<TrackerWindow>();
                _trackerWindow.Options = Options;
                _trackerWindow.Closed += (_, _) => scope.Dispose();
                _trackerWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unhandled exception occurred when starting the tracker.");
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var aboutWindow = scope.ServiceProvider.GetRequiredService<AboutWindow>();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }
    }
}
