namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class ErrorResponse(string error)
{
    public string Error { get; } = error;
}
