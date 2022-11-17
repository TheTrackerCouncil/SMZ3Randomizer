using System.Collections.Generic;

namespace Randomizer.Data.Multiworld;

public class ForfeitGameResponse : MultiworldResponse
{
    public string? ForfeitPlayerGuid { get; init; } = null;

    public List<MultiworldPlayerState>? AllPlayers { get; init; }

    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(ForfeitPlayerGuid);
}
