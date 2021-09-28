using System.Collections.Generic;

using static Randomizer.SMZ3.Reward;

namespace Randomizer.SMZ3.Regions.Zelda.LightWorld
{
    public class LightWorldNorthEast : Z3Region
    {
        public LightWorldNorthEast(World world, Config config) : base(world, config)
        {
            MushroomItem = new Location(this, 256 + 40, 0x308014, LocationType.Regular,
                name: "Potion Shop",
                alsoKnownAs: "Mushroom Item",
                vanillaItem: ItemType.Powder,
                access: items => items.Mushroom);

            SahasrahlasHideout = new(this);
            WaterfallFairy = new(this);
            ZorasDomain = new(this);
        }

        public override string Name => "Light World North East";

        public override string Area => "Light World";

        public Location MushroomItem { get; }

        public SahasrahlasHideoutRoom SahasrahlasHideout { get; }

        public ZorasDomainArea ZorasDomain { get; }

        public WaterfallFairyChamber WaterfallFairy { get; }

        public class SahasrahlasHideoutRoom : Room
        {
            private const int SphereOne = -10;

            public SahasrahlasHideoutRoom(Region region)
                : base(region, "Sahasrahla's Hideout")
            {
                LeftChest = new Location(this, 256 + 41, 0x1EA82, LocationType.Regular,
                    name: "Sahasrahla's Hut - Left",
                    vanillaItem: ItemType.FiftyRupees)
                    .Weighted(SphereOne);

                MiddleChest = new Location(this, 256 + 42, 0x1EA85, LocationType.Regular,
                    name: "Sahasrahla's Hut - Middle",
                    vanillaItem: ItemType.ThreeBombs)
                    .Weighted(SphereOne);

                RightChest = new Location(this, 256 + 43, 0x1EA88, LocationType.Regular,
                    name: "Sahasrahla's Hut - Right",
                    vanillaItem: ItemType.FiftyRupees)
                    .Weighted(SphereOne);

                Sahasrahla = new Location(this, 256 + 44, 0x5F1FC, LocationType.Regular,
                    name: "Sahasrahla",
                    vanillaItem: ItemType.Boots,
                    access: items => World.CanAquire(items, PendantGreen));
            }

            public Location LeftChest { get; }

            public Location MiddleChest { get; }

            public Location RightChest { get; }

            public Location Sahasrahla { get; }
        }

        public class WaterfallFairyChamber : Room
        {
            public WaterfallFairyChamber(Region region)
                : base(region, "Waterfall Fairy")
            {
                Left = new Location(this, 256 + 254, 0x1E9B0, LocationType.Regular,
                    "Waterfall Fairy - Left",
                    items => items.Flippers);
                Right = new Location(this, 256 + 39, 0x1E9D1, LocationType.Regular,
                    "Waterfall Fairy - Right",
                    items => items.Flippers);
            }

            public Location Left { get; }

            public Location Right { get; }
        }

        public class ZorasDomainArea : Room
        {
            public ZorasDomainArea(Region region)
                : base(region, "Zora's Domain")
            {
                Zora = new Location(this, 256 + 36, 0x1DE1C3, LocationType.Regular,
                    name: "King Zora",
                    alsoKnownAs: "Zora",
                    vanillaItem: ItemType.Flippers,
                    access: items => items.CanLiftLight() || items.Flippers); // Consider adding 500 rupee requirement into logic

                ZoraLedge = new Location(this, 256 + 37, 0x308149, LocationType.Regular,
                    name: "Zora's Ledge",
                    vanillaItem: ItemType.HeartPiece,
                    access: items => items.Flippers);
            }

            public Location Zora { get; }

            public Location ZoraLedge { get; }
        }
    }
}
