using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld;

public class DarkWorldNorthEast : Z3Region
{
    public DarkWorldNorthEast(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        Catfish = new Location(this, LocationId.Catfish, 0x1DE185, LocationType.Regular,
            name: "Catfish",
            vanillaItem: ItemType.Quake,
            access: items => items.MoonPearl && Logic.CanLiftLight(items),
            memoryAddress: 0x190,
            memoryFlag: 0x20,
            memoryType: LocationMemoryType.ZeldaMisc,
            metadata: metadata,
            trackerState: trackerState);

        Pyramid = new Location(this, LocationId.Pyramid, 0x308147, LocationType.Regular,
            name: "Pyramid",
            vanillaItem: ItemType.HeartPiece,
            memoryAddress: 0x5B,
            memoryFlag: 0x40,
            memoryType: LocationMemoryType.ZeldaMisc,
            metadata: metadata,
            trackerState: trackerState);

        PyramidFairy = new PyramidFairyChamber(this, metadata, trackerState);

        StartingRooms = new List<int>() { 79, 85, 86, 87, 91, 93, 94, 101, 109, 110, 111 };
        IsOverworld = true;
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Dark World North East");
        MapName = "Dark World";
    }

    public override string Name => "Dark World North East";

    public override string Area => "Dark World";

    public Location Catfish { get; }

    public Location Pyramid { get; }

    public PyramidFairyChamber PyramidFairy { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return Logic.CheckAgahnim(items, World, requireRewards) ||
               (items.MoonPearl && (
                       (items.Hammer && Logic.CanLiftLight(items)) ||
                       (Logic.CanLiftHeavy(items) && items.Flippers) ||
                       (Logic.CanAccessDarkWorldPortal(items) && items.Flippers)
                   )
               );
    }

    public class PyramidFairyChamber : Room
    {
        public PyramidFairyChamber(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Pyramid Fairy", metadata, "Cursed Fairy")
        {
            // Vanilla has torches instead of chests, but allows trading in
            // Lv3 sword for Lv4 sword and bow & arrow for silvers.
            Locations = new List<Location>
            {
                new Location(this, LocationId.PyramidFairyLeft, 0x1E980, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.ProgressiveSword,
                    access: items => CanAccessPyramidFairy(items, requireRewards: true),
                    relevanceRequirement: items => CanAccessPyramidFairy(items, requireRewards: false),
                    memoryAddress: 0x116,
                    memoryFlag: 0x4,
                    trackerLogic: items => World.CountReceivedReward(items, RewardType.CrystalRed) == 2,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.PyramidFairyRight, 0x1E983, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.SilverArrows,
                    access: items => CanAccessPyramidFairy(items, requireRewards: true),
                    relevanceRequirement: items => CanAccessPyramidFairy(items, requireRewards: false),
                    memoryAddress: 0x116,
                    memoryFlag: 0x5,
                    trackerLogic: items => World.CountReceivedReward(items, RewardType.CrystalRed) == 2,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }

        private bool CanAccessPyramidFairy(Progression items, bool requireRewards) =>
            (items.BothRedCrystals || (!requireRewards && World.CanAquireAll(items, RewardType.CrystalRed))) &&
            items.MoonPearl && World.DarkWorldSouth.CanEnter(items, requireRewards) &&
            (items.Hammer || (items.Mirror && Logic.CheckAgahnim(items, World, requireRewards)));
    }
}
