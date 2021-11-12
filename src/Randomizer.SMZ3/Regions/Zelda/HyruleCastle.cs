namespace Randomizer.SMZ3.Regions.Zelda
{
    public class HyruleCastle : Z3Region
    {
        public const int SphereOne = -10;

        public HyruleCastle(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyHC, ItemType.MapHC };

            Sanctuary = new Location(this, 256 + 91, 0x1EA79, LocationType.Regular,
                "Sanctuary",
                ItemType.HeartContainer)
                .Weighted(SphereOne);

            MapChest = new Location(this, 256 + 96, 0x1EB0C, LocationType.Regular,
                "Map Chest",
                ItemType.MapHC)
                .Weighted(SphereOne);

            BoomerangChest = new Location(this, 256 + 97, 0x1E974, LocationType.Regular,
                "Boomerang Chest",
                ItemType.BlueBoomerang,
                items => items.KeyHC)
                .Weighted(SphereOne);

            ZeldasCell = new Location(this, 256 + 98, 0x1EB09, LocationType.Regular,
                "Zelda's Cell",
                ItemType.FiveRupees,
                items => items.KeyHC)
                .Weighted(SphereOne);

            LinksUncle = new Location(this, 256 + 99, 0x5DF45, LocationType.NotInDungeon,
                "Link's Uncle",
                ItemType.ProgressiveSword)
                .Allow((item, items) => Config.Keysanity || !item.IsDungeonItem)
                .Weighted(SphereOne);

            SecretPassage = new Location(this, 256 + 100, 0x1E971, LocationType.NotInDungeon,
                "Secret Passage",
                ItemType.FiveRupees)
                .Allow((item, items) => Config.Keysanity || !item.IsDungeonItem)
                .Weighted(SphereOne);

            BackOfEscape = new(this);
        }

        public override string Name => "Hyrule Castle";

        public Location Sanctuary { get; }

        public Location MapChest { get; }

        public Location BoomerangChest { get; }

        public Location ZeldasCell { get; }

        public Location LinksUncle { get; }

        public Location SecretPassage { get; }

        public Sewers BackOfEscape { get; }

        public class Sewers : Room
        {
            public Sewers(Region region)
                : base(region, "Sewers", "Back of Escape")
            {
                SecretRoomLeft = new Location(this, 256 + 92, 0x1EB5D, LocationType.Regular,
                    name: "Secret Room - Left",
                    vanillaItem: ItemType.ThreeBombs,
                    access: items => items.CanLiftLight() || (items.Lamp && items.KeyHC));

                SecretRoomMiddle = new Location(this, 256 + 93, 0x1EB60, LocationType.Regular,
                    name: "Secret Room - Middle",
                    vanillaItem: ItemType.ThreeHundredRupees,
                    access: items => items.CanLiftLight() || (items.Lamp && items.KeyHC));

                SecretRoomRight = new Location(this, 256 + 94, 0x1EB63, LocationType.Regular,
                    name: "Secret Room - Right",
                    vanillaItem: ItemType.TenArrows,
                    access: items => items.CanLiftLight() || (items.Lamp && items.KeyHC));

                DarkCross = new Location(this, 256 + 95, 0x1E96E, LocationType.Regular,
                    name: "Dark Cross",
                    vanillaItem: ItemType.KeyHC,
                    access: items => items.Lamp);
            }

            public Location SecretRoomLeft { get; }

            public Location SecretRoomMiddle { get; }

            public Location SecretRoomRight { get; }

            public Location DarkCross { get; }
        }
    }
}
