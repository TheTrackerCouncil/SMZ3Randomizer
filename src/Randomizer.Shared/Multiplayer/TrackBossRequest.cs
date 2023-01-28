using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Multiplayer;

public class TrackBossRequest
{
    public TrackBossRequest(string playerGuid, BossType bossType)
    {
        PlayerGuid = playerGuid;
        BossType = bossType;
    }

    public string PlayerGuid { get; }
    public BossType BossType { get; }
}
