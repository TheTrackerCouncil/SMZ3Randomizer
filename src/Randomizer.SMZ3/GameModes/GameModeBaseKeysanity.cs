using System.Collections.Generic;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3.FileData.Patches;

namespace Randomizer.SMZ3.GameModes;

public class GameModeBaseKeysanity : GameModeBase
{
    public override GameModeType GameModeType => GameModeType.Keysanity;
    public override string Name => "Keysanity";
    public override string Description => "Keysanity";

    public override void ModifyWorldItemPools(WorldItemPools itemPool)
    {
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.MapHC));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.MapGT));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassEP));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassDP));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassTH));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassPD));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassSP));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassSW));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassTT));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassIP));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassMM));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassTR));
        itemPool.Dungeon.Remove(itemPool.Dungeon.First(x => x.Type == ItemType.CompassGT));
    }

    public override ICollection<RomPatch> GetPatches(World world)
    {
        return world.Config.GameModeConfigs.KeysanityConfig.KeysanityMode == KeysanityMode.None
            ? base.GetPatches(world)
            : new List<RomPatch> { new MetroidKeysanityPatch(), new ZeldaKeysanityPatch() };
    }

    public override void ItemTracked(Item item, Location? location, TrackerBase? _tracker)
    {
        if (!item.Type.IsInCategory(ItemCategory.Map))
            return;

        var world = item.World;

        IDungeon? dungeon = item.Type switch
        {
            ItemType.MapEP => world.EasternPalace,
            ItemType.MapTH => world.TowerOfHera,
            ItemType.MapDP => world.DesertPalace,
            ItemType.MapPD => world.PalaceOfDarkness,
            ItemType.MapSP => world.SwampPalace,
            ItemType.MapTT => world.ThievesTown,
            ItemType.MapSW => world.SkullWoods,
            ItemType.MapIP => world.IcePalace,
            ItemType.MapMM => world.MiseryMire,
            ItemType.MapTR => world.TurtleRock,
            _ => null
        };

        if (dungeon == null || dungeon.MarkedReward == dungeon.DungeonRewardType)
            return;

        _tracker?.SetDungeonReward(dungeon, dungeon.DungeonRewardType, null, true);
    }
}
