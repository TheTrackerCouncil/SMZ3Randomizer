namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackDungeonResponse(string playerGuid, string dungeonName)
{
    public string PlayerGuid { get; init; } = playerGuid;
    public string DungeonName { get; init; } = dungeonName;
}
