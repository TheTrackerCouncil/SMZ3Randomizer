using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Multiplayer;

public class TrackBossResponse
{
    public TrackBossResponse(string playerguid, BossType bossType)
    {
        PlayerGuid = playerguid;
        BossType = bossType;
    }

    public string PlayerGuid { get; init; }
    public BossType BossType { get; init; }
}
