using TrackerCouncil.Smz3.Shared.Multiplayer;

namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

public delegate void PlayerUpdatedEventHandler(MultiplayerPlayerState state, MultiplayerPlayerState? previousState, bool isLocalPlayer);
