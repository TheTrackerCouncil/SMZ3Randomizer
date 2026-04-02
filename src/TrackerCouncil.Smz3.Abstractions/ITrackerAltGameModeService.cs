using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerAltGameModeService
{
    public GameModeType GameModeType { get; }
    public bool HasAltGameMode { get; }
    public bool IsAltGameModeComplete { get; }
    public void MarkAltGameModeAsComplete();
    public bool IsAltGameModeKnowinglyComplete();
    public void OnViewingPyramidText();
}
