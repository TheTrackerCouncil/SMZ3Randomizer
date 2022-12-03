namespace Randomizer.Shared.Multiplayer;

public class TrackDungeonResponse
{
    public TrackDungeonResponse(string playerGuid, string dungeonName)
    {
        PlayerGuid = playerGuid;
        DungeonName = dungeonName;
    }

    public string PlayerGuid { get; init; }
    public string DungeonName { get; init; }
}
