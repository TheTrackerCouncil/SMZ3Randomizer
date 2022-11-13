using System.Collections.Generic;

namespace Randomizer.Data.Multiworld;

public class CreateGameResponse : MultiworldResponse
{
    public string? GameGuid { get; init; }
    public string? PlayerGuid { get; init; }
    public string? PlayerKey { get; init; }
    public List<MultiworldPlayerState>? AllPlayers { get; init; }

    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(GameGuid) && !string.IsNullOrEmpty(PlayerGuid) &&
                           !string.IsNullOrEmpty(PlayerKey) && AllPlayers != null;
}
