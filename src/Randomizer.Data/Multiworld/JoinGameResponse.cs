using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.Data.Multiworld;

public class JoinGameResponse : MultiworldResponse
{
    public string? PlayerGuid { get; init; }
    public string? PlayerKey { get; init; }
    public List<MultiworldPlayerState>? AllPlayers { get; init; }

    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(PlayerGuid) && !string.IsNullOrEmpty(PlayerKey) && AllPlayers != null;
}
