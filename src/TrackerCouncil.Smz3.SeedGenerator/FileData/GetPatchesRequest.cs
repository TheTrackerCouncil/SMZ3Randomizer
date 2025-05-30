﻿using System;
using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData;

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
    /// The PlandoConfig being used to specify data about the world
    /// </summary>
    public PlandoConfig PlandoConfig { get; init; } = new();

    /// <summary>
    /// If Multiworld patches should be enabled
    /// This is defaulted to true (at least for now) so tracker can give hints
    /// </summary>
    public const bool EnableMultiworld = true;

    /// <summary>
    /// The local world's config
    /// </summary>
    public Config Config => World.Config;

    /// <summary>
    /// If this is patching a rom that was generated externally
    /// </summary>
    public bool IsParsedRom { get; init; }

    /// <summary>
    /// Parsed text from a prior rom file
    /// </summary>
    public List<byte[]>? PreviousParsedText { get; init; }

}
