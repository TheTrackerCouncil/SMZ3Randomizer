using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;

namespace Randomizer.SMZ3.FileData.Patches;

public class MedallionPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var turtleRockAddresses = new[] { 0x308023, 0xD020, 0xD0FF, 0xD1DE };
        var miseryMireAddresses = new[] { 0x308022, 0xCFF2, 0xD0D1, 0xD1B0 };

        var turtleRockValues = data.World.TurtleRock.Medallion switch
        {
            ItemType.Bombos => new byte[] { 0x00, 0x51, 0x10, 0x00 },
            ItemType.Ether => new byte[] { 0x01, 0x51, 0x18, 0x00 },
            ItemType.Quake => new byte[] { 0x02, 0x14, 0xEF, 0xC4 },
            var x => throw new InvalidOperationException($"Tried using {x} in place of Turtle Rock medallion")
        };

        var miseryMireValues = data.World.MiseryMire.Medallion switch
        {
            ItemType.Bombos => new byte[] { 0x00, 0x51, 0x00, 0x00 },
            ItemType.Ether => new byte[] { 0x01, 0x13, 0x9F, 0xF1 },
            ItemType.Quake => new byte[] { 0x02, 0x51, 0x08, 0x00 },
            var x => throw new InvalidOperationException($"Tried using {x} in place of Misery Mire medallion")
        };

        var patches = new List<GeneratedPatch>();
        patches.AddRange(turtleRockAddresses.Zip(turtleRockValues, (i, b) => new GeneratedPatch(Snes(i), new[] { b })));
        patches.AddRange(miseryMireAddresses.Zip(miseryMireValues, (i, b) => new GeneratedPatch(Snes(i), new[] { b })));
        return patches;
    }
}
