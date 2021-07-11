using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

        public MainWindow()
        {
            InitializeComponent();


            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SMZ3CasRandomizer");
            Directory.CreateDirectory(basePath);
            _optionsPath = Path.Combine(basePath, "options.json");

            try
            {
                if (!File.Exists(_optionsPath))
                    DataContext = new RandomizerOptions(this);

                DataContext = RandomizerOptions.Load(_optionsPath)
                    .WithOwner(this);
            }
            catch
            {
                DataContext = new RandomizerOptions(this);
            }
        }

        protected RandomizerOptions Options => DataContext as RandomizerOptions;

        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            var randomizer = new SMZ3.Randomizer();
            var options = Options.ToDictionary();
            var seed = randomizer.GenerateSeed(options, Options.Seed, CancellationToken.None);

            using var smRom = File.OpenRead(Options.SMRomPath);
            using var z3Rom = File.OpenRead(Options.Z3RomPath);
            var rom = Rom.CombineSMZ3Rom(smRom, z3Rom);

            using var ips = GetType().Assembly.GetManifestResourceStream("Randomizer.App.zsm.ips");
            Rom.ApplyIps(rom, ips);
            Rom.ApplySeed(rom, seed.Worlds.First().Patches);
            // Apply additional IPS or RDC patches here

            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SMZ3CasRandomizer", "Seeds");
            Directory.CreateDirectory(basePath);

            var fileName = $"SMZ3_Cas_{DateTimeOffset.Now:yyyyMMdd-HHmmss}_{seed.Seed}.sfc";
            var path = Path.Combine(basePath, fileName);
            File.WriteAllBytes(path, rom);

            var spoilerLog = GetSpoilerLog(seed, randomizer);
            File.WriteAllText(Path.ChangeExtension(path, ".txt"), spoilerLog);

            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
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
            var items = randomizer.GetItems();
            var locations = randomizer.GetLocations();

            const int numberOfSeeds = 10000;
            var progressDialog = new ProgressDialog(this, $"Generating {numberOfSeeds} seeds...");
            var stats = new Dictionary<string, int>();
            var ct = progressDialog.CancellationToken;
            var genTask = Task.Run(() =>
            {
                for (var i = 0; i < numberOfSeeds && !ct.IsCancellationRequested; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var seed = randomizer.GenerateSeed(options, null, ct);

                    ct.ThrowIfCancellationRequested();
                    GatherStats(stats, seed);
                    progressDialog.Report((i + 1) / (double)numberOfSeeds);
                }

                progressDialog.Dispatcher.Invoke(progressDialog.Close);
            }, ct);

            progressDialog.ShowDialog();
            try
            {
                genTask.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) { }

            ReportStats(stats, numberOfSeeds);
        }

        private void GatherStats(Dictionary<string, int> stats, ISeedData seed)
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

        private void ReportStats(Dictionary<string, int> stats, int total)
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