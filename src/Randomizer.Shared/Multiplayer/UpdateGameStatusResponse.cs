namespace Randomizer.Shared.Multiplayer;

public class UpdateGameStatusResponse : MultiplayerResponse
{
    public bool IsValid => IsSuccessful;
}
