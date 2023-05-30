using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Multiplayer;

public class TrackDeathRequest
{
    public TrackDeathRequest(string playerGuid)
    {
        PlayerGuid = playerGuid;
    }

    public string PlayerGuid { get; }
}
