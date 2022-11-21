using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class RejoinGameResponse : MultiplayerResponse
{
    public RejoinGameResponse(MultiplayerGameState gameState, string playerGuid, string playerKey, List<MultiplayerPlayerState> playerStates) : base(gameState)
    {
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
        AllPlayers = playerStates;
    }

    public string PlayerGuid { get; }
    public string PlayerKey { get; }
    public List<MultiplayerPlayerState> AllPlayers { get; }


}
