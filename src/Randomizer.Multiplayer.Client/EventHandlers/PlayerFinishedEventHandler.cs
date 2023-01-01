using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void PlayerFinishedEventHandler(MultiplayerPlayerState state, bool isLocalPlayer);
