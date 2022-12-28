namespace Randomizer.Shared.Multiplayer;

public class ErrorResponse
{
    public ErrorResponse(string error)
    {
        Error = error;
    }

    public string Error { get; }
}
