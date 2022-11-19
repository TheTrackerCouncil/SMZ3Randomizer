using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class CreateGameResponse : MultiplayerResponse
{
    public string? GameGuid { get; init; }
    public string? PlayerGuid { get; init; }
    public string? PlayerKey { get; init; }
    public List<MultiplayerPlayerState>? AllPlayers { get; init; }
    public string? GameUrl { get; init; }

    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(GameGuid) && !string.IsNullOrEmpty(PlayerGuid) &&
                           !string.IsNullOrEmpty(PlayerKey) && AllPlayers != null;
}
