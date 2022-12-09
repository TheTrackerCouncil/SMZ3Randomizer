namespace Randomizer.Shared.Multiplayer;

public class UpdatePlayerConfigRequest
{
    public UpdatePlayerConfigRequest(string playerGuid, string config)
    {
        PlayerGuid = playerGuid;
        Config = config;
    }

    public string PlayerGuid { get; }

    public string Config { get; }

}
