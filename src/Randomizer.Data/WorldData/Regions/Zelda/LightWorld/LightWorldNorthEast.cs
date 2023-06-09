using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda.LightWorld
{
    public class LightWorldNorthEast : Z3Region
    {
        public LightWorldNorthEast(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            MushroomItem = new Location(this, LocationId.PotionShop, 0x308014, LocationType.Regular,
                name: "Potion Shop",
                vanillaItem: ItemType.Powder,
                access: items => items.Mushroom,
                memoryAddress: 0x191,
                memoryFlag: 0x20,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            SahasrahlasHideout = new SahasrahlasHideoutRoom(this, metadata, trackerState);
            WaterfallFairy = new WaterfallFairyChamber(this, metadata, trackerState);
            ZorasDomain = new ZorasDomainArea(this, metadata, trackerState);
            StartingRooms = new List<int>() { 15, 21, 22, 23, 27, 29, 30, 37, 45, 46, 47, 129 };
            IsOverworld = true;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Light World North East");
        }

        public override string Name => "Light World North East";

        public override string Area => "Light World";

        public Location MushroomItem { get; }

        public SahasrahlasHideoutRoom SahasrahlasHideout { get; }

        public ZorasDomainArea ZorasDomain { get; }

        public WaterfallFairyChamber WaterfallFairy { get; }

        public class SahasrahlasHideoutRoom : Room
        {
            private const int SphereOne = -10;

            public SahasrahlasHideoutRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Sahasrahla's Hut", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.SahasrahlasHutLeft, 0x1EA82, LocationType.Regular,
                        name: "Left",
                        vanillaItem: ItemType.FiftyRupees,
                        memoryAddress: 0x105,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState)
                        .Weighted(SphereOne),
                    new Location(this, LocationId.SahasrahlasHutMiddle, 0x1EA85, LocationType.Regular,
                        name: "Middle",
                        vanillaItem: ItemType.ThreeBombs,
                        memoryAddress: 0x105,
                        memoryFlag: 0x5,
                        metadata: metadata,
                        trackerState: trackerState)
                        .Weighted(SphereOne),
                    new Location(this, LocationId.SahasrahlasHutRight, 0x1EA88, LocationType.Regular,
                        name: "Right",
                        vanillaItem: ItemType.FiftyRupees,
                        memoryAddress: 0x105,
                        memoryFlag: 0x6,
                        metadata: metadata,
                        trackerState: trackerState)
                        .Weighted(SphereOne),
                    new Location(this, LocationId.Sahasrahla, 0x5F1FC, LocationType.Regular,
                        name: "Sahasrahla",
                        vanillaItem: ItemType.Boots,
                        access: items => items.GreenPendant,
                        relevanceRequirement: items => World.CanAquire(items, RewardType.PendantGreen),
                        memoryAddress: 0x190,
                        memoryFlag: 0x10,
                        memoryType: LocationMemoryType.ZeldaMisc,
                        trackerLogic: items => region.CountReward(items, RewardType.PendantGreen) == 1,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class WaterfallFairyChamber : Room
        {
            public WaterfallFairyChamber(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Waterfall Fairy", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.WaterfallFairyLeft, 0x1E9B0, LocationType.Regular,
                        name: "Left",
                        access: items => items.Flippers || Logic.CanHyruleSouthFakeFlippers(items, true),
                        memoryAddress: 0x114,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.WaterfallFairyRight, 0x1E9D1, LocationType.Regular,
                        name: "Right",
                        access: items => items.Flippers || Logic.CanHyruleSouthFakeFlippers(items, true),
                        memoryAddress: 0x114,
                        memoryFlag: 0x5,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class ZorasDomainArea : Room
        {
            public ZorasDomainArea(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Zora's Domain", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.KingZora, 0x1DE1C3, LocationType.Regular,
                        name: "King Zora",
                        vanillaItem: ItemType.Flippers,
                        access: items => (Logic.CanLiftLight(items) || items.Flippers) && (!Config.LogicConfig.ZoraNeedsRupeeItems || items.Rupees >= 500),
                        memoryAddress: 0x190,
                        memoryFlag: 0x2,
                        memoryType: LocationMemoryType.ZeldaMisc,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.ZorasLedge, 0x308149, LocationType.Regular,
                        name: "Zora's Ledge",
                        vanillaItem: ItemType.HeartPiece,
                        access: items => items.Flippers,
                        memoryAddress: 0x81,
                        memoryFlag: 0x40,
                        memoryType: LocationMemoryType.ZeldaMisc,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }
    }
}
