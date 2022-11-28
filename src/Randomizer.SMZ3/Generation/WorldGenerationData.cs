using System.Collections.Generic;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Generation;

/// <summary>
/// Class for housing all of the details for generating a rom for a particular world
/// </summary>
public class WorldGenerationData
{
    public WorldGenerationData(World world, Dictionary<int, byte[]>? patches = null, IEnumerable<string>? hints = null)
    {
        World = world;
        Patches = patches ?? new Dictionary<int, byte[]>();
        Hints = hints ?? new List<string>();
    }

    public World World { get; }
    public Dictionary<int, byte[]> Patches { get; }
    public IEnumerable<string> Hints { get; }
    public Config Config => World.Config;
    public bool IsLocalWorld => World.IsLocalWorld;
}
