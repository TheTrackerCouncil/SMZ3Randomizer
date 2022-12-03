namespace Randomizer.Shared.Multiplayer;

public class TrackDungeonRequest
{
    public TrackDungeonRequest(string playerGuid, string dungeonName)
    {
        PlayerGuid = playerGuid;
        DungeonName = dungeonName;
    }

    public string PlayerGuid { get; }
    public string DungeonName { get; }
}
