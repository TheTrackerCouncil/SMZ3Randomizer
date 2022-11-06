using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Randomizer.Data.Configuration;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions
{

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

}
