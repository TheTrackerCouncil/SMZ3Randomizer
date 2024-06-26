using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions;

public abstract class Z3Region : Region
{
    public Z3Region(World world, Config config, IMetadataService? metadata, TrackerState? trackerState)
        : base(world, config, metadata, trackerState) { }

    public Z3Region(World world, Config config, IMetadataService? metadata, TrackerState? trackerState, int? memoryAddress, int? memoryFlag, ICollection<int> startingRooms)
        : base(world, config, metadata, trackerState)
    {
        MemoryAddress = memoryAddress;
        MemoryFlag = memoryFlag;
        StartingRooms = startingRooms;
    }

    public bool IsOverworld { get; init; }

    public int? MemoryAddress { get; init; }

    public int? MemoryFlag { get; init; }

    public ICollection<int> StartingRooms { get; init; } = new List<int>();
}
