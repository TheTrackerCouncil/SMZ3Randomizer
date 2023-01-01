using Randomizer.Shared.Enums;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void TrackBossEventHandler(MultiplayerPlayerState playerState, BossType bossType);
