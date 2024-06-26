namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class StartGameRequest(string validationHash)
{
    public string ValidationHash { get; } = validationHash;
}
