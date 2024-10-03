using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using IHasReward = TrackerCouncil.Smz3.Data.WorldData.Regions.IHasReward;
using Z3Region = TrackerCouncil.Smz3.Data.WorldData.Regions.Z3Region;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for viewing the map
/// Checks if the game state is viewing the map and the value for viewing the full map is set
/// </summary>
public class ViewedMap : IZeldaStateCheck
{
    private TrackerBase? _tracker;
    private readonly IWorldAccessor _worldAccessor;
    private bool _lightWorldUpdated;
    private bool _darkWorldUpdated;

    public ViewedMap(IWorldAccessor worldAccessor, IItemService itemService)
    {
        _worldAccessor = worldAccessor;
        Items = itemService;
    }

    private World World => _worldAccessor.World;

    private IItemService Items { get; }

    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (tracker.AutoTracker == null || (_lightWorldUpdated && _darkWorldUpdated) || currentState.State != 14 ||
            currentState.Substate != 7 || currentState.ReadUInt8(0xE0) != 0x80 || currentState.OverworldScreen == null)
        {
            return false;
        }

        _tracker = tracker;
        var currentRegion = tracker.World.Regions
            .OfType<Z3Region>()
            .FirstOrDefault(x => x.StartingRooms.Contains(currentState.OverworldScreen.Value) && x.IsOverworld);
        if (currentRegion is LightWorldNorthWest or LightWorldNorthEast or LightWorldSouth or LightWorldDeathMountainEast or LightWorldDeathMountainWest && !_lightWorldUpdated)
        {
            tracker.AutoTracker.SetLatestViewAction("UpdateLightWorldRewards", UpdateLightWorldRewards);
            return true;
        }
        else if (currentRegion is DarkWorldNorthWest or DarkWorldNorthEast or DarkWorldSouth or DarkWorldMire or DarkWorldDeathMountainEast or DarkWorldDeathMountainWest && !_darkWorldUpdated)
        {
            tracker.AutoTracker.SetLatestViewAction("UpdateDarkWorldRewards", UpdateDarkWorldRewards);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Marks all of the rewards for the light world dungeons
    /// </summary>
    private void UpdateLightWorldRewards()
    {
        if (_tracker == null || _lightWorldUpdated) return;

        var dungeons = new (IHasReward Region, ItemType Map)[] {
            (World.EasternPalace, ItemType.MapEP),
            (World.DesertPalace, ItemType.MapDP),
            (World.TowerOfHera, ItemType.MapTH)
        };

        var rewards = CheckDungeonRewards(dungeons);

        if (!World.Config.ZeldaKeysanity)
        {
            if (rewards.Count(x => x is RewardType.CrystalRed or RewardType.CrystalBlue) == 3)
            {
                _tracker.Say(x => x.AutoTracker.LightWorldAllCrystals, once: true);
            }
            else if (rewards.Count == 0)
            {
                _tracker.Say(x => x.AutoTracker.LookedAtNothing);
            }
        }

        // If all dungeons are marked, save the light world as updated
        if (dungeons.Count(x => x.Region.HasCorrectlyMarkedReward) >= dungeons.Length)
        {
            _lightWorldUpdated = true;
        }

    }

    /// <summary>
    /// Marks all of the rewards for the dark world dungeons
    /// </summary>
    private void UpdateDarkWorldRewards()
    {
        if (_tracker == null || _darkWorldUpdated) return;

        var dungeons = new (IHasReward Region, ItemType Map)[] {
            (World.PalaceOfDarkness, ItemType.MapPD),
            (World.SwampPalace, ItemType.MapSP),
            (World.SkullWoods, ItemType.MapSW),
            (World.ThievesTown, ItemType.MapTT),
            (World.IcePalace, ItemType.MapIP),
            (World.MiseryMire, ItemType.MapMM),
            (World.TurtleRock, ItemType.MapTR)
        };

        var rewards = CheckDungeonRewards(dungeons);

        if (!World.Config.ZeldaKeysanity)
        {
            RewardType[] pendants = [RewardType.PendantBlue, RewardType.PendantGreen, RewardType.PendantRed];
            var isMiseryMirePendant = pendants.Contains(World.MiseryMire.Reward.Type);
            var isTurtleRockPendant = pendants.Contains(World.TurtleRock.Reward.Type);

            if (isMiseryMirePendant && isTurtleRockPendant)
            {
                _tracker.Say(x => x.AutoTracker.DarkWorldNoMedallions, once: true);
            }
            else if (rewards.Count == 0)
            {
                _tracker.Say(x => x.AutoTracker.LookedAtNothing);
            }
        }

        // If all dungeons are marked, save the light world as updated
        if (dungeons.Count(x => x.Region.HasCorrectlyMarkedReward) >= dungeons.Length)
        {
            _darkWorldUpdated = true;
        }

    }

    private List<RewardType> CheckDungeonRewards((IHasReward Region, ItemType Map)[] dungeons)
    {
        var rewards = new List<RewardType>();

        foreach (var (region, map) in dungeons)
        {
            if (World.Config.ZeldaKeysanity && !Items.IsTracked(map))
                continue;

            if (region.HasCorrectlyMarkedReward) continue;
            rewards.Add(region.RewardType);
            _tracker!.RewardTracker.SetDungeonReward(region, region.RewardType);
        }

        return rewards;
    }
}
