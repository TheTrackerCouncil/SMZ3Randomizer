using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.Data.Multiworld;

public class RejoinGameResponse : MultiworldResponse
{
    public string? PlayerGuid { get; init; }
    public string? PlayerKey { get; init; }
    public List<MultiworldPlayerState>? AllPlayers { get; init; }
    public string? GameUrl { get; init; }

    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(PlayerGuid) && !string.IsNullOrEmpty(PlayerKey) && AllPlayers != null;
}
