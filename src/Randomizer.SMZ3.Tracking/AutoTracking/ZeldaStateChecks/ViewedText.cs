using System.Collections.Generic;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Tracking;
using Randomizer.Shared;
using Randomizer.SMZ3.Contracts;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using HintTile = Randomizer.Data.Configuration.ConfigTypes.HintTile;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

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
    private Dictionary<PlayerHintTile, List<Location>> _pendingHintTiles;

    public ViewedText(IWorldAccessor worldAccessor, TrackerBase tracker, HintTileConfig hintTileConfig)
    {
        _worldAccessor = worldAccessor;
        _hintTiles = hintTileConfig.HintTiles?.ToDictionary(x => x.Room, x => x) ?? new();
        _tracker = tracker;
        _tracker.LocationCleared += TrackerOnLocationCleared;
        _pendingHintTiles = _worldAccessor.World.HintTiles
            .Where(x => x.State is { HintState: HintState.Viewed } && x.Locations?.Any() == true).ToDictionary(h => h,
                h => h.Locations!.Select(l => _worldAccessor.World.FindLocation(l)).ToList());
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
        if (tracker.AutoTracker == null || currentState.State != 14 || currentState.Substate != 2) return false;

        if (currentState.CurrentRoom == 261 && !_greenPendantUpdated && currentState.IsWithinRegion(2650, 8543, 2692, 8594))
        {
            tracker.AutoTracker.SetLatestViewAction("MarkGreenPendantDungeons", MarkGreenPendantDungeons);
        }
        else if (currentState.CurrentRoom == 284 && !_redCrystalsUpdated && currentState.IsWithinRegion(6268, 9070, 6308, 9122))
        {
            tracker.AutoTracker.SetLatestViewAction("MarkRedCrystalDungeons", MarkRedCrystalDungeons);
        }
        else if (World.HintTiles.Any() && _hintTiles.TryGetValue(currentState.CurrentRoom, out var hintTile) &&
                 !_viewedHintTileRooms.Contains(hintTile.Room) && currentState.IsWithinRegion(hintTile.TopLeftX,
                     hintTile.TopLeftY, hintTile.TopLeftX+15, hintTile.TopLeftY+1))
        {
            _lastHintTile = hintTile;
            tracker.LastViewedHintTile = World.HintTiles.First(x => x.HintTileCode == _lastHintTile.HintTileKey);
            tracker.AutoTracker.SetLatestViewAction("MarkHintTileAsViewed", MarkHintTileAsViewed);
        }

        return false;
    }

    /// <summary>
    /// Marks the dungeon with the green pendant
    /// </summary>
    private void MarkGreenPendantDungeons()
    {
        var dungeon = World.Dungeons.FirstOrDefault(x =>
            x.DungeonRewardType == RewardType.PendantGreen && x.MarkedReward != RewardType.PendantGreen);

        if (dungeon == null)
        {
            _greenPendantUpdated = true;
            return;
        }

        _tracker.SetDungeonReward(dungeon, dungeon.DungeonRewardType);
        _greenPendantUpdated = true;
    }

    /// <summary>
    /// Marks the dungeons with the red crystals
    /// </summary>
    private void MarkRedCrystalDungeons()
    {
        var dungeons = World.Dungeons.Where(x =>
            x.DungeonRewardType == RewardType.CrystalRed && x.MarkedReward != RewardType.CrystalRed).ToList();

        if (!dungeons.Any())
        {
            _redCrystalsUpdated = true;
            return;
        }

        foreach (var dungeon in dungeons)
        {
            _tracker.SetDungeonReward(dungeon, dungeon.DungeonRewardType);
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

        if (hintTile.State.HintState != HintState.Default)
        {
            return;
        }

        _tracker.UpdateHintTile(hintTile);

        if (hintTile.State.HintState == HintState.Viewed && hintTile.Locations?.Any() == true)
        {
            var locations = hintTile.Locations!.Select(x => World.FindLocation(x)).ToList();
            _pendingHintTiles.Add(hintTile, locations);
        }
    }

    private void TrackerOnLocationCleared(object? sender, LocationClearedEventArgs e)
    {
        foreach (var (hintTile, locations) in _pendingHintTiles)
        {
            if (locations.All(x => x.State.Autotracked || x.State.Cleared))
            {
                hintTile.State!.HintState = HintState.Cleared;
                _pendingHintTiles.Remove(hintTile);
                _tracker.UpdateHintTile(hintTile);
            }
        }
    }
}
