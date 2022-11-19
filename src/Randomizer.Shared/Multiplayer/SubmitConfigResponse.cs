namespace Randomizer.Shared.Multiplayer;

public class SubmitConfigResponse : MultiplayerResponse
{
    public bool IsValid => IsSuccessful;
}
