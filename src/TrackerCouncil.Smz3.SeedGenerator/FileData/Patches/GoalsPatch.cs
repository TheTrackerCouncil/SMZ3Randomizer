using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

public class GoalsPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        if (data.IsParsedRom && data.Config.ParsedRomDetails!.RomGenerator != RomGenerator.Cas)
        {
            yield return new GeneratedPatch(Snes(0xF47008), [(byte)data.Config.ParsedRomDetails.TourianBossCount, 0x00]);
            yield return new GeneratedPatch(Snes(0xF47012), [(byte)data.Config.ParsedRomDetails.GanonCrystalCount, 0x00]);
            yield break;
        }

        // Open pyramid
        if (data.Config.GameModeOptions.PyramidHole == PyramidHole.Open)
        {
            yield return new GeneratedPatch(0x40008B, [0x01]);
        }

        // Set number of crystals to enter GT
        var gtCrystals = data.Config.GameModeOptions.GanonsTowerCrystalCount;
        yield return new GeneratedPatch(0x40005E, [(byte)gtCrystals]);
        yield return new GeneratedPatch(0x40008C, gtCrystals == 0 ? [0x01] : [0x00]);

        // Crystals to damage Ganon
        var ganonCrystals = data.Config.GameModeOptions.SelectedGameModeType == GameModeType.AllDungeons ? 7 : data.Config.GameModeOptions.GanonCrystalCount;
        yield return new GeneratedPatch(0x40005F, [(byte)ganonCrystals]);
        yield return new GeneratedPatch(Snes(0xF47012), [(byte)data.Config.GameModeOptions.GanonCrystalCount, 0x00]);

        // Bosses to enter Tourian
        var numBosses = data.Config.GameModeOptions.SelectedGameModeType == GameModeType.AllDungeons ? 4 : data.Config.GameModeOptions.TourianBossCount;
        yield return new GeneratedPatch(Snes(0xF47008), [(byte)numBosses, 0x00]);
        yield return new GeneratedPatch(Snes(0xF4700B), numBosses == 0 ? [0x00, 0x04] : [0x00, 0x01]);

        // Randomized boss rewards
        var hasRandomizedBossRewards = data.Config.GameModeOptions.ShuffleMetroidBossTokens;
        yield return new GeneratedPatch(Snes(0xF4700E), hasRandomizedBossRewards ? UshortBytes(0x0001) : UshortBytes(0x0000));

        // Goal specific flags
        if (data.Config.GameModeOptions.SelectedGameModeType == GameModeType.Vanilla)
        {
            yield return new GeneratedPatch(Snes(0x30803E), [0x03]);
        }
        else if (data.Config.GameModeOptions.SelectedGameModeType == GameModeType.AllDungeons)
        {
            yield return new GeneratedPatch(Snes(0x30803E), [0x02]);
        }
        else
        {
            yield return new GeneratedPatch(Snes(0x30803E), [0x05]);
            yield return new GeneratedPatch(Snes(0xF47010), UshortBytes(0x0001));
        }
    }

    public static int GetGanonsTowerCrystalCountFromRom(byte[] rom)
    {
        return rom[Snes(0x30805E)];
    }

    public static int GetGanonCrystalCountFromRom(byte[] rom)
    {
        return rom[Snes(0x30805F)];
    }

    public static int GetTourianBossCountFromRom(byte[] rom, bool isCasRom)
    {
        if (isCasRom)
        {
            return rom[Snes(0xF47008)];
        }
        else
        {
            return BitConverter.ToInt16(rom.Skip(Snes(0xF47200)).Take(2).ToArray());
        }
    }

    public static bool GetOpenPyramid(byte[] rom)
    {
        return rom[Snes(0x30808B)] == 0x01;
    }

    public static GameModeType GetGameModeType(byte[] rom)
    {
        var val = rom[Snes(0x30803E)];
        if (val == 0x02)
        {
            return GameModeType.AllDungeons;
        }
        else
        {
            return GameModeType.Vanilla;
        }
    }
}
