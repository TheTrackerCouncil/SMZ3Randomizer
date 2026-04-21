using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Abstractions;

public abstract class GameModeBase
{
    public abstract bool IsComplete(World world);

    public abstract void UpdateWorld(World world, Random rng, GameModeOptions gameModeOptions);

    public abstract void UpdateInitialTrackerState(GameModeOptions gameModeOptions, TrackerState trackerState, ParsedRomDetails? parsedRomDetails);

    public virtual void InitializeTracker(TrackerBase tracker) {}

    public abstract GameModeInGameText GetInGameText(World world);

    public virtual string? GetGameStartText(World world)
    {
        return null;
    }

    public abstract bool IsKnowinglyComplete(World world);


    public virtual bool OnViewingPyramidText(World world)
    {
        return false;
    }

    public virtual List<Location>? GetGameModeLocations(World world, List<World> allWorlds)
    {
        return null;
    }

    public abstract List<GoalUiDetails> GetGoalUiDetails(World world, Progression progression);

    public abstract string GetSpoilerText(GameModeOptions gameModeOptions);

    public abstract Stream GetLiftOffOnGoalCompletionIpsPatch();
}
