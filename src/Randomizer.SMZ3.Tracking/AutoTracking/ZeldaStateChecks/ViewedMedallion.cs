using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda state check for marking medallion requirements for Misery Mire or Turtle Rock
/// </summary>
public class ViewedMedallion : IZeldaStateCheck
{
    private TrackerBase? _tracker;
    private readonly IWorldAccessor _worldAccessor;
    private bool _mireUpdated;
    private bool _turtleRockUpdated;

    public ViewedMedallion(IWorldAccessor worldAccessor, IItemService itemService)
    {
        _worldAccessor = worldAccessor;
        Items = itemService;
    }

    protected World World => _worldAccessor.World;

    protected IItemService Items { get; }

    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (tracker.AutoTracker == null || tracker.AutoTracker.LatestViewAction?.IsValid == true || (_mireUpdated && _turtleRockUpdated)) return false;

        _tracker = tracker;

        var x = currentState.LinkX;
        var y = currentState.LinkY;

        if (!_mireUpdated && currentState.OverworldScreen == 112 && x is >= 172 and <= 438 && y is >= 3200 and <= 3432)
        {
            tracker.AutoTracker.LatestViewAction = new AutoTrackerViewedAction(MarkMiseryMireMedallion);
            if (tracker.Options.AutoSaveLookAtEvents)
            {
                tracker.AutoTracker.LatestViewAction.Invoke();
            }
            return true;
        }
        else if (!_turtleRockUpdated && currentState.OverworldScreen == 71 && x is >= 3708 and <= 4016 && y is >= 128 and <= 368)
        {
            tracker.AutoTracker.LatestViewAction = new AutoTrackerViewedAction(MarkTurtleRockMedallion);
            if (tracker.Options.AutoSaveLookAtEvents)
            {
                tracker.AutoTracker.LatestViewAction.Invoke();
            }
            return true;
        }

        return false;
    }

    private void MarkMiseryMireMedallion()
    {
        if (_tracker == null || _mireUpdated) return;
        var dungeon = _tracker.World.MiseryMire;
        _tracker.SetDungeonRequirement(dungeon, dungeon.DungeonState.RequiredMedallion, null, true);
        _mireUpdated = true;
    }

    private void MarkTurtleRockMedallion()
    {
        if (_tracker == null || _turtleRockUpdated) return;
        var dungeon = _tracker.World.TurtleRock;
        _tracker.SetDungeonRequirement(dungeon, dungeon.DungeonState.RequiredMedallion, null, true);
        _turtleRockUpdated = true;
    }

}
