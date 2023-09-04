using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Configuration;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3.FileData;
using Randomizer.SMZ3.FileData.Patches;

namespace Randomizer.SMZ3.Generation;

/// <summary>
/// Service for getting the patches that need to be applied to a rom
/// </summary>
public class PatcherService : IPatcherService
{
    private readonly ILogger<PatcherService> _logger;
    private readonly RomPatchFactory _romPatchFactory;
    private readonly Configs _configs;

    public PatcherService(Configs configs, ILogger<PatcherService> logger)
    {
        _configs = configs;
        _logger = logger;
        _romPatchFactory = new RomPatchFactory();
    }

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
    /// <param name="plandoConfig">Plando configuration</param>
    /// <returns>The memory locations and overwrite data for all of the patches to apply to the rom</returns>
    public Dictionary<int, byte[]> GetPatches(World localWorld, List<World> worlds, string seedGuid, int seed, Random random, IEnumerable<string>? hints = null, PlandoConfig? plandoConfig = null)
    {
        hints ??= new List<string>();

        return GetPatches(new PatcherServiceData()
        {
            LocalWorld = localWorld,
            Worlds = worlds,
            SeedGuid = seedGuid,
            Seed = seed,
            Random = random,
            Hints = hints,
            Configs = _configs,
            PlandoConfig = plandoConfig
        });
    }

    private Dictionary<int, byte[]> GetPatches(PatcherServiceData data)
    {
        var patches = new List<GeneratedPatch>();

        foreach (var patch in _romPatchFactory.GetPatches())
        {
            var updates = patch.GetChanges(data);
            _logger.LogInformation("Retrieving {Number} updates from {Name}", updates.Count(), patch.GetType().Name);
            patches.AddRange(updates);
        }

        return patches.ToDictionary(x => x.Offset, x => x.Data);
    }

}
