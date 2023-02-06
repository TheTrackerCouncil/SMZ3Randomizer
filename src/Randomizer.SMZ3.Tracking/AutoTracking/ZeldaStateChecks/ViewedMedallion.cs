using Randomizer.Data.WorldData;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda state check for marking medallion requirements for Misery Mire or Turtle Rock
/// </summary>
public class ViewedMedallion : IZeldaStateCheck
{
    private Tracker? _tracker;
    private readonly IWorldAccessor _worldAccessor;

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
    public bool ExecuteCheck(Tracker tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (tracker.AutoTracker == null) return false;

        _tracker = tracker;

        var x = currentState.LinkX;
        var y = currentState.LinkY;

        if (currentState.OverworldScreen == 112 && x is >= 172 and <= 438 && y is >= 3200 and <= 3432)
        {
            tracker.AutoTracker.LatestViewAction = new AutoTrackerViewedAction(MarkMiseryMireMedallion);
            return true;
        }
        else if (currentState.OverworldScreen == 71 && x is >= 3708 and <= 4016 && y is >= 128 and <= 368)
        {
            tracker.AutoTracker.LatestViewAction = new AutoTrackerViewedAction(MarkTurtleRockMedallion);
            return true;
        }

        return false;
    }

    private void MarkMiseryMireMedallion()
    {
        if (_tracker == null) return;
        var dungeon = _tracker.World.MiseryMire;
        _tracker.SetDungeonRequirement(dungeon, dungeon.DungeonState.RequiredMedallion);
    }

    private void MarkTurtleRockMedallion()
    {
        if (_tracker == null) return;
        var dungeon = _tracker.World.TurtleRock;
        _tracker.SetDungeonRequirement(dungeon, dungeon.DungeonState.RequiredMedallion);
    }

}
