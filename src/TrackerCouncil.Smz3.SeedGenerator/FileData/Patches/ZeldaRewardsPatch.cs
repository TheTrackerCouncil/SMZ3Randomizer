using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Brinstar;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Maridia;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Norfair;
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

    public static void ApplyRewardsFromRom(byte[] rom, World world)
    {
        var regions = world.Regions.Where(x => x is (IHasBoss or IHasReward) and not CastleTower);
        var rewardValues = new Dictionary<string, RewardType>()
        {
            { string.Join(",", PendantValues(1)), RewardType.PendantGreen },
            { string.Join(",", PendantValues(2)), RewardType.PendantRed },
            { string.Join(",", PendantValues(3)), RewardType.PendantBlue },
            { string.Join(",", CrystalValues(1)), RewardType.CrystalBlue },
            { string.Join(",", CrystalValues(2)), RewardType.CrystalBlue },
            { string.Join(",", CrystalValues(3)), RewardType.CrystalBlue },
            { string.Join(",", CrystalValues(4)), RewardType.CrystalBlue },
            { string.Join(",", CrystalValues(5)), RewardType.CrystalRed },
            { string.Join(",", CrystalValues(6)), RewardType.CrystalRed },
            { string.Join(",", CrystalValues(7)), RewardType.CrystalBlue },
            { string.Join(",", BossTokenValues(1)), RewardType.MetroidBoss },
            { string.Join(",", BossTokenValues(2)), RewardType.MetroidBoss },
            { string.Join(",", BossTokenValues(3)), RewardType.MetroidBoss },
            { string.Join(",", BossTokenValues(4)), RewardType.MetroidBoss },
        };

        var regionRewards = new Dictionary<Region, RewardType>();

        foreach (var region in regions)
        {
            try
            {
                var addresses = string.Join(",", RewardAddresses(region).Select(x => rom[Snes(x)]));
                var reward = rewardValues[addresses];
                regionRewards[region] = reward;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        // var a = "1";
    }

    private IEnumerable<GeneratedPatch> RewardPatches(IEnumerable<IHasReward> regions, IEnumerable<int> rewards, Func<int, byte[]> rewardValues)
    {
        var addresses = regions.Select(x => RewardAddresses((Region)x));
        var values = rewards.Select(rewardValues);
        var associations = addresses.Zip(values, (a, v) => (a, v));
        return associations.SelectMany(x => x.a.Zip(x.v, (i, b) => new GeneratedPatch(Snes(i), [b])));
    }

    private static int[] RewardAddresses(Region region)
    {
        return region switch
        {
            EasternPalace => [0x2A09D, 0xABEF8, 0xABEF9, 0x308052, 0x30807C, 0x1C6FE],
            DesertPalace => [0x2A09E, 0xABF1C, 0xABF1D, 0x308053, 0x308078, 0x1C6FF],
            TowerOfHera => [0x2A0A5, 0xABF0A, 0xABF0B, 0x30805A, 0x30807A, 0x1C706],
            PalaceOfDarkness => [0x2A0A1, 0xABF00, 0xABF01, 0x308056, 0x30807D, 0x1C702],
            SwampPalace => [0x2A0A0, 0xABF6C, 0xABF6D, 0x308055, 0x308071, 0x1C701],
            SkullWoods => [0x2A0A3, 0xABF12, 0xABF13, 0x308058, 0x30807B, 0x1C704],
            ThievesTown => [0x2A0A6, 0xABF36, 0xABF37, 0x30805B, 0x308077, 0x1C707],
            IcePalace => [0x2A0A4, 0xABF5A, 0xABF5B, 0x308059, 0x308073, 0x1C705],
            MiseryMire => [0x2A0A2, 0xABF48, 0xABF49, 0x308057, 0x308075, 0x1C703],
            TurtleRock => [0x2A0A7, 0xABF24, 0xABF25, 0x30805C, 0x308079, 0x1C708],
            KraidsLair => [0xF26002, 0xF26004, 0xF26005, 0xF26000, 0xF26006, 0xF26007],
            WreckedShip => [0xF2600A, 0xF2600C, 0xF2600D, 0xF26008, 0xF2600E, 0xF2600F],
            InnerMaridia => [0xF26012, 0xF26014, 0xF26015, 0xF26010, 0xF26016, 0xF26017],
            LowerNorfairEast => [0xF2601A, 0xF2601C, 0xF2601D, 0xF26018, 0xF2601E, 0xF2601F],
            _ => throw new InvalidOperationException($"Region {region} should not be a dungeon reward region")
        };
    }

    private static byte[] CrystalValues(int crystal)
    {
        return crystal switch
        {
            1 => [0x02, 0x34, 0x64, 0x40, 0x7F, 0x06],
            2 => [0x10, 0x34, 0x64, 0x40, 0x79, 0x06],
            3 => [0x40, 0x34, 0x64, 0x40, 0x6C, 0x06],
            4 => [0x20, 0x34, 0x64, 0x40, 0x6D, 0x06],
            5 => [0x04, 0x32, 0x64, 0x40, 0x6E, 0x06],
            6 => [0x01, 0x32, 0x64, 0x40, 0x6F, 0x06],
            7 => [0x08, 0x34, 0x64, 0x40, 0x7C, 0x06],
            _ => throw new InvalidOperationException($"Tried using {crystal} as a crystal number")
        };
    }

    private static byte[] PendantValues(int pendant)
    {
        return pendant switch
        {
            1 => [0x04, 0x38, 0x62, 0x00, 0x69, 0x01],
            2 => [0x01, 0x32, 0x60, 0x00, 0x69, 0x03],
            3 => [0x02, 0x34, 0x60, 0x00, 0x69, 0x02],
            _ => throw new InvalidOperationException($"Tried using {pendant} as a pendant number")
        };
    }

    private static byte[] BossTokenValues(int pendant)
    {
        return pendant switch
        {
            1 => [0x01, 0x38, 0x40, 0x80, 0x69, 0x80],
            2 => [0x02, 0x34, 0x42, 0x80, 0x69, 0x81],
            3 => [0x04, 0x34, 0x44, 0x80, 0x69, 0x82],
            4 => [0x08, 0x32, 0x46, 0x80, 0x69, 0x83],
            _ => throw new InvalidOperationException($"Tried using {pendant} as a pendant number")
        };
    }
}
