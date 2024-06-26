using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Multiplayer;

namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

public delegate void TrackItemEventHandler(MultiplayerPlayerState playerState, ItemType itemType, int trackingValue);
