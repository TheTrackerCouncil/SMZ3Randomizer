using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Abstractions;

public abstract class AltGameModeBase
{
    public abstract bool IsComplete(World world);

    public virtual void UpdateWorld(World world, int seed, GameModeOptions gameModeOptions) {}

    public virtual void UpdateInitialTrackerState(GameModeOptions gameModeOptions, TrackerState trackerState) {}

    public virtual void InitializeTracker(TrackerBase tracker) {}

    public virtual AltGameModeInGameText? GetInGameText(World world)
    {
        return null;
    }

    public virtual string? GetGameStartText(World world)
    {
        return null;
    }

    public virtual bool IsKnowinglyComplete(World world)
    {
        return false;
    }

    public virtual bool OnViewingPyramidText(World world)
    {
        return false;
    }

    public virtual List<Location>? GetGameModeLocations(World world, List<World> allWorlds)
    {
        return null;
    }

    public virtual List<GoalUiDetails>? GetGoalUiDetails(World world)
    {
        return null;
    }

    public virtual string GetSpoilerText(GameModeOptions gameModeOptions)
    {
        return string.Empty;
    }
}
