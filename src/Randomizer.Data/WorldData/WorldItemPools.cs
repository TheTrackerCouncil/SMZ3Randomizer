using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.Options;
using Randomizer.Shared;

namespace Randomizer.Data.WorldData;

/// <summary>
/// Class for creating and housing the various different item pools which will be used to populate a world
/// </summary>
public class WorldItemPools
{
    public WorldItemPools(World world)
    {
        // Create the item pools
        var progression = CreateProgressionPool(world);
        var nice = CreateNicePool(world);
        var dungeon = CreateDungeonPool(world);
        var keycards = CreateKeycards(world);
        var junk = CreateJunkPool(world);

        // Remove starting inventory items
        foreach (var itemType in ItemSettingOptions.GetStartingItemTypes(world.Config))
        {
            if (progression.Any(x => x.Type == itemType))
                progression.Remove(progression.First(x => x.Type == itemType));
            else if (nice.Any(x => x.Type == itemType))
                nice.Remove(nice.First(x => x.Type == itemType));
            else if (junk.Any(x => x.Type == itemType))
                junk.Remove(junk.First(x => x.Type == itemType));
            else if (dungeon.Any(x => x.Type == itemType))
                dungeon.Remove(dungeon.First(x => x.Type == itemType));
            else if (keycards.Any(x => x.Type == itemType))
                keycards.Remove(keycards.First(x => x.Type == itemType));
        }

        // If we're missing any items, fill up the spots with twenty rupees
        var itemCount = progression.Count + nice.Count + dungeon.Count + junk.Count;
        if (world.Config.MetroidKeysanity)
            itemCount += keycards.Count;
        var locationCount = world.Locations.Count();
        if (itemCount < locationCount)
        {
            junk.AddRange(Copies(locationCount - itemCount, () => new Item(ItemType.TwentyRupees, world)));
        }

        Progression = progression;
        Nice = nice;
        Junk = junk;
        Dungeon = dungeon;
        Keycards = keycards;
    }

    public IReadOnlyCollection<Item> Progression { get; set; }
    public IReadOnlyCollection<Item> Nice { get; }
    public IReadOnlyCollection<Item> Junk { get; }
    public IReadOnlyCollection<Item> Dungeon { get; set; }
    public IReadOnlyCollection<Item> Keycards { get; set; }
    public IEnumerable<Item> AllItems => Progression.Concat(Nice).Concat(Junk).Concat(Dungeon).Concat(Keycards);

    /// <summary>
    /// Returns a list of items that are nice to have but are not required
    /// to finish the game.
    /// </summary>
    /// <param name="world">The world to assign to the items.</param>
    /// <returns>A new collection of items.</returns>
    public static List<Item> CreateNicePool(World world)
    {
        var itemPool = new List<Item> {
            new Item(ItemType.ProgressiveTunic, world),
            new Item(ItemType.ProgressiveTunic, world),
            new Item(ItemType.ProgressiveSword, world),
            new Item(ItemType.ProgressiveSword, world),
            new Item(ItemType.SilverArrows, world),
            new Item(ItemType.BlueBoomerang, world),
            new Item(ItemType.RedBoomerang, world),
            new Item(ItemType.Bottle, world),
            new Item(ItemType.Bottle, world),
            new Item(ItemType.Bottle, world),
            new Item(ItemType.Bugnet, world),
            new Item(ItemType.HeartContainerRefill, world),

            new Item(ItemType.Spazer, world),
            new Item(ItemType.XRay, world),
        };

        if (world.Config.CasPatches.QuarterMagic)
        {
            itemPool.Add(new Item(ItemType.HalfMagic, world));
        }

        itemPool.AddRange(Copies(10, () => new Item(ItemType.HeartContainer, world)));

        return itemPool;
    }

    /// <summary>
    /// Returns a list of items used to fill remaining locations.
    /// </summary>
    /// <param name="world">The world to assign to the items.</param>
    /// <returns>A new collection of items.</returns>
    public static List<Item> CreateJunkPool(World world)
    {
        var itemPool = new List<Item> {
            new Item(ItemType.Arrow, world),
            new Item(ItemType.OneHundredRupees, world)
        };

        itemPool.AddRange(Copies(24, () => new Item(ItemType.HeartPiece, world)));
        itemPool.AddRange(Copies(8, () => new Item(ItemType.TenArrows, world)));
        itemPool.AddRange(Copies(13, () => new Item(ItemType.ThreeBombs, world)));
        itemPool.AddRange(Copies(4, () => new Item(ItemType.ArrowUpgrade5, world)));
        itemPool.AddRange(Copies(4, () => new Item(ItemType.BombUpgrade5, world)));
        itemPool.AddRange(Copies(2, () => new Item(ItemType.OneRupee, world)));
        itemPool.AddRange(Copies(4, () => new Item(ItemType.FiveRupees, world)));
        itemPool.AddRange(Copies(7, () => new Item(ItemType.FiftyRupees, world)));
        itemPool.AddRange(Copies(3, () => new Item(ItemType.ThreeHundredRupees, world)));
        itemPool.AddRange(Copies(9, () => new Item(ItemType.ETank, world)));
        itemPool.AddRange(Copies(39, () => new Item(ItemType.Missile, world)));
        itemPool.AddRange(Copies(15, () => new Item(ItemType.Super, world)));
        itemPool.AddRange(Copies(8, () => new Item(ItemType.PowerBomb, world)));

        return itemPool;
    }

