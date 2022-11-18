using Randomizer.Data.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerSyncEventHandler(MultiplayerPlayerState? previousState, MultiplayerPlayerState state);

