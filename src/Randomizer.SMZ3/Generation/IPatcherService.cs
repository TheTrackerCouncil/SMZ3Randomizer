using System;
using System.Collections.Generic;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Generation;

/// <summary>
/// Service for getting the patches that need to be applied to a rom
/// </summary>
public interface IPatcherService
{
    /// <summary>
    /// Retrieves the patches that need to be applied to a rom to apply the generated world data and requested user
    /// settings.
    /// </summary>
    /// <param name="localWorld">The world of the local player</param>
    /// <param name="worlds">All worlds in the game</param>
    /// <param name="seedGuid">The string guid for the seed</param>
    /// <param name="seed">The seed number</param>
    /// <param name="random">The random generator to be used for determining various patches</param>
    /// <param name="hints">The list of hints to use for hint tiles</param>
    /// <returns>The memory locations and overwrite data for all of the patches to apply to the rom</returns>
    Dictionary<int, byte[]> GetPatches(World localWorld, List<World> worlds, string seedGuid, int seed,
        Random random, IEnumerable<string>? hints = null);
}
