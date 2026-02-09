using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerGameModeService
{
    public GameModeType GetCurrentGameModeType();
}
