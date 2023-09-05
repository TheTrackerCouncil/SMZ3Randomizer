using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.FileData;

/// <summary>
/// Class used to house data utilized by the PatcherService
/// </summary>
public class GetPatchesRequest
{
    /// <summary>
    /// The current world the patches are being generated for
    /// </summary>
    public required World World { get; init; }

    /// <summary>
    /// All of the worlds in the current game
    /// </summary>
    public required List<World> Worlds { get; init; }

    /// <summary>
    /// Unique string guide to represent the game
    /// </summary>
    public required string SeedGuid { get; init; }

    /// <summary>
    /// The seed number used to generate the game
    /// </summary>
    public required int Seed { get; init; }

    /// <summary>
    /// Random generator for assigning details to the patches
    /// </summary>
    public required Random Random { get; init; }

    /// <summary>
    /// Hints to be added as hint tiles
    /// </summary>
    public IEnumerable<string> Hints { get; init; } = new List<string>();

    public const bool EnableMultiworld = true;
    public Config Config => World.Config;


}
