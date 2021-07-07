using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new RandomizerOptions(this);
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
    }
}