using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using Randomizer.App.ViewModels;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.FileData;
using Randomizer.SMZ3.Generation;

namespace Randomizer.App
{
    public class RomGenerator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RandomizerContext _dbContext;
        private readonly Smz3Randomizer _randomizer;
        private readonly ILogger<GenerateRomWindow> _logger;

        public RomGenerator(IServiceProvider serviceProvider,
            Smz3Randomizer randomizer,
            ILogger<GenerateRomWindow> logger,
            RandomizerContext dbContext)
        {
            _serviceProvider = serviceProvider;
            _randomizer = randomizer;
            _logger = logger;
            _dbContext = dbContext;
        }

        public bool GenerateRom(RandomizerOptions options, out string path, out string error)
        {
            var bytes = GenerateRomBytes(options, out var seed);

            var folderPath = Path.Combine(options.RomOutputPath, $"{DateTimeOffset.Now:yyyyMMdd-HHmmss}_{seed.Seed}");
            Directory.CreateDirectory(folderPath);

            var romFileName = $"SMZ3_Cas_{DateTimeOffset.Now:yyyyMMdd-HHmmss}_{seed.Seed}.sfc";
            var romPath = Path.Combine(folderPath, romFileName);
            EnableMsu1Support(options, bytes, romPath, out var msuError);
            Rom.UpdateChecksum(bytes);
            File.WriteAllBytes(romPath, bytes);

            var spoilerLog = GetSpoilerLog(options, seed);
            var spoilerPath = Path.ChangeExtension(romPath, ".txt");
            File.WriteAllText(spoilerPath, spoilerLog);

            SaveSeedToDatabase(options, seed, romPath, spoilerPath);

            error = msuError;
            path = romPath;
            return true;

        }

        protected byte[] GenerateRomBytes(RandomizerOptions options, out SeedData seed)
        {
            seed = GenerateSeed(options);

            byte[] rom;
            using (var smRom = File.OpenRead(options.GeneralOptions.SMRomPath))
            using (var z3Rom = File.OpenRead(options.GeneralOptions.Z3RomPath))
            {
                rom = Rom.CombineSMZ3Rom(smRom, z3Rom);
            }

            using (var ips = GetType().Assembly.GetManifestResourceStream("Randomizer.App.zsm.ips"))
            {
                Rom.ApplyIps(rom, ips);
            }
            Rom.ApplySeed(rom, seed.Worlds[0].Patches);

            options.PatchOptions.SamusSprite.ApplyTo(rom);
            options.PatchOptions.LinkSprite.ApplyTo(rom);
            return rom;
        }

        public SeedData GenerateSeed(RandomizerOptions options, string seed = null)
        {
            var config = options.ToConfig();
            return _randomizer.GenerateSeed(config, seed ?? options.SeedOptions.Seed, CancellationToken.None);
        }

        private string GetSpoilerLog(RandomizerOptions options, SeedData seed)
        {
            var log = new StringBuilder();
            log.AppendLine(Underline($"SMZ3 Cas’ spoiler log", '='));
            log.AppendLine($"Generated on {DateTime.Now:F}");
            log.AppendLine($"Seed: {options.SeedOptions.Seed} (actual: {seed.Seed})");
            log.AppendLine($"Sword: {options.SeedOptions.SwordLocation}");
            log.AppendLine($"Morph: {options.SeedOptions.MorphLocation}");
            log.AppendLine($"Bombs: {options.SeedOptions.MorphBombsLocation}");
            log.AppendLine($"Shaktool: {options.SeedOptions.ShaktoolItem}");
            log.AppendLine($"Peg World: {options.SeedOptions.PegWorldItem}");
            log.AppendLine((options.SeedOptions.Keysanity ? "[Keysanity] " : "")
                         + (options.SeedOptions.Race ? "[Race] " : ""));
            if (File.Exists(options.PatchOptions.Msu1Path))
                log.AppendLine($"MSU-1 pack: {Path.GetFileNameWithoutExtension(options.PatchOptions.Msu1Path)}");
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

        private static string Underline(string text, char line = '-')
            => text + "\n" + new string(line, text.Length);

        private bool EnableMsu1Support(RandomizerOptions options, byte[] rom, string romPath, out string error)
        {
            var msuPath = options.PatchOptions.Msu1Path;
            if (!File.Exists(msuPath))
            {
                error = "";
                return false;
            }

            var romDrive = Path.GetPathRoot(romPath);
            var msuDrive = Path.GetPathRoot(msuPath);
            if (!romDrive.Equals(msuDrive, StringComparison.OrdinalIgnoreCase))
            {
                error = $"Due to technical limitations, the MSU-1 " +
                    $"pack and the ROM need to be on the same drive. MSU-1 " +
                    $"support cannot be enabled.\n\nPlease move or copy the MSU-1 " +
                    $"files to somewhere on {romDrive}, or change the ROM output " +
                    $"folder setting to be on the {msuDrive} drive.";
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

            error = "";
            return true;
        }

        protected void SaveSeedToDatabase(RandomizerOptions options, SeedData seed, String romPath, string spoilerPath)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string settings = JsonSerializer.Serialize(options.ToConfig(), jsonOptions);

            _dbContext.GeneratedRoms.Add(new GeneratedRom()
            {
                Seed = seed.Seed,
                RomPath = Path.GetRelativePath(options.RomOutputPath, romPath),
                SpoilerPath = Path.GetRelativePath(options.RomOutputPath, spoilerPath),
                Date = DateTimeOffset.Now,
                Settings = settings
            });
            _dbContext.SaveChanges();
        }
    }
}
