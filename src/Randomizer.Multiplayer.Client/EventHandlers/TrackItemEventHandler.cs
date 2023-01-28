using Randomizer.Shared;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void TrackItemEventHandler(MultiplayerPlayerState playerState, ItemType itemType, int trackingValue);
