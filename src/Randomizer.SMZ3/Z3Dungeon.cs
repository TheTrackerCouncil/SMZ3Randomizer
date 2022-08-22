using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Randomizer.SMZ3
{

    public abstract class Z3Dungeon : Z3Region {

        public Z3Dungeon(World world, Config config)
            : base(world, config) { }

        public Z3Dungeon(World world, Config config, int? memoryAddress, int? memoryFlag, ICollection<int> startingRooms)
            : base(world, config)
        {
            MemoryAddress = memoryAddress;
            MemoryFlag = memoryFlag;
            StartingRooms = startingRooms;
        }

    }

}
