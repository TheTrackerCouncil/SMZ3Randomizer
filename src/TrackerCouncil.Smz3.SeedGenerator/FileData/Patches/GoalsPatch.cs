using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[SkipForParsedRoms]
public class GoalsPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        // Mark Ganon invincible flag (if needed)
        yield return new GeneratedPatch(Snes(0x30803E), new byte[] { 0x05 });

        // Open pyramid
        if (data.Config.OpenPyramid)
        {
            yield return new GeneratedPatch(0x40008B, new byte[] { 0x01 });
        }

        // Set number of crystals to enter GT
        var gtCrystals = data.Config.GanonsTowerCrystalCount;
        yield return new GeneratedPatch(0x40005E, [(byte)gtCrystals]);
        yield return new GeneratedPatch(0x40008C, gtCrystals == 0 ? [0x01] : [0x00]);

        // Crystals to damage Ganon
        yield return new GeneratedPatch(0x40005F, [(byte)data.Config.GanonCrystalCount]);

        // Bosses to enter Tourian
        var numBosses = data.Config.TourianBossCount;
        yield return new GeneratedPatch(Snes(0xF47008), [(byte)numBosses, 0x00]);
        yield return new GeneratedPatch(Snes(0xF4700B), numBosses == 0 ? [0x00, 0x04] : [0x00, 0x01]);

        yield return new GeneratedPatch(Snes(0xF4700E), UshortBytes(0x0001));
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
}
