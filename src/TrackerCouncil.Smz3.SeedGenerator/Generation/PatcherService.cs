﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.SeedGenerator.FileData;
using TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

/// <summary>
/// Service for getting the patches that need to be applied to a rom
/// </summary>
public class PatcherService : IPatcherService
{
    private readonly ILogger<PatcherService> _logger;
    private readonly RomPatchFactory _romPatchFactory;

    public PatcherService(RomPatchFactory romPatchFactory, ILogger<PatcherService> logger)
    {
        _logger = logger;
        _romPatchFactory = romPatchFactory;
    }

    public Dictionary<int, byte[]> GetPatches(GetPatchesRequest data)
    {
        var patches = new List<GeneratedPatch>();

        foreach (var patch in _romPatchFactory.GetPatches(data.IsParsedRom))
        {
            var updates = patch.GetChanges(data).ToList();
            _logger.LogInformation("Retrieving {Number} updates from {Name}", updates.Count, patch.GetType().Name);
            patches.AddRange(updates);
        }

        return patches.ToDictionary(x => x.Offset, x => x.Data);
    }

}
