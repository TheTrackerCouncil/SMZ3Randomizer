using System.Collections.Generic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class BrinstarKraid : SMRegion, IHasReward
    {
        public BrinstarKraid(World world, Config config) : base(world, config)
        {
            Locations = new List<Location> {
                ETank,
                KraidsItem,
                MissileBeforeKraid,
            };
        }

        public override string Name => "Brinstar Kraid";
        public override string Area => "Brinstar";
        public override List<string> AlsoKnownAs { get; } = new()
        {
            "Warehouse"
        };

        public Reward Reward { get; set; } = Reward.GoldenFourBoss;

        public Location ETank => new(this, 43, 0x8F899C, LocationType.Hidden,
            name: "Energy Tank, Kraid",
            vanillaItem: ItemType.ETank,
            access: items => items.CardBrinstarBoss);

        public Location KraidsItem => new(this, 48, 0x8F8ACA, LocationType.Chozo,
            name: "Varia Suit",
            alsoKnownAs: "Kraid's Item",
            vanillaItem: ItemType.Varia,
            access: items => items.CardBrinstarBoss);

        public Location MissileBeforeKraid => new(this, 44, 0x8F89EC, LocationType.Hidden,
            name: "Missile (Kraid)",
            alsoKnownAs: "Warehouse Kihunter Room",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanUsePowerBombs())
            });

        public override bool CanEnter(Progression items)
        {
            return (items.CanDestroyBombWalls() || items.SpeedBooster || items.CanAccessNorfairUpperPortal()) &&
                items.Super && items.CanPassBombPassages();
        }

        public bool CanComplete(Progression items)
        {
            return GetLocation("Varia Suit").IsAvailable(items);
        }

    }

}
