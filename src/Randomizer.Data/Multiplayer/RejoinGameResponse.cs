using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.Data.Multiplayer;

public class RejoinGameResponse : MultiplayerResponse
{
    public string? PlayerGuid { get; init; }
    public string? PlayerKey { get; init; }
    public List<MultiplayerPlayerState>? AllPlayers { get; init; }
    public string? GameUrl { get; init; }

    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(PlayerGuid) && !string.IsNullOrEmpty(PlayerKey) && AllPlayers != null;
}
