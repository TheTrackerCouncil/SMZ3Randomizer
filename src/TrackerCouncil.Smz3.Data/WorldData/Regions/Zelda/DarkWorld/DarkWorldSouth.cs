using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld;

public class DarkWorldSouth : Z3Region
{
    public DarkWorldSouth(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        DiggingGame = new Location(this, LocationId.DiggingGame, 0x308148, LocationType.Regular,
            name: "Digging Game",
            vanillaItem: ItemType.HeartPiece,
            memoryAddress: 0x68,
            memoryFlag: 0x40,
            memoryType: LocationMemoryType.ZeldaMisc,
            metadata: metadata,
            trackerState: trackerState);

        Stumpy = new Location(this, LocationId.Stumpy, 0x6B0C7, LocationType.Regular,
            name: "Stumpy",
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
        MapName = "Dark World";
    }

    public override string Name => "Dark World South";
    public override string Area => "Dark World";

    public Location DiggingGame { get; }

    public Location Stumpy { get; }

    public HypeCaveRoom HypeCave { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return World.Logic.CanNavigateDarkWorld(items) && (((
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
            Locations = new List<Location>
            {
                new Location(this, LocationId.HypeCaveTop, 0x1EB1E, LocationType.Regular,
                    name: "Top",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.HypeCaveMiddleRight, 0x1EB21, LocationType.Regular,
                    name: "Middle Right",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.HypeCaveMiddleLeft, 0x1EB24, LocationType.Regular,
                    name: "Middle Left",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.HypeCaveBottom, 0x1EB27, LocationType.Regular,
                    name: "Bottom",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.HypeCaveNpc, 0x308011, LocationType.Regular,
                    name: "NPC",
                    vanillaItem: ItemType.ThreeHundredRupees,
                    memoryAddress: 0x11E,
                    memoryFlag: 0xA,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }
}
