using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda.LightWorld
{
    public class LightWorldNorthWest : Z3Region
    {
        public const int SphereOne = -14;

        public LightWorldNorthWest(World world, Config config) : base(world, config)
        {
            MasterSwordPedestal = new Location(this, 256 + 14, 0x589B0, LocationType.Pedestal,
                name: "Master Sword Pedestal",
                alsoKnownAs: "Ped",
                vanillaItem: ItemType.ProgressiveSword,
                access: items => World.CanAquireAll(items, Reward.PendantGreen, Reward.PendantNonGreen));

            Mushroom = new Location(this, 256 + 15, 0x308013, LocationType.Regular,
                name: "Mushroom",
                vanillaItem: ItemType.Mushroom)
                .Weighted(SphereOne);

            LostWoodsHideout = new Location(this, 256 + 16, 0x308000, LocationType.Regular,
                name: "Lost Woods Hideout",
                vanillaItem: ItemType.HeartPiece)
                .Weighted(SphereOne);

            LumberjackTree = new Location(this, 256 + 17, 0x308001, LocationType.Regular,
                name: "Lumberjack Tree",
                vanillaItem: ItemType.HeartPiece,
                access: items => World.CanAquire(items, Reward.Agahnim) && items.Boots);

            PegasusRocks = new Location(this, 256 + 18, 0x1EB3F, LocationType.Regular,
                name: "Pegasus Rocks",
                alsoKnownAs: "Bonk Rocks",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Boots);

            GraveyardLedge = new Location(this, 256 + 19, 0x308004, LocationType.Regular,
                name: "Graveyard Ledge",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Mirror && items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items));

            KingsTomb = new Location(this, 256 + 20, 0x1E97A, LocationType.Regular,
                name: "King's Tomb",
                vanillaItem: ItemType.Cape,
                access: items => items.Boots && (
                    Logic.CanLiftHeavy(items) ||
                    (items.Mirror && items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items))));

            BottleMerchant = new Location(this, 256 + 31, 0x5EB18, LocationType.Regular,
                name: "Bottle Merchant",
                vanillaItem: ItemType.Bottle)
                .Weighted(SphereOne);

            ChickenHouse = new Location(this, 256 + 250, 0x1E9E9, LocationType.Regular,
                name: "Chicken House",
                alsoKnownAs: "Chicken Lady's House",
                vanillaItem: ItemType.TenArrows)
                .Weighted(SphereOne);

            SickKid = new Location(this, 256 + 33, 0x6B9CF, LocationType.Regular,
                name: "Sick Kid",
                alsoKnownAs: "Bug Catching Kid's House",
                vanillaItem: ItemType.Bugnet,
                access: items => items.Bottle);

            TavernBackRoom = new Location(this, 256 + 34, 0x1E9CE, LocationType.Regular,
                name: "Kakariko Tavern",
                alsoKnownAs: "Inn back room",
                vanillaItem: ItemType.Bottle)
                .Weighted(SphereOne);

            Blacksmith = new Location(this, 256 + 76, 0x30802A, LocationType.Regular,
                name: "Blacksmith",
                vanillaItem: ItemType.ProgressiveSword,
                access: items => World.DarkWorldNorthWest.CanEnter(items) && Logic.CanLiftHeavy(items));

            MagicBat = new Location(this, 256 + 35, 0x308015, LocationType.Regular,
                name: "Magic Bat",
                vanillaItem: ItemType.HalfMagic,
                access: items => items.Powder
                         && (items.Hammer
                             || (items.MoonPearl && items.Mirror && Logic.CanLiftHeavy(items))));

            KakarikoWell = new(this);
            BlindsHideout = new(this);
        }

        public override string Name => "Light World North West";

        public override string Area => "Light World";

        public Location MasterSwordPedestal { get; }

        public Location Mushroom { get; }

        public Location LostWoodsHideout { get; }

        public Location LumberjackTree { get; }

        public Location PegasusRocks { get; }

        public Location GraveyardLedge { get; }

        public Location KingsTomb { get; }

        public Location BottleMerchant { get; }

        public Location ChickenHouse { get; }

        public Location SickKid { get; }

        public Location TavernBackRoom { get; }

        public Location Blacksmith { get; }

        public Location MagicBat { get; }

        public KakarikoWellArea KakarikoWell { get; }

        public BlindsHideoutRoom BlindsHideout { get; }

        public class KakarikoWellArea : Room
        {
            public KakarikoWellArea(Region region)
                : base(region, "Kakariko Well")
            {
                BackCave = new Location(this, 256 + 21, 0x1EA8E, LocationType.Regular,
                    name: "Top",
                    alsoKnownAs: "Back cave",
                    vanillaItem: ItemType.HeartPiece)
                    .Weighted(SphereOne);

                Left = new Location(this, 256 + 22, 0x1EA91, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TwentyRupees)
                    .Weighted(SphereOne);

                Middle = new Location(this, 256 + 23, 0x1EA94, LocationType.Regular,
                    name: "Middle",
                    vanillaItem: ItemType.TwentyRupees)
                    .Weighted(SphereOne);

                Right = new Location(this, 256 + 24, 0x1EA97, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.TwentyRupees)
                    .Weighted(SphereOne);

                Bottom = new Location(this, 256 + 25, 0x1EA9A, LocationType.Regular,
                    name: "Bottom",
                    vanillaItem: ItemType.ThreeBombs)
                    .Weighted(SphereOne);
            }

            public Location BackCave { get; }

            public Location Left { get; }

            public Location Middle { get; }

            public Location Right { get; }

            public Location Bottom { get; }
        }

        public class BlindsHideoutRoom : Room
        {
            public BlindsHideoutRoom(Region region)
                : base(region, "Blind's Hideout")
            {
                BackRoom = new Location(this, 256 + 26, 0x1EB0F, LocationType.Regular,
                    "Top")
                    .Weighted(SphereOne);
                FarLeft = new Location(this, 256 + 27, 0x1EB18, LocationType.Regular,
                    "Far Left")
                    .Weighted(SphereOne);
                Left = new Location(this, 256 + 28, 0x1EB12, LocationType.Regular,
                    "Left")
                    .Weighted(SphereOne);
                Right = new Location(this, 256 + 29, 0x1EB15, LocationType.Regular,
                    "Right")
                    .Weighted(SphereOne);
                FarRight = new Location(this, 256 + 30, 0x1EB1B, LocationType.Regular,
                    "Far Right")
                    .Weighted(SphereOne);
            }

            public Location BackRoom { get; }

            public Location FarLeft { get; }

            public Location Left { get; }

            public Location Right { get; }

            public Location FarRight { get; }
        }

    }
}
