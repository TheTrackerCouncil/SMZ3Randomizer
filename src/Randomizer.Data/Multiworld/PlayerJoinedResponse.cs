using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.Data.Multiworld;

public class PlayerJoinedResponse : MultiworldResponse
{
    public string PlayerGuid { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public Dictionary<string, string> AllPlayers { get; set; } = new();
}
