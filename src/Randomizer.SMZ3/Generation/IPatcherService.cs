using System;
using System.Collections.Generic;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3.FileData;

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
    /// <param name="request">Request with required world and seed data to generate the patches</param>
    Dictionary<int, byte[]> GetPatches(GetPatchesRequest request);
}
