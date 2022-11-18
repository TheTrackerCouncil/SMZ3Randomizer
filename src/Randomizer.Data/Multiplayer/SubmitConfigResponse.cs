namespace Randomizer.Data.Multiplayer;

public class SubmitConfigResponse : MultiplayerResponse
{
    public bool IsValid => IsSuccessful;
}
