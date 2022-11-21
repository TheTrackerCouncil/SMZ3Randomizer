namespace Randomizer.Shared.Multiplayer;

public class UpdatePlayerStateRequest : MultiplayerRequest
{
    public UpdatePlayerStateRequest(string gameGuid, string playerGuid, string playerKey, MultiplayerPlayerState state, bool propogate = true)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
        State = state;
        Propogate = propogate;
    }

    public MultiplayerPlayerState State { get; }

    public bool Propogate { get; }

}
