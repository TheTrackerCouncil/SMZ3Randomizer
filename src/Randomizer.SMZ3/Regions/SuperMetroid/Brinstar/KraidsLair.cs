using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class KraidsLair : SMRegion, IHasReward
    {
        public KraidsLair(World world, Config config) : base(world, config)
        {
            ETank = new(this, 43, 0x8F899C, LocationType.Hidden,
                name: "Energy Tank, Kraid",
                vanillaItem: ItemType.ETank,
                access: items => items.CardBrinstarBoss);
            KraidsItem = new(this, 48, 0x8F8ACA, LocationType.Chozo,
                name: "Varia Suit",
                alsoKnownAs: "Kraid's Reliquary",
                vanillaItem: ItemType.Varia,
                access: items => items.CardBrinstarBoss);
            MissileBeforeKraid = new(this, 44, 0x8F89EC, LocationType.Hidden,
                name: "Missile (Kraid)",
                alsoKnownAs: "Warehouse Kihunter Room",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => items => world.Logic.CanUsePowerBombs(items)
                });
        }

        public override string Name => "Kraid's Lair";

        public override string Area => "Brinstar";

        public override List<string> AlsoKnownAs { get; } = new()
        {
            "Warehouse"
        };

        public Reward Reward { get; set; } = Reward.GoldenFourBoss;

        public Location ETank { get; }

        public Location KraidsItem { get; }

        public Location MissileBeforeKraid { get; }

        public override bool CanEnter(Progression items)
        {
            return (World.Logic.CanDestroyBombWalls(items) || items.SpeedBooster || World.Logic.CanAccessNorfairUpperPortal(items)) &&
                items.Super && World.Logic.CanPassBombPassages(items);
        }

        public bool CanComplete(Progression items)
        {
            return KraidsItem.IsAvailable(items);
        }

    }

}
