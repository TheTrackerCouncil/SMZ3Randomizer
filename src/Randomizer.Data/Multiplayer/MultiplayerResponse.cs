namespace Randomizer.Data.Multiplayer;

public class MultiplayerResponse
{
    public MultiplayerGameStatus GameStatus { get; set; }
    public bool IsSuccessful { get; set; }
    public string Error { get; set; } = "Unknown Error";
}
