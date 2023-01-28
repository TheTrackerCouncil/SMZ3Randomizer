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
            MainStreetCeiling = new Location(this, 136, 0x8FC437, LocationType.Visible,
                name: "Missile (green Maridia shinespark)",
                alsoKnownAs: new[] { "Main Street - Ceiling (Shinespark)", "Main Street Missiles" },
                vanillaItem: ItemType.Missile,
                access: items => items.SpeedBooster,
                memoryAddress: 0x11,
                memoryFlag: 0x1,
                metadata: metadata,
                trackerState: trackerState);
            MainStreetCrabSupers = new Location(this, 137, 0x8FC43D, LocationType.Visible,
                name: "Super Missile (green Maridia)",
                alsoKnownAs: new[] { "Main Street - Crab Supers" },
                vanillaItem: ItemType.Super,
                access: items => Logic.CanWallJump(WallJumpDifficulty.Medium)
                              || (Logic.CanWallJump(WallJumpDifficulty.Easy) && items.Ice)
                              || items.HiJump || Logic.CanFly(items),
                memoryAddress: 0x11,
                memoryFlag: 0x2,
                metadata: metadata,
                trackerState: trackerState);
            MamaTurtleRoom = new Location(this, 138, 0x8FC47D, LocationType.Visible,
                name: "Energy Tank, Mama turtle",
                alsoKnownAs: new[] { "Mama Turtle Room" },
                vanillaItem: ItemType.ETank,
                access: items => CanReachTurtleRoom(items)
                              && (Logic.CanFly(items)
                                  || items.SpeedBooster
                                  || items.Grapple), // Reaching the item
                memoryAddress: 0x11,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);
            MamaTurtleWallItem = new Location(this, 139, 0x8FC483, LocationType.Hidden,
                name: "Missile (green Maridia tatori)",
                alsoKnownAs: new[] { "Mama Turtle Room - Wall item" },
                vanillaItem: ItemType.Missile,
                access: items => CanReachTurtleRoom(items)
                              && (Logic.CanWallJump(WallJumpDifficulty.Easy)
                                  || items.SpeedBooster
                                  || (items.Grapple && items.HiJump)
                                  || Logic.CanFly(items)), // Reaching the item
                memoryAddress: 0x11,
                memoryFlag: 0x8,
                metadata: metadata,
                trackerState: trackerState);
            MemoryRegionId = 4;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Outer Maridia");
        }

        public override string Name => "Outer Maridia";

        public override string Area => "Maridia";

        public Location MainStreetCeiling { get; }

        public Location MainStreetCrabSupers { get; }

        public Location MamaTurtleRoom { get; }

        public Location MamaTurtleWallItem { get; }

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
    }

}
