using System.Collections.Generic;
using TrackerCouncil.Smz3.SeedGenerator.FileData;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

/// <summary>
/// Service for getting the patches that need to be applied to a rom
/// </summary>
public interface IPatcherService
{
    /// <summary>
    /// Retrieves the patches that need to be applied to a rom to apply the generated world data and requested user
    /// settings.
    /// </summary>
    /// <param name="request">Request with required world and seed data to generate the patches</param>
    Dictionary<int, byte[]> GetPatches(GetPatchesRequest request);
}
