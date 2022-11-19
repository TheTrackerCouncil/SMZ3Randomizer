namespace Randomizer.Shared.Multiplayer;

public class SubmitConfigRequest : MultiplayerRequest
{
    public SubmitConfigRequest(string gameGuid, string playerGuid, string playerKey, string config)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
        Config = config;
    }

    public string Config { get; }

}