    /// <summary>
    /// Returns a list of dungeon-specific items.
    /// </summary>
    /// <remarks>The order of the dungeon pool is significant.</remarks>
    /// <param name="world">The world to assign to the items.</param>
    /// <returns>A new collection of items.</returns>
    public static List<Item> CreateDungeonPool(World world)
    {
        var itemPool = new List<Item>();

        itemPool.AddRange(new[] {
            new Item(ItemType.BigKeyEP, world),
            new Item(ItemType.BigKeyDP, world),
            new Item(ItemType.BigKeyTH, world),
            new Item(ItemType.BigKeyPD, world),
            new Item(ItemType.BigKeySP, world),
            new Item(ItemType.BigKeySW, world),
            new Item(ItemType.BigKeyTT, world),
            new Item(ItemType.BigKeyIP, world),
            new Item(ItemType.BigKeyMM, world),
            new Item(ItemType.BigKeyTR, world),
            new Item(ItemType.BigKeyGT, world),
        });

        itemPool.AddRange(Copies(1, () => new Item(ItemType.KeyHC, world)));
        itemPool.AddRange(Copies(2, () => new Item(ItemType.KeyCT, world)));
        itemPool.AddRange(Copies(1, () => new Item(ItemType.KeyDP, world)));
        itemPool.AddRange(Copies(1, () => new Item(ItemType.KeyTH, world)));
        itemPool.AddRange(Copies(6, () => new Item(ItemType.KeyPD, world)));
        itemPool.AddRange(Copies(1, () => new Item(ItemType.KeySP, world)));
        itemPool.AddRange(Copies(3, () => new Item(ItemType.KeySW, world)));
        itemPool.AddRange(Copies(1, () => new Item(ItemType.KeyTT, world)));
        itemPool.AddRange(Copies(2, () => new Item(ItemType.KeyIP, world)));
        itemPool.AddRange(Copies(3, () => new Item(ItemType.KeyMM, world)));
        itemPool.AddRange(Copies(4, () => new Item(ItemType.KeyTR, world)));
        itemPool.AddRange(Copies(4, () => new Item(ItemType.KeyGT, world)));

        itemPool.AddRange(new[] {
            new Item(ItemType.MapEP, world),
            new Item(ItemType.MapDP, world),
            new Item(ItemType.MapTH, world),
            new Item(ItemType.MapPD, world),
            new Item(ItemType.MapSP, world),
            new Item(ItemType.MapSW, world),
            new Item(ItemType.MapTT, world),
            new Item(ItemType.MapIP, world),
            new Item(ItemType.MapMM, world),
            new Item(ItemType.MapTR, world),
        });
        if (!world.Config.MetroidKeysanity)
        {
            itemPool.AddRange(new[] {
                new Item(ItemType.MapHC, world),
                new Item(ItemType.MapGT, world),
                new Item(ItemType.CompassEP, world),
                new Item(ItemType.CompassDP, world),
                new Item(ItemType.CompassTH, world),
                new Item(ItemType.CompassPD, world),
                new Item(ItemType.CompassSP, world),
                new Item(ItemType.CompassSW, world),
                new Item(ItemType.CompassTT, world),
                new Item(ItemType.CompassIP, world),
                new Item(ItemType.CompassMM, world),
                new Item(ItemType.CompassTR, world),
                new Item(ItemType.CompassGT, world),
            });
        }

        return itemPool;
    }

