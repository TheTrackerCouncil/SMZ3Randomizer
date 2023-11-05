﻿using System;
using System.Collections.Generic;
using Randomizer.Abstractions;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches;

public class GoalsPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        // Mark Ganon invincible flag (if needed)
        yield return new GeneratedPatch(Snes(0x30803E), new byte[] { 0x03 });

        // Open pyramid
        if (data.Config.OpenPyramid)
        {
            yield return new GeneratedPatch(0x40008B, new byte[] { 0x01 });
        }

        // Set number of crystals to enter GT
        var gtCrystals = data.Config.GanonsTowerCrystalCount;
        yield return new GeneratedPatch(0x40005E, new [] { (byte)gtCrystals });
        yield return new GeneratedPatch(0x40008C, gtCrystals == 0 ? new byte[] { 0x01 } : new byte[] { 0x00 });

        // Crystals to damage Ganon
        yield return new GeneratedPatch(0x40005F, new byte[] { (byte)data.Config.GanonCrystalCount});

        // Bosses to enter Tourian
        var numBosses = data.Config.TourianBossCount;
        yield return new GeneratedPatch(Snes(0xF47008), new byte[] { (byte)numBosses, 0x00 });
        yield return new GeneratedPatch(Snes(0xF4700B), numBosses == 0 ? new byte[] { 0x00, 0x04  } : new byte[] { 0x00, 0x01 } );
    }
}
