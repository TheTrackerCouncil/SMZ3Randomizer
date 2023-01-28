using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void PlayerUpdatedEventHandler(MultiplayerPlayerState state, MultiplayerPlayerState? previousState, bool isLocalPlayer);
