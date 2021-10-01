using System.Collections.Generic;

using static Randomizer.SMZ3.Reward;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld
{
    public class DarkWorldSouth : Z3Region
    {
        public DarkWorldSouth(World world, Config config) : base(world, config)
        {
            DiggingGame = new Location(this, 256 + 82, 0x308148, LocationType.Regular,
                name: "Digging Game",
                vanillaItem: ItemType.HeartPiece);

            Stumpy = new Location(this, 256 + 83, 0x6B0C7, LocationType.Regular,
                name: "Stumpy",
                alsoKnownAs: "Haunted Grove",
                vanillaItem: ItemType.Shovel);

            HypeCave = new(this);
        }

        public override string Name => "Dark World South";
        public override string Area => "Dark World";

        public Location DiggingGame { get; }

        public Location Stumpy { get; }

        public HypeCaveRoom HypeCave { get; }

        public override bool CanEnter(Progression items)
        {
            return items.MoonPearl && (((
                    World.CanAquire(items, Agahnim) ||
                    (items.CanAccessDarkWorldPortal(Config) && items.Flippers)
                ) && (items.Hammer || (items.Hookshot && (items.Flippers || items.CanLiftLight())))) ||
                (items.Hammer && items.CanLiftLight()) ||
                items.CanLiftHeavy()
            );
        }


        public class HypeCaveRoom : Room
        {
            public HypeCaveRoom(Region region) : base(region, "Hype Cave")
            {
                Top = new Location(this, 256 + 84, 0x1EB1E, LocationType.Regular,
                    name: "Top",
                    vanillaItem: ItemType.TwentyRupees);

                MiddleRight = new Location(this, 256 + 85, 0x1EB21, LocationType.Regular,
                    name: "Middle Right",
                    vanillaItem: ItemType.TwentyRupees);

                MiddleLeft = new Location(this, 256 + 86, 0x1EB24, LocationType.Regular,
                    name: "Middle Left",
                    vanillaItem: ItemType.TwentyRupees);

                Bottom = new Location(this, 256 + 87, 0x1EB27, LocationType.Regular,
                    name: "Bottom",
                    vanillaItem: ItemType.TwentyRupees);

                Npc = new Location(this, 256 + 88, 0x308011, LocationType.Regular,
                    name: "NPC",
                    vanillaItem: ItemType.ThreeHundredRupees);
            }

            public Location Top { get; }

            public Location MiddleRight { get; }

            public Location MiddleLeft { get; }

            public Location Bottom { get; }

            public Location Npc { get; }
        }
    }

}
