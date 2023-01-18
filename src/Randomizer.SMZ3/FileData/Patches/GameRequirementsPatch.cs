using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches;

public class GameRequirementsPatch : RomPatch
{
    public override IEnumerable<(int offset, byte[] data)> GetChanges(Config config)
    {
        // Open pyramid
        yield return (0x40008B, new byte[] { 0x01 });

        // Set number of crystals to enter GT
        var gtCrystals = 0;
        yield return (0x40005E, new [] { (byte)gtCrystals });
        yield return (0x40008C, gtCrystals == 0 ? new byte[] { 0x01 } : new byte[] { 0x00 });

        // Crystals to damage Ganon
        yield return (0x40005F, new byte[] { 0x00 });

        // Bosses to enter Tourian
        var numBosses = 1;
        yield return (Snes(0xF47008), new byte[] { (byte)numBosses, 0x00 });
        yield return (Snes(0xF4700B), numBosses == 0 ? new byte[] { 0x00, 0x04  } : new byte[] { 0x01, 0x00 } );
    }
}
