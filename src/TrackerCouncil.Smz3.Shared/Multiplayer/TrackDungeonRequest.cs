namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackDungeonRequest(string playerGuid, string dungeonName)
{
    public string PlayerGuid { get; } = playerGuid;
    public string DungeonName { get; } = dungeonName;
}
