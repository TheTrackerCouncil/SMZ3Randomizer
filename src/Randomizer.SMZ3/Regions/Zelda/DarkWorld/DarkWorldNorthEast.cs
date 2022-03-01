using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld
{
    public class DarkWorldNorthEast : Z3Region
    {
        public DarkWorldNorthEast(World world, Config config) : base(world, config)
        {
            Catfish = new Location(this, 256 + 78, 0x1DE185, LocationType.Regular,
                name: "Catfish",
                alsoKnownAs: "Lake of Ill Omen",
                vanillaItem: ItemType.Quake,
                access: items => items.MoonPearl && World.AdvancedLogic.CanLiftLight(items));

            Pyramid = new Location(this, 256 + 79, 0x308147, LocationType.Regular,
                name: "Pyramid",
                alsoKnownAs: "Pyramid of Power",
                vanillaItem: ItemType.HeartPiece);

            PyramidFairy = new(this);
        }

        public override string Name => "Dark World North East";

        public override string Area => "Dark World";

        public Location Catfish { get; }

        public Location Pyramid { get; }

        public PyramidFairyChamber PyramidFairy { get; }

        public override bool CanEnter(Progression items)
        {
            return World.CanAquire(items, Reward.Agahnim) || (items.MoonPearl && (
                (items.Hammer && World.AdvancedLogic.CanLiftLight(items)) ||
                (World.AdvancedLogic.CanLiftHeavy(items) && items.Flippers) ||
                (World.AdvancedLogic.CanAccessDarkWorldPortal(items) && items.Flippers)
            ));
        }

        public class PyramidFairyChamber : Room
        {
            public PyramidFairyChamber(Region region)
                : base(region, "Pyramid Fairy", "Cursed Fairy")
            {
                // Vanilla has torches instead of chests, but allows trading in
                // Lv3 sword for Lv4 sword and bow & arrow for silvers.
                Left = new Location(this, 256 + 80, 0x1E980, LocationType.Regular,
                    "Left",
                    ItemType.ProgressiveSword, 
                    items => World.CanAquireAll(items, Reward.CrystalRed) && items.MoonPearl && World.DarkWorldSouth.CanEnter(items) &&
                             (items.Hammer || (items.Mirror && World.CanAquire(items, Reward.Agahnim))));

                Right = new Location(this, 256 + 81, 0x1E983, LocationType.Regular,
                    "Right",
                    ItemType.SilverArrows, 
                    items => World.CanAquireAll(items, Reward.CrystalRed) && items.MoonPearl && World.DarkWorldSouth.CanEnter(items) &&
                             (items.Hammer || (items.Mirror && World.CanAquire(items, Reward.Agahnim))));
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
