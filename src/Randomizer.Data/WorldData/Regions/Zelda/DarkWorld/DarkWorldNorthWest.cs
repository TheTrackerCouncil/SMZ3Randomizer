using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda.DarkWorld
{
    public class DarkWorldNorthWest : Z3Region
    {
        public DarkWorldNorthWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            BumperCaveLedge = new Location(this, 256 + 71, 0x308146, LocationType.Regular,
                name: "Bumper Cave",
                alsoKnownAs: new[] { "Bumper Cave Ledge" },
                vanillaItem: ItemType.HeartPiece,
                access: items => Logic.CanLiftLight(items) && items.Cape,
                memoryAddress: 0x4A,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            ChestGame = new Location(this, 256 + 72, 0x1EDA8, LocationType.Regular,
                name: "Chest Game",
                vanillaItem: ItemType.HeartPiece,
                memoryAddress: 0x106,
                memoryFlag: 0xA,
                metadata: metadata,
                trackerState: trackerState);

            CShapedHouse = new Location(this, 256 + 73, 0x1E9EF, LocationType.Regular,
                name: "C-Shaped House", // ???
                vanillaItem: ItemType.ThreeHundredRupees, // ???
                memoryAddress: 0x11C,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            Brewery = new Location(this, 256 + 74, 0x1E9EC, LocationType.Regular,
                name: "Brewery", // ???
                vanillaItem: ItemType.ThreeHundredRupees,
                memoryAddress: 0x106,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            PegWorld = new Location(this, 256 + 75, 0x308006, LocationType.Regular,
                name: "Hammer Pegs",
                alsoKnownAs: new[] { "Peg World" },
                vanillaItem: ItemType.HeartPiece,
                access: items => Logic.CanLiftHeavy(items) && items.Hammer,
                memoryAddress: 0x127,
                memoryFlag: 0xA,
                metadata: metadata,
                trackerState: trackerState);

            PurpleChestTurnin = new Location(this, 256 + 77, 0x6BD68, LocationType.Regular,
                name: "Purple Chest",
                alsoKnownAs: new[] { "Purple Chest turn-in" },
                vanillaItem: ItemType.Bottle,
                access: items => Logic.CanLiftHeavy(items),
                memoryAddress: 0x149,
                memoryFlag: 0x10,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            StartingRooms = new List<int>() { 64, 66, 74, 80, 81, 82, 83, 84, 88, 90, 98 };
            IsOverworld = true;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Dark World North West");
        }

        public override string Name => "Dark World North West";
        public override string Area => "Dark World";

        public Location BumperCaveLedge { get; }

        public Location ChestGame { get; }

        public Location CShapedHouse { get; }

        public Location Brewery { get; }

        public Location PegWorld { get; }

        public Location PurpleChestTurnin { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.MoonPearl && (((
                    Logic.CheckAgahnim(items, World, requireRewards) ||
                    (Logic.CanAccessDarkWorldPortal(items) && items.Flippers)
                ) && items.Hookshot && (items.Flippers || Logic.CanLiftLight(items) || items.Hammer)) ||
                (items.Hammer && Logic.CanLiftLight(items)) ||
                Logic.CanLiftHeavy(items)
            );
        }

    }
}
