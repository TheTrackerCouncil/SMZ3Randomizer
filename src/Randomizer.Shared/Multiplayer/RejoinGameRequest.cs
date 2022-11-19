namespace Randomizer.Shared.Multiplayer;

public class RejoinGameRequest : MultiplayerRequest
{
    public RejoinGameRequest(string gameGuid, string playerGuid, string playerKey)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
    }
}
