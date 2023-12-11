using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.Data.WorldData;

public class VisibleItemMetroid
{
    public int Room { get; set; }
    public List<VisibleItemArea> Areas { get; set; } = new();
}
