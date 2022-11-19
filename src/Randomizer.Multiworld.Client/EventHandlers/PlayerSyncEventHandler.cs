using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void PlayerSyncEventHandler(MultiplayerPlayerState? previousState, MultiplayerPlayerState state);

