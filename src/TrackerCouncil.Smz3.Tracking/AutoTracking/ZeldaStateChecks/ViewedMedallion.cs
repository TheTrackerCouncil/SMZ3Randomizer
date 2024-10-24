using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda state check for marking medallion requirements for Misery Mire or Turtle Rock
/// </summary>
public class ViewedMedallion(IWorldAccessor worldAccessor) : IZeldaStateCheck
{
    private TrackerBase? _tracker;
    private bool _mireUpdated;
    private bool _turtleRockUpdated;
    private World World => worldAccessor.World;

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

        if (!_mireUpdated && currentState.OverworldScreen == 112 && x is >= 172 and <= 438 && y is >= 3200 and <= 3432 && !((IHasPrerequisite)World.MiseryMire).HasMarkedCorrectly)
        {
            tracker.AutoTracker.SetLatestViewAction("MarkMiseryMireMedallion", MarkMiseryMireMedallion);
            return true;
        }
        else if (!_turtleRockUpdated && currentState.OverworldScreen == 71 && x is >= 3708 and <= 4016 && y is >= 128 and <= 368 && !((IHasPrerequisite)World.TurtleRock).HasMarkedCorrectly)
        {
            tracker.AutoTracker.SetLatestViewAction("MarkTurtleRockMedallion", MarkTurtleRockMedallion);
            return true;
        }

        return false;
    }

    private void MarkMiseryMireMedallion()
    {
        if (_tracker == null || _mireUpdated) return;
        var dungeon = _tracker.World.MiseryMire;
        _tracker.PrerequisiteTracker.SetDungeonRequirement(dungeon, dungeon.PrerequisiteState.RequiredItem, null, true);
        _mireUpdated = true;
    }

    private void MarkTurtleRockMedallion()
    {
        if (_tracker == null || _turtleRockUpdated) return;
        var dungeon = _tracker.World.TurtleRock;
        _tracker.PrerequisiteTracker.SetDungeonRequirement(dungeon, dungeon.PrerequisiteState.RequiredItem, null, true);
        _turtleRockUpdated = true;
    }

}
