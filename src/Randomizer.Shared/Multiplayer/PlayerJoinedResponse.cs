using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class PlayerJoinedResponse : MultiplayerResponse
{
    public string? PlayerGuid { get; init; }
    public string? PlayerName { get; init; }
    public List<MultiplayerPlayerState>? AllPlayers { get; init; }
    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(PlayerGuid) && !string.IsNullOrEmpty(PlayerName) && AllPlayers != null;
}
