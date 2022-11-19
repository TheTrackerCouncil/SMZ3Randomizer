using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class UpdateGameStatusRequest : MultiplayerRequest
{
    public UpdateGameStatusRequest(string gameGuid, string playerGuid, string playerKey, MultiplayerGameStatus gameStatus)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
        GameStatus = gameStatus;
    }

    public MultiplayerGameStatus GameStatus { get; }

}
