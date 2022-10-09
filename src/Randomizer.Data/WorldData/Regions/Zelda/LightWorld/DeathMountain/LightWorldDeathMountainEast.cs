using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;

namespace Randomizer.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain
{
    public class LightWorldDeathMountainEast : Z3Region
    {
        public LightWorldDeathMountainEast(World world, Config config) : base(world, config)
        {
            FloatingIsland = new Location(this, 256 + 4, 0x308141, LocationType.Regular,
                name: "Floating Island",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Mirror && items.MoonPearl && Logic.CanLiftHeavy(items),
                memoryAddress: 0x5,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc);

            SpiralCave = new Location(this, 256 + 5, 0x1E9BF, LocationType.Regular,
                name: "Spiral Cave",
                vanillaItem: ItemType.FiftyRupees,
                memoryAddress: 0xFE,
                memoryFlag: 0x4);

            MirrorCave = new Location(this, 256 + 13, 0x1E9C5, LocationType.Regular,
                name: "Mimic Cave",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Mirror && items.KeyTR >= 2 && World.TurtleRock.CanEnter(items, true),
                memoryAddress: 0x10C,
                memoryFlag: 0x4,
                trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.DungeonState?.MarkedMedallion));

            ParadoxCave = new ParadoxCaveRoom(this);

            StartingRooms = new List<int>() { 5, 7 };
            IsOverworld = true;
        }

        public override string Name => "Light World Death Mountain East";

        public override string Area => "Death Mountain";

        public Location FloatingIsland { get; }

        public Location SpiralCave { get; }

        public Location MirrorCave { get; }

        public ParadoxCaveRoom ParadoxCave { get; }


        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return World.LightWorldDeathMountainWest.CanEnter(items, requireRewards) && (
                (items.Hammer && items.Mirror) ||
                items.Hookshot
            );
        }

        public class ParadoxCaveRoom : Room
        {
            public ParadoxCaveRoom(Region region)
                : base(region, "Paradox Cave")
            {
                LowerLeft = new Location(this, 256 + 6, 0x1EB39, LocationType.Regular,
                    name: "Paradox Cave Upper - Left",
                    vanillaItem: ItemType.ThreeBombs,
                    memoryAddress: 0xFF,
                    memoryFlag: 0x4);

                LowerRight = new Location(this, 256 + 7, 0x1EB3C, LocationType.Regular,
                    name: "Paradox Cave Upper - Right",
                    vanillaItem: ItemType.TenArrows,
                    memoryAddress: 0xFF,
                    memoryFlag: 0x5);

                UpperFarLeft = new Location(this, 256 + 8, 0x1EB2A, LocationType.Regular,
                    name: "Paradox Cave Lower - Far Left",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0xEF,
                    memoryFlag: 0x4);

                UpperLeft = new Location(this, 256 + 9, 0x1EB2D, LocationType.Regular,
                    name: "Paradox Cave Lower - Left",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0xEF,
                    memoryFlag: 0x5);

                UpperMiddle = new Location(this, 256 + 10, 0x1EB36, LocationType.Regular,
                    name: "Paradox Cave Lower - Middle",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0xEF,
                    memoryFlag: 0x8);

                UpperRight = new Location(this, 256 + 11, 0x1EB30, LocationType.Regular,
                    name: "Paradox Cave Lower - Right",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0xEF,
                    memoryFlag: 0x6);

                UpperFarRight = new Location(this, 256 + 12, 0x1EB33, LocationType.Regular,
                    name: "Paradox Cave Lower - Far Right",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0xEF,
                    memoryFlag: 0x7);
            }

            public Location LowerLeft { get; }

            public Location LowerRight { get; }

            public Location UpperFarLeft { get; }

            public Location UpperLeft { get; }

            public Location UpperMiddle { get; }

            public Location UpperRight { get; }

            public Location UpperFarRight { get; }
        }
    }

}
