namespace Randomizer.SMZ3.Regions.Zelda
{
    public class EasternPalace : Z3Region, IHasReward
    {
        public EasternPalace(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.BigKeyEP, ItemType.MapEP, ItemType.CompassEP };

            CannonballChest = new Location(this, 256 + 103, 0x1E9B3, LocationType.Regular,
                name: "Cannonball Chest",
                vanillaItem: ItemType.OneHundredRupees);

            MapChest = new Location(this, 256 + 104, 0x1E9F5, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapEP);

            CompassChest = new Location(this, 256 + 105, 0x1E977, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassEP);

            BigChest = new Location(this, 256 + 106, 0x1E97D, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Bow,
                access: items => items.BigKeyEP);

            BigKeyChest = new Location(this, 256 + 107, 0x1E9B9, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyEP,
                access: items => items.Lamp);

            ArmosKnightsRewards = new Location(this, 256 + 108, 0x308150, LocationType.Regular,
                name: "Armos Knights",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyEP && items.Bow && items.Lamp);
        }

        public override string Name => "Eastern Palace";

        public Reward Reward { get; set; } = Reward.None;

        public Location CannonballChest { get; }

        public Location MapChest { get; }

        public Location CompassChest { get; }

        public Location BigChest { get; }

        public Location BigKeyChest { get; }

        public Location ArmosKnightsRewards { get; }

        public bool CanComplete(Progression items)
            => ArmosKnightsRewards.IsAvailable(items);
    }
}
