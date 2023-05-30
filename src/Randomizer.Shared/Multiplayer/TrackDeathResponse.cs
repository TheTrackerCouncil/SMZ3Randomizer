using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Multiplayer;

public class TrackDeathResponse
{
    public TrackDeathResponse(string playerguid)
    {
        PlayerGuid = playerguid;
    }

    public string PlayerGuid { get; init; }
}