    /// <summary>
    /// Returns a list of the items required to progress through the game.
    /// </summary>
    /// <param name="world">The world to assign to the items.</param>
    /// <returns>A new collection of items.</returns>
    public static List<Item> CreateProgressionPool(World world)
    {
        var itemPool = new List<Item> {
            new Item(ItemType.ProgressiveShield, world, isProgression: true),
            new Item(ItemType.ProgressiveShield, world, isProgression: true),
            new Item(ItemType.ProgressiveShield, world, isProgression: true),
            new Item(ItemType.ProgressiveSword, world, isProgression: true),
            new Item(ItemType.ProgressiveSword, world, isProgression: true),
            new Item(ItemType.Bow, world, isProgression: true),
            new Item(ItemType.Hookshot, world, isProgression: true),
            new Item(ItemType.Mushroom, world, isProgression: true),
            new Item(ItemType.Powder, world, isProgression: true),
            new Item(ItemType.Firerod, world, isProgression: true),
            new Item(ItemType.Icerod, world, isProgression: true),
            new Item(ItemType.Bombos, world, isProgression: true),
            new Item(ItemType.Ether, world, isProgression: true),
            new Item(ItemType.Quake, world, isProgression: true),
            new Item(ItemType.Lamp, world, isProgression: true),
            new Item(ItemType.Hammer, world, isProgression: true),
            new Item(ItemType.Shovel, world, isProgression: true),
            new Item(ItemType.Flute, world, isProgression: true),
            new Item(ItemType.Book, world, isProgression: true),
            new Item(ItemType.Bottle, world, isProgression: true),
            new Item(ItemType.Somaria, world, isProgression: true),
            new Item(ItemType.Byrna, world, isProgression: true),
            new Item(ItemType.Cape, world, isProgression: true),
            new Item(ItemType.Mirror, world, isProgression: true),
            new Item(ItemType.Boots, world, isProgression: true),
            new Item(ItemType.ProgressiveGlove, world, isProgression: true),
            new Item(ItemType.ProgressiveGlove, world, isProgression: true),
            new Item(ItemType.Flippers, world, isProgression: true),
            new Item(ItemType.MoonPearl, world, isProgression: true),
            new Item(ItemType.HalfMagic, world, isProgression: true),

            new Item(ItemType.Grapple, world, isProgression: true),
            new Item(ItemType.Charge, world, isProgression: true),
            new Item(ItemType.Ice, world, isProgression: true),
            new Item(ItemType.Wave, world, isProgression: true),
            new Item(ItemType.Plasma, world, isProgression: true),
            new Item(ItemType.Varia, world, isProgression: true),
            new Item(ItemType.Gravity, world, isProgression: true),
            new Item(ItemType.Morph, world, isProgression: true),
            new Item(ItemType.Bombs, world, isProgression: true),
            new Item(ItemType.SpringBall, world, isProgression: true),
            new Item(ItemType.ScrewAttack, world, isProgression: true),
            new Item(ItemType.HiJump, world, isProgression: true),
            new Item(ItemType.SpaceJump, world, isProgression: true),
            new Item(ItemType.SpeedBooster, world, isProgression: true),

            new Item(ItemType.Missile, world, isProgression: true),
            new Item(ItemType.Super, world, isProgression: true),
            new Item(ItemType.PowerBomb, world, isProgression: true),
            new Item(ItemType.PowerBomb, world, isProgression: true),
            new Item(ItemType.ETank, world, isProgression: true),
            new Item(ItemType.ETank, world, isProgression: true),
            new Item(ItemType.ETank, world, isProgression: true),
            new Item(ItemType.ETank, world, isProgression: true),
            new Item(ItemType.ETank, world, isProgression: true),

            new Item(ItemType.ReserveTank, world, isProgression: true),
            new Item(ItemType.ReserveTank, world, isProgression: true),
            new Item(ItemType.ReserveTank, world, isProgression: true),
            new Item(ItemType.ReserveTank, world, isProgression: true),

            new Item(ItemType.ThreeHundredRupees, world, isProgression: true),
            new Item(ItemType.ThreeHundredRupees, world, isProgression: true),
        };

        return itemPool;
    }

    /// <summary>
    /// Returns a list of the items required to open doors in Metroid in a
    /// keysanity seed.
    /// </summary>
    /// <param name="world">The world to assign to the items.</param>
    /// <returns>A new collection of items.</returns>
    public static List<Item> CreateKeycards(World world)
    {
        return new List<Item> {
            new Item(ItemType.CardCrateriaL1, world),
            new Item(ItemType.CardCrateriaL2, world),
            new Item(ItemType.CardCrateriaBoss, world),
            new Item(ItemType.CardBrinstarL1, world),
            new Item(ItemType.CardBrinstarL2, world),
            new Item(ItemType.CardBrinstarBoss, world),
            new Item(ItemType.CardNorfairL1, world),
            new Item(ItemType.CardNorfairL2, world),
            new Item(ItemType.CardNorfairBoss, world),
            new Item(ItemType.CardMaridiaL1, world),
            new Item(ItemType.CardMaridiaL2, world),
            new Item(ItemType.CardMaridiaBoss, world),
            new Item(ItemType.CardWreckedShipL1, world),
            new Item(ItemType.CardWreckedShipBoss, world),
            new Item(ItemType.CardLowerNorfairL1, world),
            new Item(ItemType.CardLowerNorfairBoss, world),
        };
    }

    private static List<Item> Copies(int nr, Func<Item> template)
    {
        return Enumerable.Range(1, nr).Select(i => template()).ToList();
    }
}
