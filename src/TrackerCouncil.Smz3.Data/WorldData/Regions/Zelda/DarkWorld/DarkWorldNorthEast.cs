using System;
using System.Collections.Generic;
using System.Linq;
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
            access: items => World.Logic.CanNavigateDarkWorld(items) && Logic.CanLiftLight(items),
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

    public Accessibility GanonAccessibility { get; private set; }

    public event EventHandler? UpdatedGanonAccessibility;

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return Logic.CheckAgahnim(items, World, requireRewards) ||
               (World.Logic.CanNavigateDarkWorld(items) && (
                       (items.Hammer && Logic.CanLiftLight(items)) ||
                       (Logic.CanLiftHeavy(items) && items.Flippers) ||
                       (Logic.CanAccessDarkWorldPortal(items) && items.Flippers)
                   )
               );
    }

    public void UpdateGanonAccessibility(Progression progression, Progression assumedKeyProgression)
    {
        Accessibility NewAccessibility;

        var ganonCrystalRequirement = World.State?.GanonCrystalCount == null
            ? Config.GanonCrystalCount
            : World.State?.MarkedGanonCrystalCount ?? 7;

        var numAcquiredCrystals = World.Rewards.Count(x =>
            x.MarkedReward?.IsInAnyCategory(RewardCategory.Crystal) == true && x.HasReceivedReward);

        if (World.Bosses.First(x => x.Type == BossType.Ganon).Defeated)
        {
            NewAccessibility = Accessibility.Cleared;
        }
        else if (World.LegacyWorld == null)
        {
            var canAccessPyramid = Pyramid.IsAvailable(progression);

            var canClimbGt = World.GanonsTower.CanBeatBoss(progression, true) ||
                             (!World.Config.ZeldaKeysanity &&
                              World.GanonsTower.Locations.All(x => x.IsAvailable(assumedKeyProgression)) &&
                              World.GanonsTower.CanBeatBoss(assumedKeyProgression, true));

            var isPyramidOpen = World.Config.OpenPyramid || canClimbGt;
            var canHurtGanon = numAcquiredCrystals >= ganonCrystalRequirement && progression.MasterSword &&
                               (progression.Lamp || progression.FireRod);

            NewAccessibility = canAccessPyramid && isPyramidOpen && canHurtGanon
                ? Accessibility.Available
                : Accessibility.OutOfLogic;
        }
        else
        {
            var canAccessPyramid = World.LegacyWorld.IsLocationAccessible((int)LocationId.Pyramid, progression.LegacyProgression);

            var canClimbGt =
                World.LegacyWorld.IsLocationAccessible((int)LocationId.GanonsTowerMoldormChest,
                    progression.LegacyProgression) || (!World.Config.ZeldaKeysanity &&
                                                       World.GanonsTower.Locations.All(x =>
                                                           World.LegacyWorld.IsLocationAccessible((int)x.Id,
                                                               assumedKeyProgression.LegacyProgression)));

            var isPyramidOpen = World.Config.OpenPyramid || canClimbGt;
            var canHurtGanon = numAcquiredCrystals >= ganonCrystalRequirement && progression.MasterSword &&
                               (progression.Lamp || progression.FireRod);

            NewAccessibility = canAccessPyramid && isPyramidOpen && canHurtGanon
                ? Accessibility.Available
                : Accessibility.OutOfLogic;
        }

        if (NewAccessibility != GanonAccessibility)
        {
            GanonAccessibility = NewAccessibility;
            UpdatedGanonAccessibility?.Invoke(this, EventArgs.Empty);
        }
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
            (items.BothRedCrystals || (!requireRewards && World.CanAquireAll(items, requireRewards, RewardType.CrystalRed))) &&
            World.Logic.CanNavigateDarkWorld(items) && World.DarkWorldSouth.CanEnter(items, requireRewards) &&
            (items.Hammer || (items.Mirror && Logic.CheckAgahnim(items, World, requireRewards)));
    }
}
