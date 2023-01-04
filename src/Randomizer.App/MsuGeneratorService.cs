using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Shared;
using Randomizer.SMZ3.Generation;

namespace Randomizer.App;

public class MsuGeneratorService
{
    private static readonly Dictionary<int, int> s_fallbackSongs = new()
    {
        { 31, 23 }, // Pre-Kraid -> Tension / Hostile Incoming theme
        { 32, 22 }, // Kraid Fight -> Big Boss Battle 2
        { 33, 23 }, // Pre-Phantoon -> Tension / Hostile Incoming theme
        { 34, 22 }, // Phantoon Fight -> Big Boss Battle 2
        { 35, 19 }, // Draygon -> Big Boss Battle 1
        { 36, 19 }, // Draygon -> Big Boss Battle 1
        { 37, 23 }, // Pre-Baby Metroid -> Tension / Hostile Incoming theme
        { 38, 22 }, // Baby Metroid appearance -> Big Boss Battle 2
        { 39, 10 }, // Hyper Mode -> Samus Theme
        { 115, 113 }, // Dark Woods -> Dark Death Mountain
        { 137, 116 }, // Castle Tower -> Hyrule Castle
        { 146, 122 }, // Ganon's Tower -> Crystal Dungeon
        { 147, 121 }, // Armos Knights -> Generic Boss
        { 148, 121 }, // Lanmolas -> Generic Boss
        { 149, 121 }, // Agahnim 1 -> Generic Boss
        { 150, 121 }, // Arrghus -> Generic Boss
        { 151, 121 }, // Helmasaur King -> Generic Boss
        { 152, 121 }, // Vitreous -> Generic Boss
        { 153, 121 }, // Mothula -> Generic Boss
        { 154, 121 }, // Kholdstare -> Generic Boss
        { 155, 121 }, // Moldorm -> Generic Boss
        { 156, 121 }, // Blind -> Generic Boss
        { 157, 121 }, // Trinexx -> Generic Boss
        { 158, 121 }, // Agahnim 2 -> Generic Boss
        { 159, 146 }, // Ganon's Tower Ascent -> Ganon's Tower
        { 160, 102 }, // Light World (Post Ped Pull) -> Light World
        { 161, 109 }, // Dark World (All Crystals) -> Dark World
    };

    /// <summary>
    /// Enables MSU support for a rom
    /// </summary>
    /// <param name="options">The randomizer generation options</param>
    /// <param name="rom">The bytes of the previously generated rom</param>
    /// <param name="romPath">The path to the rom file</param>
    /// <param name="worldGenerationData">World data for using </param>
    /// <param name="error">Any error that was ran into when updating the rom</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool EnableMsu1Support(RandomizerOptions options, byte[] rom, string romPath, WorldGenerationData worldGenerationData, out string error)
    {
        var msuPath = options.PatchOptions.Msu1Path;
        if (!File.Exists(msuPath))
        {
            error = "";
            return false;
        }

        var romDrive = Path.GetPathRoot(romPath);
        var msuDrive = Path.GetPathRoot(msuPath);
        if (romDrive?.Equals(msuDrive, StringComparison.OrdinalIgnoreCase) == false)
        {
            error = $"Due to technical limitations, the MSU-1 " +
                $"pack and the ROM need to be on the same drive. MSU-1 " +
                $"support cannot be enabled.\n\nPlease move or copy the MSU-1 " +
                $"files to somewhere on {romDrive}, or change the ROM output " +
                $"folder setting to be on the {msuDrive} drive.";
            return false;
        }

        var romFolder = Path.GetDirectoryName(romPath);
        var msuFolder = Path.GetDirectoryName(msuPath);
        var romBaseName = Path.GetFileNameWithoutExtension(romPath);
        var msuBaseName = Path.GetFileNameWithoutExtension(msuPath);

        // First copy base files
        foreach (var msuFile in Directory.EnumerateFiles(msuFolder!, $"{msuBaseName}*"))
        {
            var fileName = Path.GetFileName(msuFile);
            var suffix = fileName.Replace(msuBaseName, "");

            var link = Path.Combine(romFolder!, romBaseName + suffix);
            NativeMethods.CreateHardLink(link, msuFile, IntPtr.Zero);
        }

        // Loop through dungeons and fix missing pendant/crystal themes
        var fallbackPendantFile = Path.Combine(romFolder!, $"{romBaseName}-117.pcm");
        var fallbackCrystalFile = Path.Combine(romFolder!, $"{romBaseName}-122.pcm");
        foreach (var dungeon in worldGenerationData.World.Dungeons.Where(x => x.HasReward && x is not CastleTower))
        {
            var intendedPcmIndex = 135 + dungeon.SongIndex;
            var intendedPcmFile = Path.Combine(romFolder!, $"{romBaseName}-{intendedPcmIndex}.pcm");
            if (File.Exists(intendedPcmFile))
            {
                if (!options.PatchOptions.EnableExtendedSoundtrack)
                    File.Delete(intendedPcmFile);
                else
                    continue;
            }
            var fallbackPcmFile = dungeon.DungeonRewardType is RewardType.CrystalBlue or RewardType.CrystalRed
                ? fallbackCrystalFile
                : fallbackPendantFile;
            if (File.Exists(fallbackPcmFile))
                NativeMethods.CreateHardLink(intendedPcmFile, fallbackPcmFile, IntPtr.Zero);
        }

        // Loop through files with fallbacks
        foreach (var intendedPcmIndex in s_fallbackSongs.Keys.OrderBy(x => x))
        {
            var intendedPcmFile = Path.Combine(romFolder!, $"{romBaseName}-{intendedPcmIndex}.pcm");
            if (File.Exists(intendedPcmFile)) continue;
            var fallbackPcmIndex = s_fallbackSongs[intendedPcmIndex];
            var fallbackPcmFile = Path.Combine(romFolder!, $"{romBaseName}-{fallbackPcmIndex}.pcm");
            if (File.Exists(fallbackPcmFile))
                NativeMethods.CreateHardLink(intendedPcmFile, fallbackPcmFile, IntPtr.Zero);
        }

        // To avoid weirdness with missing boss fanfare, use an empty pcm file in its place
        var bossVictoryPcmFile = Path.Combine(romFolder!, $"{romBaseName}-119.pcm");
        if (!File.Exists(bossVictoryPcmFile))
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Randomizer.App.Resources.empty.pcm");
            if (stream == null) throw new FileNotFoundException("empty.pcm file not found");
            using var fileStream = new FileStream(bossVictoryPcmFile, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
        }

        error = "";
        return true;
    }

}
