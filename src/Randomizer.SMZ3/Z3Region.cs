using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Randomizer.SMZ3
{

    public abstract class Z3Region : Region {
        public Z3Region(World world, Config config)
            : base(world, config) { }

        public Z3Region(World world, Config config, int? memoryAddress, int? memoryFlag, ICollection<int> startingRooms)
            : base(world, config)
        {
            MemoryAddress = memoryAddress;
            MemoryFlag = memoryFlag;
            StartingRooms = startingRooms;
        }

        public int? MemoryAddress { get; init; }

        public int? MemoryFlag { get; init; }

        public ICollection<int> StartingRooms { get; init; }
    }

}
