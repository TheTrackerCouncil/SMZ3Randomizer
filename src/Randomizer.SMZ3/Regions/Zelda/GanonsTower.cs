using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class GanonsTower : Z3Region
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5C9
        };

        public GanonsTower(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyGT, ItemType.BigKeyGT, ItemType.MapGT, ItemType.CompassGT };

            BobsTorch = new Location(this, 256 + 189, 0x308161, LocationType.Regular,
                name: "Bob's Torch",
                vanillaItem: ItemType.KeyGT,
                access: items => items.Boots);

            MapChest = new Location(this, 256 + 194, 0x1EAD3, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapGT,
                access: items => items.Hammer && (items.Hookshot || items.Boots) && items.KeyGT >=
                    (new[] { ItemType.BigKeyGT, ItemType.KeyGT }.Any(type => MapChest.ItemIs(type, World)) ? 3 : 4))
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyGT, World) && items.KeyGT >= 3);

            FiresnakeRoom = new Location(this, 256 + 195, 0x1EAD0, LocationType.Regular,
                name: "Firesnake Room",
                vanillaItem: ItemType.KeyGT,
                access: items => items.Hammer && items.Hookshot && items.KeyGT >= (new[] {
                            RandomizerRoom.TopRight,
                            RandomizerRoom.TopLeft,
                            RandomizerRoom.BottomLeft,
                            RandomizerRoom.BottomRight
                    }.Any(l => l.ItemIs(ItemType.BigKeyGT, World))
                    || FiresnakeRoom.ItemIs(ItemType.KeyGT, World) ? 2 : 3));

            TileRoom = new Location(this, 256 + 202, 0x1EAE2, LocationType.Regular,
                name: "Tile Room",
                vanillaItem: ItemType.KeyGT,
                access: items => items.Somaria);

            BobsChest = new Location(this, 256 + 207, 0x1EADF, LocationType.Regular,
                name: "Bob's Chest",
                vanillaItem: ItemType.TenArrows,
                access: items => items.KeyGT >= 3 && (
                    (items.Hammer && items.Hookshot) ||
                    (items.Somaria && items.FireRod)));

            BigChest = new Location(this, 256 + 208, 0x1EAD6, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveTunic,
                access: items => items.BigKeyGT && items.KeyGT >= 3 && (
                    (items.Hammer && items.Hookshot) ||
                    (items.Somaria && items.FireRod)))
                .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));

            PreMoldormChest = new Location(this, 256 + 214, 0x1EB03, LocationType.Regular,
                name: "Pre-Moldorm Chest",
                vanillaItem: ItemType.KeyGT,
                access: TowerAscend)
                .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));

            MoldormChest = new Location(this, 256 + 215, 0x1EB06, LocationType.Regular,
                name: "Moldorm Chest",
                vanillaItem: ItemType.TwentyRupees,
                access: items => items.BigKeyGT && items.KeyGT >= 4 &&
                    items.Bow && items.CanLightTorches() &&
                    CanBeatMoldorm(items) && items.Hookshot)
                .Allow((item, items) => new[] { ItemType.KeyGT, ItemType.BigKeyGT }.All(type => item.IsNot(type, World)));

            DMsRoom = new(this);
            RandomizerRoom = new(this);
            HopeRoom = new(this);
            CompassRoom = new(this);
            BigKeyRoom = new(this);
            MiniHelmasaurRoom = new(this);
        }

        public override string Name => "Ganon's Tower";

        public Location BobsTorch { get; }

        public Location MapChest { get; }

        public Location FiresnakeRoom { get; }

        public Location TileRoom { get; }

        public Location BobsChest { get; }

        public Location BigChest { get; }

        public Location PreMoldormChest { get; }

        public Location MoldormChest { get; }

        public DMsRoomRoom DMsRoom { get; }

        public RandomizerRoomRoom RandomizerRoom { get; }

        public HopeRoomRoom HopeRoom { get; }

        public CompassRoomRoom CompassRoom { get; }

        public BigKeyRoomRoom BigKeyRoom { get; }

        public MiniHelmasaurRoomRoom MiniHelmasaurRoom { get; }

        public override bool CanEnter(Progression items)
        {
            var requiredRewards = new[] { Reward.CrystalBlue, Reward.CrystalRed, Reward.GoldenFourBoss };
            return items.MoonPearl && World.DarkWorldDeathMountainEast.CanEnter(items) &&
                World.CanAquireAll(items, requiredRewards);
        }

        public override bool CanFill(Item item, Progression items)
        {
            if (Config.MultiWorld)
            {
                if (item.World != World || item.Progression)
                {
                    return false;
                }

                if (Config.Keysanity
                    && !((item.Type == ItemType.BigKeyGT || item.Type == ItemType.KeyGT) && item.World == World)
                    && (item.IsKey || item.IsBigKey || item.IsKeycard))
                {
                    return false;
                }
            }

            return base.CanFill(item, items);
        }

        private static bool TowerAscend(Progression items)
        {
            return items.BigKeyGT && items.KeyGT >= 3 && items.Bow && items.CanLightTorches();
        }

        private static bool CanBeatMoldorm(Progression items)
        {
            return items.Sword || items.Hammer;
        }

        public class DMsRoomRoom : Room
        {
            public DMsRoomRoom(Region region)
                : base(region, "DMs Room")
            {
                // "bombs, arrows, and Rupees" - but what?
                TopLeft = new Location(this, 256 + 190, 0x1EAB8, LocationType.Regular,
                    "Top Left",
                    items => items.Hammer && items.Hookshot);
                TopRight = new Location(this, 256 + 191, 0x1EABB, LocationType.Regular,
                    "Top Right",
                    items => items.Hammer && items.Hookshot);
                BottomLeft = new Location(this, 256 + 192, 0x1EABE, LocationType.Regular,
                    "Bottom Left",
                    items => items.Hammer && items.Hookshot);
                BottomRight = new Location(this, 256 + 193, 0x1EAC1, LocationType.Regular,
                    "Bottom Right",
                    items => items.Hammer && items.Hookshot);
            }

            public Location TopLeft { get; }

            public Location TopRight { get; }

            public Location BottomLeft { get; }

            public Location BottomRight { get; }
        }

        public class RandomizerRoomRoom : Room
        {
            public RandomizerRoomRoom(Region region)
                : base(region, "Randomizer Room") // The room with all the floor tiles
            {
                TopLeft = new Location(this, 256 + 196, 0x1EAC4, LocationType.Regular,
                    "Top Left",
                    items => LeftSide(items, new[] { TopRight, BottomLeft, BottomRight }));
                TopRight = new Location(this, 256 + 197, 0x1EAC7, LocationType.Regular,
                    "Top Right",
                    items => LeftSide(items, new[] { TopLeft, BottomLeft, BottomRight }));
                BottomLeft = new Location(this, 256 + 198, 0x1EACA, LocationType.Regular,
                    "Bottom Left",
                    items => LeftSide(items, new[] { TopRight, TopLeft, BottomRight }));
                BottomRight = new Location(this, 256 + 199, 0x1EACD, LocationType.Regular,
                    "Bottom Right",
                    items => LeftSide(items, new[] { TopRight, TopLeft, BottomLeft }));
            }

            public Location TopLeft { get; }

            public Location TopRight { get; }

            public Location BottomLeft { get; }

            public Location BottomRight { get; }

            private bool LeftSide(Progression items, IList<Location> locations)
            {
                return items.Hammer && items.Hookshot && items.KeyGT >= (locations.Any(l => l.ItemIs(ItemType.BigKeyGT, World)) ? 3 : 4);
            }
        }

        public class HopeRoomRoom : Room
        {
            public HopeRoomRoom(Region region)
                : base(region, "Hope Room", "Right Side First Room")
            {
                Left = new Location(this, 256 + 200, 0x1EAD9, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TenArrows);

                Right = new Location(this, 256 + 201, 0x1EADC, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.ThreeBombs);
            }

            public Location Left { get; }

            public Location Right { get; }
        }

        public class CompassRoomRoom : Room
        {
            public CompassRoomRoom(Region region)
                : base(region, "Compass Room")
            {
                TopLeft = new Location(this, 256 + 203, 0x1EAE5, LocationType.Regular,
                    name: "Top Left",
                    vanillaItem: ItemType.CompassGT,
                    access: items => RightSide(items, new[] { TopRight, BottomLeft, BottomRight }));

                TopRight = new Location(this, 256 + 204, 0x1EAE8, LocationType.Regular,
                    "Top Right",
                    items => RightSide(items, new[] { TopLeft, BottomLeft, BottomRight }));

                BottomLeft = new Location(this, 256 + 205, 0x1EAEB, LocationType.Regular,
                    "Bottom Left",
                    items => RightSide(items, new[] { TopRight, TopLeft, BottomRight }));

                BottomRight = new Location(this, 256 + 206, 0x1EAEE, LocationType.Regular,
                    "Bottom Right",
                    items => RightSide(items, new[] { TopRight, TopLeft, BottomLeft }));
            }

            public Location TopLeft { get; }

            public Location TopRight { get; }

            public Location BottomLeft { get; }

            public Location BottomRight { get; }

            private bool RightSide(Progression items, IList<Location> locations)
            {
                return items.Somaria && items.FireRod && items.KeyGT >= (locations.Any(l => l.ItemIs(ItemType.BigKeyGT, World)) ? 3 : 4);
            }
        }

        public class BigKeyRoomRoom : Room
        {
            public BigKeyRoomRoom(Region region)
                : base(region, "Big Key Room")
            {
                Bottom = new Location(this, 256 + 209, 0x1EAF1, LocationType.Regular,
                    name: "Bottom Big Key Chest",
                    vanillaItem: ItemType.BigKeyGT,
                    access: BigKeyRoom);

                Left = new Location(this, 256 + 210, 0x1EAF4, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TenArrows,
                    access: BigKeyRoom);

                Right = new Location(this, 256 + 211, 0x1EAF7, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.ThreeBombs,
                    access: BigKeyRoom);
            }

            public Location Bottom { get; }

            public Location Left { get; }

            public Location Right { get; }

            private bool BigKeyRoom(Progression items)
            {
                return items.KeyGT >= 3 && CanBeatArmos(items)
                    && ((items.Hammer && items.Hookshot) || (items.FireRod && items.Somaria));
            }

            private bool CanBeatArmos(Progression items)
            {
                return items.Sword || items.Hammer || items.Bow ||
                    (items.CanExtendMagic(2) && (items.Somaria || items.Byrna)) ||
                    (items.CanExtendMagic(4) && (items.FireRod || items.IceRod));
            }
        }

        public class MiniHelmasaurRoomRoom : Room
        {
            public MiniHelmasaurRoomRoom(Region region)
                : base(region, "Mini Helmasaur Room")
            {
                Left = new Location(this, 256 + 212, 0x1EAFD, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.ThreeBombs,
                    access: TowerAscend)
                    .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));

                Right = new Location(this, 256 + 213, 0x1EB00, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.ThreeBombs,
                    access: TowerAscend)
                    .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
