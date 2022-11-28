using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Generation;

/// <summary>
/// Class for generating a series of worlds
/// </summary>
public class WorldGenerationDataCollection : List<WorldGenerationData>
{
    /// <summary>
    /// Retrieves the generation data for the local player's world
    /// </summary>
    public WorldGenerationData LocalWorld => this.Single(x => x.IsLocalWorld);

    /// <summary>
    /// Retrieves a collection of all of the worlds that were generated
    /// </summary>
    public IEnumerable<World> Worlds => this.Select(x => x.World);

    /// <summary>
    /// Retrieves a particular world by player guid
    /// </summary>
    /// <param name="guid">The guid for the player to look up</param>
    /// <returns>The world matching the player</returns>
    public WorldGenerationData GetWorld(string guid) => this.Single(x => x.World.Guid == guid);
}
