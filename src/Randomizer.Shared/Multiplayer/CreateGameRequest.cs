namespace Randomizer.Shared.Multiplayer;

public class CreateGameRequest
{
    public CreateGameRequest(string playerName, string phoneticName, MultiplayerGameType gameType, string randomizerVersion, int multiplayerVersion, bool saveToDatabase, bool sendItemsOnComplete, bool deathLink)
    {
        PlayerName = playerName;
        PhoneticName = phoneticName;
        GameType = gameType;
        RandomizerVersion = randomizerVersion;
        MultiplayerVersion = multiplayerVersion;
        SaveToDatabase = saveToDatabase;
        SendItemsOnComplete = sendItemsOnComplete;
        DeathLink = deathLink;
    }

    public string PlayerName { get; }

    public string PhoneticName { get; }

    public MultiplayerGameType GameType { get; }

    public string RandomizerVersion { get; }

    public int MultiplayerVersion { get; }

    public bool SaveToDatabase { get; }

    public bool SendItemsOnComplete { get; }

    public bool DeathLink;
}
