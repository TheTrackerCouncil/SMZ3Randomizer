using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using System.Collections.Generic;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Crateria
{
    public class WestCrateria : SMRegion
    {
        public WestCrateria(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            GauntletShaft = new GauntletShaftRoom(this, metadata, trackerState);
            GauntletEnergyTank = new GauntletEnergyTankRoom(this, metadata, trackerState);
            Terminator = new TerminatorRoom(this, metadata, trackerState);
            MemoryRegionId = 0;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("West Crateria");
            MapName = "Crateria";
        }

        public override string Name => "West Crateria";

        public override string Area => "Crateria";

        public GauntletShaftRoom GauntletShaft { get; }

        public GauntletEnergyTankRoom GauntletEnergyTank { get; }

        public TerminatorRoom Terminator { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return Logic.CanDestroyBombWalls(items) || Logic.CanParlorSpeedBoost(items);
        }

        private bool CanEnterAndLeaveGauntlet(Progression items)
        {
            return items.CardCrateriaL1 && items.Morph && (Logic.CanFly(items) || items.SpeedBooster || Logic.CanWallJump(WallJumpDifficulty.Hard)) && (
                        Logic.CanIbj(items) ||
                        (Logic.CanUsePowerBombs(items) && items.TwoPowerBombs) ||
                        Logic.CanSafelyUseScrewAttack(items)
                    );
        }

        public class GauntletShaftRoom : Room
        {
            public GauntletShaftRoom(WestCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Gauntlet Shaft", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaGauntletShaftRight, 0x8F8464, LocationType.Visible,
                        name: "Right",
                        vanillaItem: ItemType.Missile,
                        access: items => region.CanEnterAndLeaveGauntlet(items) && Logic.CanPassBombPassages(items) && Logic.HasEnergyReserves(items, 2),
                        memoryAddress: 0x1,
                        memoryFlag: 0x2,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.CrateriaGauntletShaftLeft, 0x8F846A, LocationType.Visible,
                        name: "Left",
                        vanillaItem: ItemType.Missile,
                        access: items => region.CanEnterAndLeaveGauntlet(items) && Logic.CanPassBombPassages(items) && Logic.HasEnergyReserves(items, 2),
                        memoryAddress: 0x1,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class GauntletEnergyTankRoom : Room
        {
            public GauntletEnergyTankRoom(WestCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Gauntlet Energy Tank Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaGauntletEnergyTank, 0x8F8264, LocationType.Visible,
                        name: "Energy Tank, Gauntlet",
                        vanillaItem: ItemType.ETank,
                        access: items => region.CanEnterAndLeaveGauntlet(items) && Logic.HasEnergyReserves(items, 1),
                        memoryAddress: 0x0,
                        memoryFlag: 0x20,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class TerminatorRoom : Room
        {
            public TerminatorRoom(WestCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Terminator Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaTerminator, 0x8F8432, LocationType.Visible,
                        name: "Energy Tank, Terminator",
                        vanillaItem: ItemType.ETank,
                        memoryAddress: 0x1,
                        memoryFlag: 0x1,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }
    }
}
