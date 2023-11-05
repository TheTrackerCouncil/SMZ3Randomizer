using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using System.Collections.Generic;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Crateria
{
    public class CentralCrateria : SMRegion
    {
        // Considered part of West according to the speedrun wiki

        public CentralCrateria(World world, Config config, IMetadataService? metadata, TrackerState? trackerState)
            : base(world, config, metadata, trackerState)
        {
            PowerBomb = new PowerBombRoom(this, metadata, trackerState);
            TheFinalMissile = new TheFinalMissileRoom(this, metadata, trackerState);
            Pit = new PitRoom(this, metadata, trackerState);
            CrateriaSuper = new CrateriaSuperRoom(this, metadata, trackerState);
            BombTorizo = new BombTorizoRoom(this, metadata, trackerState);
            MemoryRegionId = 0;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Central Crateria");
        }

        public override string Name => "Central Crateria";

        public override string Area => "Crateria";

        public PowerBombRoom PowerBomb { get; }

        public TheFinalMissileRoom TheFinalMissile { get; }

        public PitRoom Pit { get; }

        public CrateriaSuperRoom CrateriaSuper { get; }

        public BombTorizoRoom BombTorizo { get; }

        public class PowerBombRoom : Room
        {
            public PowerBombRoom(CentralCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Crateria Power Bomb Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaPowerBomb, 0x8F81CC, LocationType.Visible,
                        name: "Power Bomb (Crateria surface)",
                        vanillaItem: ItemType.PowerBomb,
                        access: items => (Config.GameModeConfigs.KeysanityConfig.MetroidKeysanity ? items.CardCrateriaL1 : Logic.CanUsePowerBombs(items)) && (items.SpeedBooster || Logic.CanFly(items)),
                        memoryAddress: 0x0,
                        memoryFlag: 0x1,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class TheFinalMissileRoom : Room
        {
            public TheFinalMissileRoom(CentralCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "The Final Missile Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaFinalMissile, 0x8F8486, LocationType.Visible,
                        name: "Missile (Crateria middle)",
                        vanillaItem: ItemType.Missile,
                        access: items => Logic.CanPassBombPassages(items),
                        memoryAddress: 0x1,
                        memoryFlag: 0x10,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class PitRoom : Room
        {
            public PitRoom(CentralCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Pit Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaPit, 0x8F83EE, LocationType.Visible,
                        name: "Missile (Crateria bottom)",
                        vanillaItem: ItemType.Missile,
                        access: items => Logic.CanDestroyBombWalls(items),
                        memoryAddress: 0x0,
                        memoryFlag: 0x40,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class CrateriaSuperRoom : Room
        {
            public CrateriaSuperRoom(CentralCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Crateria Super Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaSuper, 0x8F8478, LocationType.Visible,
                        name: "Super Missile (Crateria)",
                        vanillaItem: ItemType.Super,
                        access: items => Logic.CanUsePowerBombs(items) && Logic.HasEnergyReserves(items, 2) && items.SpeedBooster && (!World.Config.LogicConfig.LaunchPadRequiresIceBeam || items.Ice),
                        memoryAddress: 0x1,
                        memoryFlag: 0x8,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class BombTorizoRoom : Room
        {
            public BombTorizoRoom(CentralCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Bomb Torizo Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaBombTorizo, 0x8F8404, LocationType.Chozo,
                        name: "Bombs",
                        vanillaItem: ItemType.Bombs,
                        access: items => (Config.GameModeConfigs.KeysanityConfig.MetroidKeysanity ? items.CardCrateriaBoss : Logic.CanOpenRedDoors(items))
                                         && (Logic.CanPassBombPassages(items) || Logic.CanWallJump(WallJumpDifficulty.Hard)),
                        memoryAddress: 0x0,
                        memoryFlag: 0x80,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }
    }
}
