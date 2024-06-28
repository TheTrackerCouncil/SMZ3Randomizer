namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class UpdateGameStateRequest(MultiplayerGameStatus? gameStatus)
{
    public MultiplayerGameStatus? GameStatus { get; } = gameStatus;
}
