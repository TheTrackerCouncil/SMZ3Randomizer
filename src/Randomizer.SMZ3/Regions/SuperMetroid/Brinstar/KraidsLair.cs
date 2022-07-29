using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class KraidsLair : SMRegion, IHasReward
    {
        public KraidsLair(World world, Config config) : base(world, config)
        {
            ETank = new Location(this, 43, 0x8F899C, LocationType.Hidden,
                name: "Energy Tank, Kraid",
                vanillaItem: ItemType.ETank,
                access: items => items.CardBrinstarBoss,
                memoryAddress: 0x5,
                memoryFlag: 0x8);
            KraidsItem = new Location(this, 48, 0x8F8ACA, LocationType.Chozo,
                name: "Varia Suit",
                alsoKnownAs: new[] { "Kraid's Reliquary" },
                vanillaItem: ItemType.Varia,
                access: items => items.CardBrinstarBoss,
                memoryAddress: 0x6,
                memoryFlag: 0x1);
            MissileBeforeKraid = new Location(this, 44, 0x8F89EC, LocationType.Hidden,
                name: "Missile (Kraid)",
                alsoKnownAs: new[] { "Warehouse Kihunter Room" },
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanUsePowerBombs(items),
                memoryAddress: 0x5,
                memoryFlag: 0x10);
            BossReward = new Location(this, -1, -1, LocationType.MetroidBoss,
                name: "Kraid Reward",
                vanillaItem: ItemType.GoldenFourBoss,
                access: items => CanComplete(items),
                memoryAddress: null,
                memoryFlag: null);
            MemoryRegionId = 1;
        }

        public override string Name => "Kraid's Lair";

        public override string Area => "Brinstar";

        public override List<string> AlsoKnownAs { get; } = new List<string>()
        {
            "Warehouse"
        };

        public Reward Reward { get; set; } = Reward.GoldenFourBoss;

        public Location ETank { get; }

        public Location KraidsItem { get; }

        public Location MissileBeforeKraid { get; }

        public Location BossReward { get; }

        public Location RewardLocation => BossReward;

        public override bool CanEnter(Progression items)
        {
            return (Logic.CanDestroyBombWalls(items) || items.SpeedBooster || Logic.CanAccessNorfairUpperPortal(items))
                && items.Super && Logic.CanPassBombPassages(items)
                && (items.HiJump || Logic.CanWallJump(WallJumpDifficulty.Medium) || Logic.CanFly(items));
        }

        public bool CanComplete(Progression items)
        {
            return KraidsItem.IsAvailable(items);
        }

    }

}
