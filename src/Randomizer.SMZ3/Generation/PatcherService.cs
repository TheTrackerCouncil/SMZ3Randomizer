using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.SMZ3.FileData;
using Randomizer.SMZ3.FileData.Patches;
using Randomizer.SMZ3.GameModes;

namespace Randomizer.SMZ3.Generation;

/// <summary>
/// Service for getting the patches that need to be applied to a rom
/// </summary>
public class PatcherService : IPatcherService
{
    private readonly ILogger<PatcherService> _logger;
    private readonly RomPatchFactory _romPatchFactory;
    private readonly IGameModeService _gameModeService;

    public PatcherService(RomPatchFactory romPatchFactory, ILogger<PatcherService> logger, IGameModeService gameModeService)
    {
        _logger = logger;
        _gameModeService = gameModeService;
        _romPatchFactory = romPatchFactory;
    }

    public Dictionary<int, byte[]> GetPatches(GetPatchesRequest data)
    {
        var patches = new Dictionary<int, byte[]>();

        foreach (var patch in _romPatchFactory.GetPatches().SelectMany(x => x.GetChanges(data)))
        {
            patches[patch.Offset] = patch.Data;
        }

        foreach (var patch in _gameModeService.GetPatches(data.World).SelectMany(x => x.GetChanges(data)))
        {
            patches[patch.Offset] = patch.Data;
        }

        return patches;
    }

}
