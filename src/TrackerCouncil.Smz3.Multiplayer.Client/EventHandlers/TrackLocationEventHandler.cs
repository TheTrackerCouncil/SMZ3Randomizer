using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Multiplayer;

namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

public delegate void TrackLocationEventHandler(MultiplayerPlayerState playerState, LocationId locationId);
