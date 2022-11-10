using System.Collections.Generic;
using Randomizer.Data.WorldData;

namespace Randomizer.Data.Multiworld;

public class StartGameResponse : MultiworldResponse
{
    public List<MultiworldPlayerState>? Players { get; set; }
}
