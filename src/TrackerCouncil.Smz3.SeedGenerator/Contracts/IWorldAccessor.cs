using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.WorldData;

namespace TrackerCouncil.Smz3.SeedGenerator.Contracts;

/// <summary>
/// Provides access to the current <see cref="Data.WorldData.World"/>.
/// </summary>
public interface IWorldAccessor
{
    /// <summary>
    /// Gets or sets the current <see cref="Data.WorldData.World"/>, if one is
    /// available.
    /// </summary>
    World World { get; set; }

    /// <summary>
    /// List of all the worlds
    /// </summary>
    List<World> Worlds { get; set; }
}
