using Randomizer.Shared;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void TrackLocationEventHandler(MultiplayerPlayerState playerState, LocationId locationId);
