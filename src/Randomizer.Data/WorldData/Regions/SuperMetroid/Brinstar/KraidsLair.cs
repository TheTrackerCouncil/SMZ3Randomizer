using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Brinstar
{
    public class KraidsLair : SMRegion, IHasBoss
    {
        public KraidsLair(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            ETank = new Location(this, 43, 0x8F899C, LocationType.Hidden,
                name: "Energy Tank, Kraid",
                vanillaItem: ItemType.ETank,
                access: items => items.Kraid,
                relevanceRequirement: items => CanBeatBoss(items),
                memoryAddress: 0x5,
                memoryFlag: 0x8,
                metadata: metadata,
                trackerState: trackerState);
            KraidsItem = new Location(this, 48, 0x8F8ACA, LocationType.Chozo,
                name: "Varia Suit",
                alsoKnownAs: new[] { "Kraid's Reliquary" },
                vanillaItem: ItemType.Varia,
                access: items => items.Kraid,
                relevanceRequirement: items => CanBeatBoss(items),
                memoryAddress: 0x6,
                memoryFlag: 0x1,
                metadata: metadata,
                trackerState: trackerState);
            MissileBeforeKraid = new Location(this, 44, 0x8F89EC, LocationType.Hidden,
                name: "Missile (Kraid)",
                alsoKnownAs: new[] { "Warehouse Kihunter Room" },
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanUsePowerBombs(items),
                memoryAddress: 0x5,
                memoryFlag: 0x10,
                metadata: metadata,
                trackerState: trackerState);
            MemoryRegionId = 1;
            Boss = new Boss(Shared.Enums.BossType.Kraid, World, this, metadata, trackerState);
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Kraid's Lair");
        }

        public override string Name => "Kraid's Lair";

        public override string Area => "Brinstar";

        public override List<string> AlsoKnownAs { get; } = new List<string>()
        {
            "Warehouse"
        };

        public Boss Boss{ get; set; }

        public Location ETank { get; }

        public Location KraidsItem { get; }

        public Location MissileBeforeKraid { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return (Logic.CanDestroyBombWalls(items) || items.SpeedBooster || Logic.CanAccessNorfairUpperPortal(items))
                && items.Super && Logic.CanPassBombPassages(items)
                && (items.HiJump || Logic.CanWallJump(WallJumpDifficulty.Medium) || Logic.CanFly(items));
        }

        public bool CanBeatBoss(Progression items)
        {
            return CanEnter(items, true) && items.CardBrinstarBoss;
        }

    }

}
