using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerAltGameModeService
{
    public GameModeType GameModeType { get; }
    public bool HasAltGameMode { get; }
    public bool IsAltGameModeComplete { get; }
    public event EventHandler? GoalStateChanged;
    public void MarkAltGameModeAsComplete();
    public bool IsAltGameModeKnowinglyComplete();
    public void OnViewingPyramidText();
    public void NotifyOfGoalStateChange();
    public List<GoalUiDetails> GetGoalUiDetails();
}
