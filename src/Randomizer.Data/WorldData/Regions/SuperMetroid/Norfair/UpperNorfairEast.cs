using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using System.Collections.Generic;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair
{
    public class UpperNorfairEast : SMRegion
    {
        public UpperNorfairEast(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            BubbleMountainHiddenHall = new BubbleMountainHiddenHallRoom(this, metadata, trackerState);
            GreenBubblesMissile = new GreenBubblesMissileRoom(this, metadata, trackerState);
            BubbleMountain = new BubbleMountainRoom(this, metadata, trackerState);
            SpeedBoosterHall = new SpeedBoosterHallRoom(this, metadata, trackerState);
            SpeedBooster = new SpeedBoosterRoom(this, metadata, trackerState);
            DoubleChamber = new DoubleChamberRoom(this, metadata, trackerState);
            WaveBeam = new WaveBeamRoom(this, metadata, trackerState);
            MemoryRegionId = 2;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Upper Norfair East");
        }

        public override string Name => "Upper Norfair, East";

        public override string Area => "Upper Norfair";

        public BubbleMountainHiddenHallRoom BubbleMountainHiddenHall { get; }

        public GreenBubblesMissileRoom GreenBubblesMissile { get; }

        public BubbleMountainRoom BubbleMountain { get; }

        public SpeedBoosterHallRoom SpeedBoosterHall { get; }

        public SpeedBoosterRoom SpeedBooster { get; }

        public DoubleChamberRoom DoubleChamber { get; }

        public WaveBeamRoom WaveBeam { get; }

        // Todo: Super is not actually needed for Frog Speedway, but changing this will affect locations
        // Todo: Ice Beam -> Croc Speedway is not considered
        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return (
                        ((Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph) ||
                        Logic.CanAccessNorfairUpperPortal(items)
                    ) && items.Varia && items.Super && (
                        /* Cathedral */
                        (Logic.CanOpenRedDoors(items) && (Config.MetroidKeysanity ? items.CardNorfairL2 : items.Super) &&
                            (Logic.CanFly(items) || items.HiJump || items.SpeedBooster)) ||
                        /* Frog Speedway */
                        (items.SpeedBooster && (items.CardNorfairL2 || items.Wave) && Logic.CanUsePowerBombs(items))
                    );
        }

        private bool CanReachBubbleMountainLeftSide(Progression items)
            => Logic.CanFly(items)
            || (items.Grapple && items.Morph && (items.SpeedBooster || Logic.CanPassBombPassages(items)))
            || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy))
            || (items.Ice && items.HiJump)
            || Logic.CanWallJump(WallJumpDifficulty.Insane)
            || (Logic.CanWallJump(WallJumpDifficulty.Hard) && items.Grapple);

        private bool CanReachBubbleMountainRightSide(Progression items)
            => Logic.CanFly(items)
            || (items.Morph && (items.SpeedBooster || Logic.CanPassBombPassages(items)))
            || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy))
            || (items.Ice && items.HiJump)
            || Logic.CanWallJump(WallJumpDifficulty.Hard);

        public class BubbleMountainHiddenHallRoom : Room
        {
            public BubbleMountainHiddenHallRoom(UpperNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Bubble Mountain Hidden Hall", metadata, "Norfair Reserve Tank")
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.UpperNorfairReserveTankChozo, 0x8F8C3E, LocationType.Chozo,
                        name: "Main Item",
                        vanillaItem: ItemType.ReserveTank,
                        access: items => items.CardNorfairL2 && items.Morph && region.CanReachBubbleMountainLeftSide(items),
                        memoryAddress: 0x7,
                        memoryFlag: 0x20,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.UpperNorfairReserveTankHidden, 0x8F8C44, LocationType.Hidden,
                        name: "Hidden Item",
                        vanillaItem: ItemType.Missile,
                        access: items => items.CardNorfairL2 && items.Morph && region.CanReachBubbleMountainLeftSide(items),
                        memoryAddress: 0x7,
                        memoryFlag: 0x40,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class GreenBubblesMissileRoom : Room
        {
            public GreenBubblesMissileRoom(UpperNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Green Bubbles Missile Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.UpperNorfairGreenBubblesMissile, 0x8F8C52, LocationType.Visible,
                        name: "Missile (bubble Norfair green door)",
                        vanillaItem: ItemType.Missile,
                        access: items => items.CardNorfairL2 && region.CanReachBubbleMountainLeftSide(items),
                        memoryAddress: 0x7,
                        memoryFlag: 0x80,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class BubbleMountainRoom : Room
        {
            public BubbleMountainRoom(UpperNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Bubbles Mountain", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.UpperNorfairBubbleMountain, 0x8F8C66, LocationType.Visible,
                        name: "Missile (bubble Norfair)",
                        vanillaItem: ItemType.Missile,
                        access: items => items.CardNorfairL2,
                        memoryAddress: 0x8,
                        memoryFlag: 0x1,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class SpeedBoosterHallRoom : Room
        {
            public SpeedBoosterHallRoom(UpperNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Speed Booster Hall", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.UpperNorfairSpeedBoosterHall, 0x8F8C74, LocationType.Hidden,
                        name: "Missile (Speed Booster)",
                        vanillaItem: ItemType.Missile,
                        access: items => items.CardNorfairL2 && region.CanReachBubbleMountainRightSide(items)
                                         && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump),
                        memoryAddress: 0x8,
                        memoryFlag: 0x2,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class SpeedBoosterRoom : Room
        {
            public SpeedBoosterRoom(UpperNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Speed Booster Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.UpperNorfairSpeedBooster, 0x8F8C82, LocationType.Chozo,
                        name: "Speed Booster",
                        vanillaItem: ItemType.SpeedBooster,
                        access: items => items.CardNorfairL2 && region.CanReachBubbleMountainRightSide(items)
                                         && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump),
                        memoryAddress: 0x8,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class DoubleChamberRoom : Room
        {
            public DoubleChamberRoom(UpperNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Double Chamber Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.UpperNorfairDoubleChamber, 0x8F8CBC, LocationType.Visible,
                        name: "Missile (Wave Beam)",
                        vanillaItem: ItemType.Missile,
                        access: items => (items.CardNorfairL2 && region.CanReachBubbleMountainRightSide(items)) ||
                            (items.SpeedBooster && items.Wave && items.Morph && items.Super),
                        memoryAddress: 0x8,
                        memoryFlag: 0x8,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class WaveBeamRoom : Room
        {
            public WaveBeamRoom(UpperNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Wave Beam Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.UpperNorfairWaveBeam, 0x8F8CCA, LocationType.Chozo,
                        name: "Wave Beam",
                        vanillaItem: ItemType.Wave,
                        access: items => items.Morph && (
                                (items.CardNorfairL2 && region.CanReachBubbleMountainRightSide(items)) ||
                                (items.SpeedBooster && items.Wave && items.Morph && items.Super)
                            ),
                        memoryAddress: 0x8,
                        memoryFlag: 0x10,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }
    }
}
