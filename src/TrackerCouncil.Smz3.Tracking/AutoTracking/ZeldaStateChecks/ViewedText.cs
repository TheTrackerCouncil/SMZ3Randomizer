using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for reading text
/// Checks if the game state says the player is reading text for hints
/// </summary>
public class ViewedText : IZeldaStateCheck
{
    private readonly IWorldAccessor _worldAccessor;
    private readonly TrackerBase _tracker;
    private bool _greenPendantUpdated;
    private bool _redCrystalsUpdated;
    private readonly Dictionary<int, HintTile> _hintTiles;
    private HashSet<int> _viewedHintTileRooms = new();
    private HintTile? _lastHintTile;
    private Dictionary<PlayerHintTile, List<Location>> _pendingHintTiles = [];

    public ViewedText(IWorldAccessor worldAccessor, TrackerBase tracker, HintTileConfig hintTileConfig)
    {
        _worldAccessor = worldAccessor;
        _hintTiles = hintTileConfig.HintTiles?.ToDictionary(x => x.Room, x => x) ?? [];
        _tracker = tracker;
        InitHintTiles(_worldAccessor.World.HintTiles);
    }

    private World World => _worldAccessor.World;

    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (tracker.AutoTracker == null || currentState.State != 14 || currentState.Substate != 2 || currentState.CurrentRoom == null) return false;

        if (currentState.CurrentRoom == 261 && !_greenPendantUpdated && currentState.IsWithinRegion(2650, 8543, 2692, 8594))
        {
            tracker.AutoTracker.SetLatestViewAction("MarkGreenPendantDungeons", MarkGreenPendantDungeons);
        }
        else if (currentState.CurrentRoom == 284 && !_redCrystalsUpdated && currentState.IsWithinRegion(6268, 9070, 6308, 9122))
        {
            tracker.AutoTracker.SetLatestViewAction("MarkRedCrystalDungeons", MarkRedCrystalDungeons);
        }
        else if (World.HintTiles.Any() && _hintTiles.TryGetValue(currentState.CurrentRoom.Value, out var hintTile) &&
                 !_viewedHintTileRooms.Contains(hintTile.Room) && currentState.IsWithinRegion(hintTile.TopLeftX,
                     hintTile.TopLeftY, hintTile.TopLeftX+15, hintTile.TopLeftY+1))
        {
            _lastHintTile = hintTile;
            tracker.AutoTracker.SetLatestViewAction("MarkHintTileAsViewed", MarkHintTileAsViewed);
        }

        return false;
    }

    private void InitHintTiles(IEnumerable<PlayerHintTile> hintTiles)
    {
        foreach (var hintTile in hintTiles)
        {
            if (hintTile.HintState != HintState.Viewed || hintTile.Locations?.Any() != true)
            {
                continue;
            }

            AddPendingHintTile(hintTile);
        }
    }

    /// <summary>
    /// Marks the dungeon with the green pendant
    /// </summary>
    private void MarkGreenPendantDungeons()
    {
        var dungeon = World.RewardRegions.FirstOrDefault(x =>
            x is { RewardType: RewardType.PendantGreen, HasCorrectlyMarkedReward: false });

        if (dungeon == null)
        {
            _greenPendantUpdated = true;
            return;
        }

        _tracker.RewardTracker.SetAreaReward(dungeon, dungeon.RewardType);
        _greenPendantUpdated = true;
    }

    /// <summary>
    /// Marks the dungeons with the red crystals
    /// </summary>
    private void MarkRedCrystalDungeons()
    {
        var dungeons = World.RewardRegions.Where(x =>
            x is { RewardType: RewardType.CrystalRed, HasCorrectlyMarkedReward: false }).ToList();

        if (dungeons.Count == 0)
        {
            _redCrystalsUpdated = true;
            return;
        }

        foreach (var dungeon in dungeons)
        {
            _tracker.RewardTracker.SetAreaReward(dungeon, dungeon.RewardType);
        }

        _redCrystalsUpdated = true;
    }

    private void MarkHintTileAsViewed()
    {
        if (_lastHintTile == null) return;

        var hintTile = World.HintTiles.First(x => x.HintTileCode == _lastHintTile.HintTileKey);

        if (hintTile.State == null)
        {
            return;
        }

        _viewedHintTileRooms.Add(_lastHintTile.Room);

        if (hintTile.HintState != HintState.Default)
        {
            return;
        }

        _tracker.GameStateTracker.UpdateHintTile(hintTile);

        if (hintTile.HintState == HintState.Viewed && hintTile.Locations?.Any() == true)
        {
            AddPendingHintTile(hintTile);
        }
    }

    private void AddPendingHintTile(PlayerHintTile hintTile)
    {
        var locations = hintTile.Locations!.Select(x => World.FindLocation(x)).ToList();
        _pendingHintTiles.Add(hintTile, locations);

        foreach (var location in locations)
        {
            location.ClearedUpdated += (sender, args) =>
            {
                locations.Remove(location);
                if (locations.Count != 0) return;
                hintTile.HintState = HintState.Cleared;
                _pendingHintTiles.Remove(hintTile);
                _tracker.GameStateTracker.UpdateHintTile(hintTile);
            };
        }
    }
}
