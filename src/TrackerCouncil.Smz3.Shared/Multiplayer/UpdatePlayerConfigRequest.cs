namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class UpdatePlayerConfigRequest(string playerGuid, string config)
{
    public string PlayerGuid { get; } = playerGuid;

    public string Config { get; } = config;
}
