using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[Order(-10)]
public class ZeldaRewardsPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var crystalsBlue = new[] { 1, 2, 3, 4, 7 }.Shuffle(data.Random);
        var crystalsRed = new[] { 5, 6 }.Shuffle(data.Random);
        var crystalRewards = crystalsBlue.Concat(crystalsRed);

        var pendantRewards = new[] { 1, 2, 3 };

        var regions = data.World.Regions.OfType<IHasReward>().ToList();
        var crystalRegions = regions.Where(x => x.RewardType == RewardType.CrystalBlue).Concat(regions.Where(x => x.RewardType == RewardType.CrystalRed));
        var pendantRegions = regions.Where(x => x.RewardType is RewardType.PendantGreen or RewardType.PendantRed or RewardType.PendantBlue).OrderBy(r => (int)r.RewardType);

        foreach (var patch in RewardPatches(crystalRegions, crystalRewards, CrystalValues))
        {
            yield return patch;
        }

        foreach (var patch in RewardPatches(pendantRegions, pendantRewards, PendantValues))
        {
            yield return patch;
        }
    }

    private IEnumerable<GeneratedPatch> RewardPatches(IEnumerable<IHasReward> regions, IEnumerable<int> rewards, Func<int, byte[]> rewardValues)
    {
        var addresses = regions.Select(RewardAddresses);
        var values = rewards.Select(rewardValues);
        var associations = addresses.Zip(values, (a, v) => (a, v));
        return associations.SelectMany(x => x.a.Zip(x.v, (i, b) => new GeneratedPatch(Snes(i), new[] { b })));
    }

    private static int[] RewardAddresses(IHasReward region)
    {
        return region switch
        {
            EasternPalace _ => new[] { 0x2A09D, 0xABEF8, 0xABEF9, 0x308052, 0x30807C, 0x1C6FE },
            DesertPalace _ => new[] { 0x2A09E, 0xABF1C, 0xABF1D, 0x308053, 0x308078, 0x1C6FF },
            TowerOfHera _ => new[] { 0x2A0A5, 0xABF0A, 0xABF0B, 0x30805A, 0x30807A, 0x1C706 },
            PalaceOfDarkness _ => new[] { 0x2A0A1, 0xABF00, 0xABF01, 0x308056, 0x30807D, 0x1C702 },
            SwampPalace _ => new[] { 0x2A0A0, 0xABF6C, 0xABF6D, 0x308055, 0x308071, 0x1C701 },
            SkullWoods _ => new[] { 0x2A0A3, 0xABF12, 0xABF13, 0x308058, 0x30807B, 0x1C704 },
            ThievesTown _ => new[] { 0x2A0A6, 0xABF36, 0xABF37, 0x30805B, 0x308077, 0x1C707 },
            IcePalace _ => new[] { 0x2A0A4, 0xABF5A, 0xABF5B, 0x308059, 0x308073, 0x1C705 },
            MiseryMire _ => new[] { 0x2A0A2, 0xABF48, 0xABF49, 0x308057, 0x308075, 0x1C703 },
            TurtleRock _ => new[] { 0x2A0A7, 0xABF24, 0xABF25, 0x30805C, 0x308079, 0x1C708 },
            var x => throw new InvalidOperationException($"Region {x} should not be a dungeon reward region")
        };
    }

    private static byte[] CrystalValues(int crystal)
    {
        return crystal switch
        {
            1 => new byte[] { 0x02, 0x34, 0x64, 0x40, 0x7F, 0x06 },
            2 => new byte[] { 0x10, 0x34, 0x64, 0x40, 0x79, 0x06 },
            3 => new byte[] { 0x40, 0x34, 0x64, 0x40, 0x6C, 0x06 },
            4 => new byte[] { 0x20, 0x34, 0x64, 0x40, 0x6D, 0x06 },
            5 => new byte[] { 0x04, 0x32, 0x64, 0x40, 0x6E, 0x06 },
            6 => new byte[] { 0x01, 0x32, 0x64, 0x40, 0x6F, 0x06 },
            7 => new byte[] { 0x08, 0x34, 0x64, 0x40, 0x7C, 0x06 },
            var x => throw new InvalidOperationException($"Tried using {x} as a crystal number")
        };
    }

    private static byte[] PendantValues(int pendant)
    {
        return pendant switch
        {
            1 => new byte[] { 0x04, 0x38, 0x62, 0x00, 0x69, 0x01 },
            2 => new byte[] { 0x01, 0x32, 0x60, 0x00, 0x69, 0x03 },
            3 => new byte[] { 0x02, 0x34, 0x60, 0x00, 0x69, 0x02 },
            var x => throw new InvalidOperationException($"Tried using {x} as a pendant number")
        };
    }
}
