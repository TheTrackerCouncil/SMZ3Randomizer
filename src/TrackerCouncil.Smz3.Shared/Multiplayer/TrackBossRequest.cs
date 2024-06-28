using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackBossRequest(string playerGuid, BossType bossType)
{
    public string PlayerGuid { get; } = playerGuid;
    public BossType BossType { get; } = bossType;
}
