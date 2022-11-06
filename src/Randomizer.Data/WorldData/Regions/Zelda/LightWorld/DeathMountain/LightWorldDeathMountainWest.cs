using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain
{
    public class LightWorldDeathMountainWest : Z3Region
    {
        public LightWorldDeathMountainWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            EtherTablet = new Location(this, 256 + 0, 0x308016, LocationType.Ether,
                name: "Ether Tablet",
                vanillaItem: ItemType.Ether,
                access: items => items.Book && items.MasterSword && (items.Mirror || (items.Hammer && items.Hookshot)),
                memoryAddress: 0x191,
                memoryFlag: 0x1,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            SpectacleRock = new Location(this, 256 + 1, 0x308140, LocationType.Regular,
                name: "Spectacle Rock",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Mirror,
                memoryAddress: 0x3,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            SpectacleRockCave = new Location(this, 256 + 2, 0x308002, LocationType.Regular,
                name: "Spectacle Rock Cave",
                vanillaItem: ItemType.HeartPiece,
                memoryAddress: 0xEA,
                memoryFlag: 0xA,
                metadata: metadata,
                trackerState: trackerState);

            OldManReward = new Location(this, 256 + 3, 0x1EE9FA, LocationType.Regular,
                name: "Old Man",
                vanillaItem: ItemType.Mirror,
                access: items => Logic.CanPassSwordOnlyDarkRooms(items),
                memoryAddress: 0x190,
                memoryFlag: 0x1,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            StartingRooms = new List<int>() { 3 };
            IsOverworld = true;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Light World Death Mountain East");
        }

        public override string Name => "Light World Death Mountain West";

        public override string Area => "Death Mountain";

        public Location EtherTablet { get; }

        public Location SpectacleRock { get; }

        public Location SpectacleRockCave { get; }

        public Location OldManReward { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.Flute
                || (Logic.CanLiftLight(items) && Logic.CanPassSwordOnlyDarkRooms(items))
                || Logic.CanAccessDeathMountainPortal(items);
        }
    }
}
