﻿using Randomizer.Data.Configuration.ConfigTypes;
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
            BlueBrinstarMorphBall = new BlueBrinstarMorphBallRoom(this, metadata, trackerState);
            BlueBrinstarFirstMissile = new BlueBrinstarFirstMissileRoom(this, metadata, trackerState);
            BlueBrinstarEnergyTank = new BlueBrinstarEnergyTankRoom(this, metadata, trackerState);
            BlueBrinstarTop = new BlueBrinstarTopRoom(this, metadata, trackerState);
            MemoryRegionId = 1;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Blue Brinstar");
        }

        public override string Name => "Blue Brinstar";

        public override string Area => "Brinstar";

        public BlueBrinstarMorphBallRoom BlueBrinstarMorphBall { get; }

        public BlueBrinstarFirstMissileRoom BlueBrinstarFirstMissile { get; }

        public BlueBrinstarEnergyTankRoom BlueBrinstarEnergyTank { get; }

        public BlueBrinstarTopRoom BlueBrinstarTop { get; }

        public class BlueBrinstarMorphBallRoom : Room
        {
            public BlueBrinstarMorphBallRoom(BlueBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Blue Brinstar Morph Ball Room", metadata, "Morph Ball Room")
            {
                MorphBall = new Location(this, LocationId.BlueBrinstarMorphBallRight, 0x8F86EC, LocationType.Visible,
                    name: "Morphing Ball",
                    vanillaItem: ItemType.Morph,
                    memoryAddress: 0x3,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);

                PowerBomb = new Location(this, LocationId.BlueBrinstarMorphBallLeft, 0x8F874C, LocationType.Visible,
                    name: "Power Bomb (blue Brinstar)",
                    vanillaItem: ItemType.PowerBomb,
                    access: items => Logic.CanUsePowerBombs(items),
                    memoryAddress: 0x3,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location MorphBall { get; }

            public Location PowerBomb { get; }
        }

        public class BlueBrinstarFirstMissileRoom : Room
        {
            public BlueBrinstarFirstMissileRoom(BlueBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Blue Brinstar First Missile Room", metadata)
            {
                MiddleMissile = new Location(this, LocationId.BlueBrinstarFirstMissile, 0x8F8798, LocationType.Visible,
                    name: "Missile (blue Brinstar middle)",
                    vanillaItem: ItemType.Missile,
                    access: items => items.CardBrinstarL1 && items.Morph,
                    memoryAddress: 0x3,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location MiddleMissile { get; }
        }

        public class BlueBrinstarEnergyTankRoom : Room
        {
            public BlueBrinstarEnergyTankRoom(BlueBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Blue Brinstar Energy Tank Room", metadata)
            {
                Ceiling = new Location(this, LocationId.BlueBrinstarEnergyTankCeiling, 0x8F879E, LocationType.Hidden,
                    name: "Energy Tank, Brinstar Ceiling",
                    vanillaItem: ItemType.ETank,
                    access: items => items.CardBrinstarL1 && (Logic.CanFly(items) || items.HiJump || items.SpeedBooster || items.Ice),
                    memoryAddress: 0x3,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState);

                BottomMissile = new Location(this, LocationId.BlueBrinstarEnergyTankRight, 0x8F8802, LocationType.Chozo,
                    name: "Missile (blue Brinstar bottom)",
                    vanillaItem: ItemType.Missile,
                    access: items => items.Morph,
                    memoryAddress: 0x4,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Ceiling { get; }

            public Location BottomMissile { get; }
        }

        public class BlueBrinstarTopRoom : Room
        {
            public BlueBrinstarTopRoom(BlueBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Blue Brinstar Top", metadata, "Billy Mays Room")
            {
                MainItem = new Location(this, LocationId.BlueBrinstarDoubleMissileVisible, 0x8F8836, LocationType.Visible,
                    name: "Main Item",
                    vanillaItem: ItemType.Missile,
                    access: CanEnter,
                    memoryAddress: 0x4,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState);

                HiddenItem = new Location(this, LocationId.BlueBrinstarDoubleMissileHidden, 0x8F883C, LocationType.Hidden,
                    name: "Hidden Item",
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
