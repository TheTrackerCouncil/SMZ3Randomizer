using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.Data.Multiplayer;

public class PlayerJoinedResponse : MultiplayerResponse
{
    public string? PlayerGuid { get; init; }
    public string? PlayerName { get; init; }
    public List<MultiplayerPlayerState>? AllPlayers { get; init; }
    public bool IsValid => IsSuccessful && !string.IsNullOrEmpty(PlayerGuid) && !string.IsNullOrEmpty(PlayerName) && AllPlayers != null;
}
