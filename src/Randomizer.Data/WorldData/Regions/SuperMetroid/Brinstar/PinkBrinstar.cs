using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Brinstar
{
    public class PinkBrinstar : SMRegion
    {
        public PinkBrinstar(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            Weight = -4;
            SporeSpawnReward = new Location(this, 14, 0x8F84E4, LocationType.Chozo,
                name: "Super Missile (pink Brinstar)",
                alsoKnownAs: new[] { "Spore Spawn's item" },
                vanillaItem: ItemType.Super,
                access: items => items.CardBrinstarBoss && Logic.CanPassBombPassages(items) && items.Super,
                memoryAddress: 0x1,
                memoryFlag: 0x40,
                metadata: metadata,
                trackerState: trackerState);
            PinkShaftTop = new Location(this, 21, 0x8F8608, LocationType.Visible,
                name: "Missile (pink Brinstar top)",
                alsoKnownAs: new[] { "Pink Shaft (top)", "Big Pink (top)" },
                vanillaItem: ItemType.Missile,
                access: items => items.Grapple || Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items),
                memoryAddress: 0x2,
                memoryFlag: 0x20,
                metadata: metadata,
                trackerState: trackerState);
            PinkShaftBottom = new Location(this, 22, 0x8F860E, LocationType.Visible,
                name: "Missile (pink Brinstar bottom)",
                alsoKnownAs: new[] { "Pink Shaft (bottom)", "Big Pink (bottom)" },
                vanillaItem: ItemType.Missile,
                memoryAddress: 0x2,
                memoryFlag: 0x40,
                metadata: metadata,
                trackerState: trackerState);
            PinkShaftChozo = new Location(this, 23, 0x8F8614, LocationType.Chozo,
                name: "Charge Beam",
                alsoKnownAs: new[] { "Pink Shaft - Chozo" },
                vanillaItem: ItemType.Charge,
                access: items => Logic.CanPassBombPassages(items),
                memoryAddress: 0x2,
                memoryFlag: 0x80,
                metadata: metadata,
                trackerState: trackerState);
            MissionImpossible = new Location(this, 24, 0x8F865C, LocationType.Visible,
                name: "Power Bomb (pink Brinstar)",
                alsoKnownAs: new[] { "Mission: Impossible", "Pink Brinstar Power Bomb Room" },
                vanillaItem: ItemType.PowerBomb,
                access: items => Logic.CanUsePowerBombs(items) && items.Super && Logic.HasEnergyReserves(items, 1)
                              && (items.Grapple || Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                memoryAddress: 0x3,
                memoryFlag: 0x1,
                metadata: metadata,
                trackerState: trackerState);
            GreenHillZone = new Location(this, 25, 0x8F8676, LocationType.Visible,
                name: "Missile (green Brinstar pipe)",
                alsoKnownAs: new[] { "Green Hill Zone", "Jungle slope" },
                vanillaItem: ItemType.Missile,
                access: items => items.Morph
                              && (items.PowerBomb || items.Super || Logic.CanAccessNorfairUpperPortal(items))
                              && (items.HiJump || Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                memoryAddress: 0x3,
                memoryFlag: 0x2,
                metadata: metadata,
                trackerState: trackerState);
            Waterway = new Location(this, 33, 0x8F87FA, LocationType.Visible,
                name: "Energy Tank, Waterway",
                alsoKnownAs: new[] { "Waterway" },
                vanillaItem: ItemType.ETank,
                access: items => Logic.CanUsePowerBombs(items) && Logic.CanOpenRedDoors(items) && items.SpeedBooster &&
                        ((Logic.HasEnergyReserves(items, 1) && !World.Config.LogicConfig.WaterwayNeedsGravitySuit) || items.Gravity),
                memoryAddress: 0x4,
                memoryFlag: 0x2,
                metadata: metadata,
                trackerState: trackerState);
            WaveBeamGlitchRoom = new Location(this, 35, 0x8F8824, LocationType.Visible,
                name: "Energy Tank, Brinstar Gate",
                alsoKnownAs: new[] { "Hoptank Room", "Wave Beam Glitch room" },
                vanillaItem: ItemType.ETank,
                access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items)
                              && items.Wave && Logic.HasEnergyReserves(items, 1)
                              && (items.Grapple || Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                memoryAddress: 0x4,
                memoryFlag: 0x8,
                metadata: metadata,
                trackerState: trackerState);
            MemoryRegionId = 1;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Pink Brinstar");
        }

        public override string Name => "Pink Brinstar";

        public override string Area => "Brinstar";

        public Location SporeSpawnReward { get; }

        public Location PinkShaftTop { get; }

        public Location PinkShaftBottom { get; }

        public Location PinkShaftChozo { get; }

        public Location MissionImpossible { get; }

        public Location GreenHillZone { get; }

        public Location Waterway { get; }

        public Location WaveBeamGlitchRoom { get; }

        public override bool CanEnter(Progression items, bool requireRewards) =>
                (Logic.CanOpenRedDoors(items) && (Logic.CanDestroyBombWalls(items) || Logic.CanParlorSpeedBoost(items))) ||
                Logic.CanUsePowerBombs(items) ||
                (Logic.CanAccessNorfairUpperPortal(items) && items.Morph && items.Wave &&
                    (items.Ice || items.HiJump || items.SpaceJump));
    }
}
