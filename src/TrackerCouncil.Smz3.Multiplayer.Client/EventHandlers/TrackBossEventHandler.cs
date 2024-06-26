using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Multiplayer;

namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

public delegate void TrackBossEventHandler(MultiplayerPlayerState playerState, BossType bossType);
