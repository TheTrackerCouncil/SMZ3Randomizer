namespace Randomizer.Shared.Multiplayer;

public class UpdateGameStateRequest
{
    public UpdateGameStateRequest(MultiplayerGameStatus? gameStatus)
    {
        GameStatus = gameStatus;
    }

    public MultiplayerGameStatus? GameStatus { get; }

}
