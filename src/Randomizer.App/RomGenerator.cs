using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;

using Randomizer.App.Patches;
using Randomizer.App.ViewModels;
using Randomizer.Shared;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.FileData;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Regions;

namespace Randomizer.App
{
    /// <summary>
    /// Class to handle generating roms 
    /// </summary>
    public class RomGenerator
    {
        private readonly RandomizerContext _dbContext;
        private readonly Smz3Randomizer _randomizer;

        public RomGenerator(Smz3Randomizer randomizer,
            RandomizerContext dbContext)
        {
            _randomizer = randomizer;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Generates a rom and returns details about the rom
        /// </summary>
        /// <param name="options">The randomizer generation options</param>
        /// <param name="path">The path to the rom</param>
        /// <param name="error">Any error message from generating the rom</param>
        /// <param name="rom">The db entry for the rom</param>
        /// <returns>True if the rom was generated successfully, false otherwise</returns>
        public bool GenerateRom(RandomizerOptions options, out string path, out string error, out GeneratedRom rom)
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

            rom = SaveSeedToDatabase(options, seed, romPath, spoilerPath);

            error = msuError;
            path = romPath;

            return true;

        }

        /// <summary>
        /// Generates a seed for a rom based on the given randomizer options
        /// </summary>
        /// <param name="options">The randomizer generation options</param>
        /// <param name="seed">The string seed to use for generating the rom</param>
        /// <returns>The seed data</returns>
        public SeedData GenerateSeed(RandomizerOptions options, string seed = null)
        {
            var config = options.ToConfig();
            return _randomizer.GenerateSeed(config, seed ?? options.SeedOptions.Seed, CancellationToken.None);
        }

        /// <summary>
        /// Uses the options to generate the rom
        /// </summary>
        /// <param name="options">The randomizer generation options</param>
        /// <param name="seed">The generated seed data</param>
        /// <returns>The bytes of the rom file</returns>
        protected byte[] GenerateRomBytes(RandomizerOptions options, out SeedData seed)
        {
            seed = GenerateSeed(options);

            var assembly = GetType().Assembly;
            var smIpsFiles = new List<Stream>();
            if (options.PatchOptions.CasualSuperMetroidPatches)
            {
                smIpsFiles.Add(IpsPatch.Respin());
            }

            byte[] rom;
            try
            {
                using (var smRom = File.OpenRead(options.GeneralOptions.SMRomPath))
                using (var z3Rom = File.OpenRead(options.GeneralOptions.Z3RomPath))
                {
                    rom = Rom.CombineSMZ3Rom(smRom, z3Rom, smIpsFiles);
                }
            }
            finally
            {
                smIpsFiles.ForEach(x => x.Close());
            }

            using (var ips = IpsPatch.Smz3())
            {
                Rom.ApplyIps(rom, ips);
            }
            Rom.ApplySeed(rom, seed.Worlds[0].Patches);

            options.PatchOptions.SamusSprite.ApplyTo(rom);
            options.PatchOptions.LinkSprite.ApplyTo(rom);

            if (options.PatchOptions.ShipPatch != null)
            {

                var shipPatchFileName = Path.Combine(AppContext.BaseDirectory, "Ships", options.PatchOptions.ShipPatch);
                if (File.Exists(shipPatchFileName))
                {
                    using var customShipBasePatch = IpsPatch.CustomShip();
                    Rom.ApplySuperMetroidIps(rom, customShipBasePatch);

                    using var shipPatch = File.OpenRead(shipPatchFileName);
                    Rom.ApplySuperMetroidIps(rom, shipPatch);
                }
            }

            return rom;
        }
        /// <summary>
        /// Takes the given seed information and saves it to the database
        /// </summary>
        /// <param name="options">The randomizer generation options</param>
        /// <param name="seed">The generated seed data</param>
        /// <param name="romPath">The path to the rom file</param>
        /// <param name="spoilerPath">The path to the spoiler file</param>
        /// <returns>The db entry for the generated rom</returns>
        protected GeneratedRom SaveSeedToDatabase(RandomizerOptions options, SeedData seed, string romPath, string spoilerPath)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string settings = JsonSerializer.Serialize(options.ToConfig(), jsonOptions);

            GeneratedRom rom = new GeneratedRom()
            {
                Seed = seed.Seed,
                RomPath = Path.GetRelativePath(options.RomOutputPath, romPath),
                SpoilerPath = Path.GetRelativePath(options.RomOutputPath, spoilerPath),
                Date = DateTimeOffset.Now,
                Settings = settings
            };
            _dbContext.GeneratedRoms.Add(rom);
            _dbContext.SaveChanges();
            return rom;
        }

        /// <summary>
        /// Underlines text in the spoiler log
        /// </summary>
        /// <param name="text">The text to be underlined</param>
        /// <param name="line">The character to use for underlining</param>
        /// <returns>The text to be underlined followed by the underlining text</returns>
        private static string Underline(string text, char line = '-')
            => text + "\n" + new string(line, text.Length);

        /// <summary>
        /// Gets the spoiler log of a given seed 
        /// </summary>
        /// <param name="options">The randomizer generation options</param>
        /// <param name="seed">The previously generated seed data</param>
        /// <returns>The string output of the spoiler log file</returns>
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

            var spheres = seed.Playthrough.GetPlaythroughText();
            for (var i = 0; i < spheres.Count; i++)
            {
                if (spheres[i].Count == 0)
                    continue;

                log.AppendLine(Underline($"Sphere {i + 1}"));
                log.AppendLine();
                foreach (var (location, item) in spheres[i])
                    log.AppendLine($"{location}: {item}");
                log.AppendLine();
            }

            log.AppendLine(Underline("Rewards"));
            log.AppendLine();
            foreach (var region in seed.Worlds[0].World.Regions)
            {
                if (region is IHasReward rewardRegion && rewardRegion.Reward != Reward.Agahnim && rewardRegion.Reward != Reward.GoldenFourBoss)
                    log.AppendLine($"{region.Name}: {rewardRegion.Reward}");
            }
            log.AppendLine();

            log.AppendLine(Underline("Medallions"));
            log.AppendLine();
            foreach (var region in seed.Worlds[0].World.Regions)
            {
                if (region is INeedsMedallion medallionReegion)
                    log.AppendLine($"{region.Name}: {medallionReegion.Medallion}");
            }
            log.AppendLine();

            log.AppendLine(Underline("All items"));
            log.AppendLine();

            var world = seed.Worlds.Single();
            foreach (var location in world.World.Locations)
            {
                log.AppendLine($"{location}: {location.Item}");
            }

            return log.ToString();
        }
        /// <summary>
        /// Enabled MSU support for a rom
        /// </summary>
        /// <param name="options">The randomizer generation options</param>
        /// <param name="rom">The bytes of the previously generated rom</param>
        /// <param name="romPath">The path to the rom file</param>
        /// <param name="error">Any error that was ran into when updating the rom</param>
        /// <returns>True if successful, false otherwise</returns>
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

            using (var ips = IpsPatch.MsuSupport())
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
    }
}
