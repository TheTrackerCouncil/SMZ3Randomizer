using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Maridia
{
    public class OuterMaridia : SMRegion
    {
        public OuterMaridia(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            MainStreet = new MainStreetRoom(this, metadata, trackerState);
            MamaTurtle = new MamaTurtleRoom(this, metadata, trackerState);
            MemoryRegionId = 4;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Outer Maridia");
        }

        public override string Name => "Outer Maridia";

        public override string Area => "Maridia";

        public MainStreetRoom MainStreet { get; }

        public MamaTurtleRoom MamaTurtle { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.Gravity
                && ((World.UpperNorfairWest.CanEnter(items, requireRewards) && Logic.CanUsePowerBombs(items))
                    || (Logic.CanAccessMaridiaPortal(items, requireRewards)
                    && items.CardMaridiaL1
                    && items.CardMaridiaL2
                    && (Logic.CanPassBombPassages(items) || items.ScrewAttack)));
        }

        public bool CanReachTurtleRoom(Progression items) => Logic.CanOpenRedDoors(items)
            && (Logic.CanWallJump(WallJumpDifficulty.Medium)
                || (Logic.CanWallJump(WallJumpDifficulty.Easy) && (items.Plasma || items.ScrewAttack))
                || items.HiJump
                || Logic.CanFly(items));

        public class MainStreetRoom : Room
        {
            public MainStreetRoom(OuterMaridia region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Main Street Room", metadata, "Main Street")
            {
                MainStreetCeiling = new Location(this, LocationId.MainStreetBottom, 0x8FC437, LocationType.Visible,
                    name: "Missile (green Maridia shinespark)",
                    vanillaItem: ItemType.Missile,
                    access: items => items.SpeedBooster,
                    memoryAddress: 0x11,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState);
                MainStreetCrabSupers = new Location(this, LocationId.MainStreetTop, 0x8FC43D, LocationType.Visible,
                    name: "Super Missile (green Maridia)",
                    vanillaItem: ItemType.Super,
                    access: items => Logic.CanWallJump(WallJumpDifficulty.Medium)
                                  || (Logic.CanWallJump(WallJumpDifficulty.Easy) && items.Ice)
                                  || items.HiJump || Logic.CanFly(items),
                    memoryAddress: 0x11,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location MainStreetCeiling { get; }

            public Location MainStreetCrabSupers { get; }
        }

        public class MamaTurtleRoom : Room
        {
            public MamaTurtleRoom(OuterMaridia region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Mama Turtle Room", metadata)
            {
                EnergyTank = new Location(this, LocationId.MamaTurtleVisible, 0x8FC47D, LocationType.Visible,
                    name: "Energy Tank, Mama turtle",
                    vanillaItem: ItemType.ETank,
                    access: items => region.CanReachTurtleRoom(items)
                                  && (Logic.CanFly(items)
                                      || items.SpeedBooster
                                      || items.Grapple), // Reaching the item
                    memoryAddress: 0x11,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
                MamaTurtleWallItem = new Location(this, LocationId.MamaTurtleHidden, 0x8FC483, LocationType.Hidden,
                    name: "Missile (green Maridia tatori)",
                    vanillaItem: ItemType.Missile,
                    access: items => region.CanReachTurtleRoom(items)
                                  && (Logic.CanWallJump(WallJumpDifficulty.Easy)
                                      || items.SpeedBooster
                                      || (items.Grapple && items.HiJump)
                                      || Logic.CanFly(items)), // Reaching the item
                    memoryAddress: 0x11,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location EnergyTank { get; }

            public Location MamaTurtleWallItem { get; }
        }
    }
}
