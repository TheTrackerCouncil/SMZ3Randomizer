using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.Data.Multiworld;

public class JoinGameResponse : MultiworldResponse
{
    public string PlayerGuid { get; set; } = "";
    public string PlayerKey { get; set; } = "";
    public Dictionary<string, string> AllPlayers { get; set; } = new();
}
