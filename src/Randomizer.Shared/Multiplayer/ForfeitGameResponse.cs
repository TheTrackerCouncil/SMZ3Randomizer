using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class ForfeitGameResponse : MultiplayerResponse
{
    public string? ForfeitPlayerGuid { get; init; } = null;

    public List<MultiplayerPlayerState>? AllPlayers { get; init; }

    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(ForfeitPlayerGuid);
}
