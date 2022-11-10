namespace Randomizer.Data.Multiworld;

public class CreateGameRequest : MultiworldRequest
{
    public CreateGameRequest(string playerName)
    {
        PlayerName = playerName;
    }

    public string PlayerName { get; }
}
