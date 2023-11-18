using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using Randomizer.Shared;
using Randomizer.SMZ3.Contracts;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for reading text
/// Checks if the game state says the player is reading text for hints
/// </summary>
public class ViewedText : IZeldaStateCheck
{
    private readonly IWorldAccessor _worldAccessor;
    private readonly IItemService _items;
    private TrackerBase? _tracker;
    private bool _greenPendantUpdated;
    private bool _redCrystalsUpdated;

    public ViewedText(IWorldAccessor worldAccessor, IItemService itemService)
    {
        _worldAccessor = worldAccessor;
        _items = itemService;
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
        if (tracker.AutoTracker == null || currentState.State != 14 && currentState.Substate != 2) return false;

        _tracker = tracker;

        if (currentState.CurrentRoom == 261 && !_greenPendantUpdated && currentState.IsWithinRegion(2650, 8543, 2692, 8594))
        {
            tracker.AutoTracker.SetLatestViewAction(MarkGreenPendantDungeons);
        }
        else if (currentState.CurrentRoom == 284 && !_redCrystalsUpdated && currentState.IsWithinRegion(6268, 9070, 6308, 9122))
        {
            tracker.AutoTracker.SetLatestViewAction(MarkRedCrystalDungeons);
        }

        return false;
    }

    /// <summary>
    /// Marks the dungeon with the green pendant
    /// </summary>
    private void MarkGreenPendantDungeons()
    {
        if (_tracker == null) return;

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
        if (_tracker == null) return;

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
}
