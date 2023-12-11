using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.Data.WorldData;

public class VisibleItemZelda
{
    public int? Room { get; set; }
    public int? OverworldScreen { get; set; }
    public List<VisibleItemArea> Areas { get; set; } = new();
}
