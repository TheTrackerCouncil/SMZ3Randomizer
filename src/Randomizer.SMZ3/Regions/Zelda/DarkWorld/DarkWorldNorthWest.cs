﻿using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld
{
    public class DarkWorldNorthWest : Z3Region
    {
        public DarkWorldNorthWest(World world, Config config) : base(world, config)
        {
            BumperCaveLedge = new Location(this, 256 + 71, 0x308146, LocationType.Regular,
                name: "Bumper Cave",
                alsoKnownAs: "Bumper Cave Ledge",
                vanillaItem: ItemType.HeartPiece,
                access: items => Logic.CanLiftLight(items) && items.Cape);

            ChestGame = new Location(this, 256 + 72, 0x1EDA8, LocationType.Regular,
                name: "Chest Game",
                vanillaItem: ItemType.HeartPiece);

            CShapedHouse = new Location(this, 256 + 73, 0x1E9EF, LocationType.Regular,
                name: "C-Shaped House", // ???
                vanillaItem: ItemType.ThreeHundredRupees); // ???

            Brewery = new Location(this, 256 + 74, 0x1E9EC, LocationType.Regular,
                name: "Brewery", // ???
                vanillaItem: ItemType.ThreeHundredRupees);

            PegWorld = new Location(this, 256 + 75, 0x308006, LocationType.Regular,
                name: "Hammer Pegs",
                alsoKnownAs: "Peg World",
                vanillaItem: ItemType.HeartPiece,
                access: items => Logic.CanLiftHeavy(items) && items.Hammer);

            PurpleChestTurnin = new Location(this, 256 + 77, 0x6BD68, LocationType.Regular,
                name: "Purple Chest",
                alsoKnownAs: "Purple Chest turn-in",
                vanillaItem: ItemType.Bottle, // ???
                access: items => Logic.CanLiftHeavy(items));
        }

        public override string Name => "Dark World North West";
        public override string Area => "Dark World";

        public Location BumperCaveLedge { get; }

        public Location ChestGame { get; }

        public Location CShapedHouse { get; }

        public Location Brewery { get; }

        public Location PegWorld { get; }

        public Location PurpleChestTurnin { get; }

        public override bool CanEnter(Progression items)
        {
            return items.MoonPearl && (((
                    World.CanAquire(items, Reward.Agahnim) ||
                    (Logic.CanAccessDarkWorldPortal(items) && items.Flippers)
                ) && items.Hookshot && (items.Flippers || Logic.CanLiftLight(items) || items.Hammer)) ||
                (items.Hammer && Logic.CanLiftLight(items)) ||
                Logic.CanLiftHeavy(items)
            );
        }

    }
}
