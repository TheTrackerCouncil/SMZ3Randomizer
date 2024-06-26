namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class CreateGameRequest(
    string playerName,
    string phoneticName,
    MultiplayerGameType gameType,
    string randomizerVersion,
    int multiplayerVersion,
    bool saveToDatabase,
    bool sendItemsOnComplete,
    bool deathLink)
{
    public string PlayerName { get; } = playerName;

    public string PhoneticName { get; } = phoneticName;

    public MultiplayerGameType GameType { get; } = gameType;

    public string RandomizerVersion { get; } = randomizerVersion;

    public int MultiplayerVersion { get; } = multiplayerVersion;

    public bool SaveToDatabase { get; } = saveToDatabase;

    public bool SendItemsOnComplete { get; } = sendItemsOnComplete;

    public bool DeathLink { get; } = deathLink;
}
