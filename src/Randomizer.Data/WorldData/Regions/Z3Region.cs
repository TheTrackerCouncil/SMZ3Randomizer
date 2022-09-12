using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;

namespace Randomizer.Data.WorldData.Regions
{

    public abstract class Z3Region : Region
    {
        public Z3Region(World world, Config config)
            : base(world, config) { }

        public Z3Region(World world, Config config, int? memoryAddress, int? memoryFlag, ICollection<int> startingRooms)
            : base(world, config)
        {
            MemoryAddress = memoryAddress;
            MemoryFlag = memoryFlag;
            StartingRooms = startingRooms;
        }

        public bool IsOverworld { get; init; }

        public int? MemoryAddress { get; init; }

        public int? MemoryFlag { get; init; }

        public ICollection<int> StartingRooms { get; init; }
    }

}
