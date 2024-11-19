using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[SkipForParsedRoms]
public class MedallionPatch : RomPatch
{
    private static readonly int[] s_turtleRockAddresses = { Snes(0x308023), Snes(0xD020), Snes(0xD0FF), Snes(0xD1DE) };
    private static readonly int[] s_miseryMireAddresses = { Snes(0x308022), Snes(0xCFF2), Snes(0xD0D1), Snes(0xD1B0) };

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var turtleRockValues = data.World.TurtleRock.PrerequisiteState.RequiredItem switch
        {
            ItemType.Bombos => new byte[] { 0x00, 0x51, 0x10, 0x00 },
            ItemType.Ether => new byte[] { 0x01, 0x51, 0x18, 0x00 },
            ItemType.Quake => new byte[] { 0x02, 0x14, 0xEF, 0xC4 },
            var x => throw new InvalidOperationException($"Tried using {x} in place of Turtle Rock medallion")
        };

        var miseryMireValues = data.World.MiseryMire.PrerequisiteState.RequiredItem switch
        {
            ItemType.Bombos => new byte[] { 0x00, 0x51, 0x00, 0x00 },
            ItemType.Ether => new byte[] { 0x01, 0x13, 0x9F, 0xF1 },
            ItemType.Quake => new byte[] { 0x02, 0x51, 0x08, 0x00 },
            var x => throw new InvalidOperationException($"Tried using {x} in place of Misery Mire medallion")
        };

        var patches = new List<GeneratedPatch>();
        patches.AddRange(s_turtleRockAddresses.Zip(turtleRockValues, (i, b) => new GeneratedPatch(i, [b])));
        patches.AddRange(s_miseryMireAddresses.Zip(miseryMireValues, (i, b) => new GeneratedPatch(i, [b])));
        return patches;
    }

    public static List<ParsedRomPrerequisiteDetails> GetPrerequisitesFromRom(byte[] rom, IEnumerable<IHasPrerequisite> examplePrerequisiteRegions)
    {
        var types = new[] { ItemType.Bombos, ItemType.Ether, ItemType.Quake };
        var mmMedallion = types[rom.Skip(s_miseryMireAddresses[0]).First()];
        var trMedallion = types[rom.Skip(s_turtleRockAddresses[0]).First()];

        // ReSharper disable PossibleMultipleEnumeration
        var miseryMireRegion = examplePrerequisiteRegions.First(x => x is MiseryMire);
        var turtleRockRegion = examplePrerequisiteRegions.First(x => x is TurtleRock);
        // ReSharper restore PossibleMultipleEnumeration

        var toReturn = new List<ParsedRomPrerequisiteDetails>
        {
            new()
            {
                RegionType = miseryMireRegion.GetType(),
                RegionName = miseryMireRegion.Name,
                PrerequisiteItem = mmMedallion,
            },
            new()
            {
                RegionType = turtleRockRegion.GetType(),
                RegionName = turtleRockRegion.Name,
                PrerequisiteItem = trMedallion,
            }
        };

        return toReturn;
    }
}
