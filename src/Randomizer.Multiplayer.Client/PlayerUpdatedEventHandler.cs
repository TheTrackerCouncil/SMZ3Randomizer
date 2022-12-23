using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerUpdatedEventHandler(MultiplayerPlayerState state, MultiplayerPlayerState? previousState, bool isLocalPlayer);
