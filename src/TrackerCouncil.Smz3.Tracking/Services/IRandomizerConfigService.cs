using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Service for retrieving the randomizer generation config for the current world
/// </summary>
public interface IRandomizerConfigService
{
    /// <summary>
    /// Retrieves the config
    /// </summary>
    public Config Config { get; }
}
