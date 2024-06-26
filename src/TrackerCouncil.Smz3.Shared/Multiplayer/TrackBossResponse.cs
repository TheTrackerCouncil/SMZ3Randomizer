using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackBossResponse(string playerguid, BossType bossType)
{
    public string PlayerGuid { get; init; } = playerguid;
    public BossType BossType { get; init; } = bossType;
}
