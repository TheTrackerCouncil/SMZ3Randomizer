using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Shared;

namespace Randomizer.SMZ3.FileData.Patches;

public class DungeonMusicPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        if (data.World.Config.ZeldaKeysanity)
        {
            return new List<GeneratedPatch>();
        };

        var regions = data.World.Regions.OfType<IHasReward>().Where(x => x.RewardType != RewardType.Agahnim);
        var music = regions.Select(x => (byte)(x.RewardType switch {
            RewardType.PendantBlue => 0x11,
            RewardType.PendantGreen => 0x11,
            RewardType.PendantRed => 0x11,
            _ => 0x16
        }));

        return MusicPatches(regions, music);
    }

    private IEnumerable<GeneratedPatch> MusicPatches(IEnumerable<IHasReward> regions, IEnumerable<byte> music)
    {
        var addresses = regions.Select(MusicAddresses);
        var associations = addresses.Zip(music, (a, b) => (a, b));
        return associations.SelectMany(x => x.a.Select(i => new GeneratedPatch(Snes(i), new byte[] { x.b })));
    }

    private int[] MusicAddresses(IHasReward region)
    {
        return region switch
        {
            EasternPalace _ => new[] { 0x2D59A },
            DesertPalace _ => new[] { 0x2D59B, 0x2D59C, 0x2D59D, 0x2D59E },
            TowerOfHera _ => new[] { 0x2D5C5, 0x2907A, 0x28B8C },
            PalaceOfDarkness _ => new[] { 0x2D5B8 },
            SwampPalace _ => new[] { 0x2D5B7 },
            SkullWoods _ => new[] { 0x2D5BA, 0x2D5BB, 0x2D5BC, 0x2D5BD, 0x2D608, 0x2D609, 0x2D60A, 0x2D60B },
            ThievesTown _ => new[] { 0x2D5C6 },
            IcePalace _ => new[] { 0x2D5BF },
            MiseryMire _ => new[] { 0x2D5B9 },
            TurtleRock _ => new[] { 0x2D5C7, 0x2D5A7, 0x2D5AA, 0x2D5AB },
            var x => throw new InvalidOperationException($"Region {x} should not be a dungeon music region"),
        };
    }
}
