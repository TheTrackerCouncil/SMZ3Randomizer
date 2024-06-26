using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Service for retrieving information about the current state of
/// the world
/// </summary>
public class RandomizerConfigService : IRandomizerConfigService
{
    private readonly IWorldAccessor _world;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="worldAccessor"></param>
    public RandomizerConfigService(IWorldAccessor worldAccessor)
    {
        _world = worldAccessor;
    }

    /// <summary>
    /// Retrieves the config of the current world
    /// </summary>
    public Config Config => _world.World.Config;
}
