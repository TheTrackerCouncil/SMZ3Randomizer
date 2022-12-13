using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Brinstar
{
    public class BlueBrinstar : SMRegion
    {
        public BlueBrinstar(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            MorphBall = new Location(this, 26, 0x8F86EC, LocationType.Visible,
                name: "Morphing Ball",
                alsoKnownAs: new[] { "Morph Ball (Corridor No. 1)" },
                vanillaItem: ItemType.Morph,
                memoryAddress: 0x3,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            PowerBomb = new Location(this, 27, 0x8F874C, LocationType.Visible,
                name: "Power Bomb (blue Brinstar)",
                alsoKnownAs: new[] { "Power Bomb wall (Corridor No. 1)" },
                vanillaItem: ItemType.PowerBomb,
                access: items => Logic.CanUsePowerBombs(items),
                memoryAddress: 0x3,
                memoryFlag: 0x8,
                metadata: metadata,
                trackerState: trackerState);

            MiddleMissile = new Location(this, 28, 0x8F8798, LocationType.Visible,
                name: "Missile (blue Brinstar middle)",
                vanillaItem: ItemType.Missile,
                access: items => items.CardBrinstarL1 && items.Morph,
                memoryAddress: 0x3,
                memoryFlag: 0x10,
                metadata: metadata,
                trackerState: trackerState);

            Ceiling = new Location(this, 29, 0x8F879E, LocationType.Hidden,
                name: "Energy Tank, Brinstar Ceiling",
                vanillaItem: ItemType.ETank,
                access: items => items.CardBrinstarL1 && (Logic.CanFly(items) || items.HiJump || items.SpeedBooster || items.Ice),
                memoryAddress: 0x3,
                memoryFlag: 0x20,
                metadata: metadata,
                trackerState: trackerState);

            BottomMissile = new Location(this, 34, 0x8F8802, LocationType.Chozo,
                name: "Missile (blue Brinstar bottom)",
                vanillaItem: ItemType.Missile,
                access: items => items.Morph,
                memoryAddress: 0x4,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BlueBrinstarTop = new BlueBrinstarTopRoom(this, metadata, trackerState);

            MemoryRegionId = 1;

            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Blue Brinstar");
        }

        public override string Name => "Blue Brinstar";

        public override string Area => "Brinstar";

        public Location MorphBall { get; }

        public Location PowerBomb { get; }

        public Location MiddleMissile { get; }

        public Location Ceiling { get; }

        public Location BottomMissile { get; }

        public BlueBrinstarTopRoom BlueBrinstarTop { get; }

        public class BlueBrinstarTopRoom : Room
        {
            public BlueBrinstarTopRoom(BlueBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Blue Brinstar Top", metadata, "Billy Mays Room")
            {
                MainItem = new Location(this, 36, 0x8F8836, LocationType.Visible,
                    name: "Main Item",
                    alsoKnownAs: new[] { "Missile (blue Brinstar top)", "Billy Mays Room" },
                    vanillaItem: ItemType.Missile,
                    access: CanEnter,
                    memoryAddress: 0x4,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState);

                HiddenItem = new Location(this, 37, 0x8F883C, LocationType.Hidden,
                    name: "Hidden Item",
                    alsoKnownAs: new[] { "Missile (blue Brinstar behind missile)", "Billy Mays Room - Hidden item" },
                    vanillaItem: ItemType.Missile,
                    access: CanEnter,
                    memoryAddress: 0x4,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location MainItem { get; }

            public Location HiddenItem { get; }

            public bool CanEnter(Progression items)
            {
                // Can get to east side of blue brin
                return items.CardBrinstarL1 && Logic.CanUsePowerBombs(items) &&
                       // Can climb the shaft
                       (Logic.CanWallJump(WallJumpDifficulty.Medium) || items.SpaceJump || items.SpeedBooster) &&
                       // Can either easily jump out of the water or is comfortable doing the door jump
                       (!World.Config.LogicConfig.EasyBlueBrinstarTop || items.Gravity || items.SpaceJump);
            }
        }
    }
}
