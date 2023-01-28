using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda.DarkWorld
{
    public class DarkWorldSouth : Z3Region
    {
        public DarkWorldSouth(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            DiggingGame = new Location(this, 256 + 82, 0x308148, LocationType.Regular,
                name: "Digging Game",
                vanillaItem: ItemType.HeartPiece,
                memoryAddress: 0x68,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            Stumpy = new Location(this, 256 + 83, 0x6B0C7, LocationType.Regular,
                name: "Stumpy",
                alsoKnownAs: new[] { "Haunted Grove" },
                vanillaItem: ItemType.Shovel,
                memoryAddress: 0x190,
                memoryFlag: 0x8,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            HypeCave = new HypeCaveRoom(this, metadata, trackerState);

            StartingRooms = new List<int>() { 104, 105, 106, 107, 108, 109, 114, 115, 116, 117, 119, 122, 123, 124, 127 };
            IsOverworld = true;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Dark World South");
        }

        public override string Name => "Dark World South";
        public override string Area => "Dark World";

        public Location DiggingGame { get; }

        public Location Stumpy { get; }

        public HypeCaveRoom HypeCave { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.MoonPearl && (((
                    Logic.CheckAgahnim(items, World, requireRewards) ||
                    (Logic.CanAccessDarkWorldPortal(items) && items.Flippers)
                ) && (items.Hammer || (items.Hookshot && (items.Flippers || Logic.CanLiftLight(items))))) ||
                (items.Hammer && Logic.CanLiftLight(items)) ||
                Logic.CanLiftHeavy(items)
            );
        }


        public class HypeCaveRoom : Room
        {
            public HypeCaveRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Hype Cave", metadata)
            {
                Top = new Location(this, 256 + 84, 0x1EB1E, LocationType.Regular,
                    name: "Top",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);

                MiddleRight = new Location(this, 256 + 85, 0x1EB21, LocationType.Regular,
                    name: "Middle Right",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);

                MiddleLeft = new Location(this, 256 + 86, 0x1EB24, LocationType.Regular,
                    name: "Middle Left",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState);

                Bottom = new Location(this, 256 + 87, 0x1EB27, LocationType.Regular,
                    name: "Bottom",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState);

                Npc = new Location(this, 256 + 88, 0x308011, LocationType.Regular,
                    name: "NPC",
                    vanillaItem: ItemType.ThreeHundredRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0xA,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Top { get; }

            public Location MiddleRight { get; }

            public Location MiddleLeft { get; }

            public Location Bottom { get; }

            public Location Npc { get; }
        }
    }

}
