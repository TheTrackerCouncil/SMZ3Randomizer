using System;
using System.Collections.Generic;

namespace Randomizer.Data.Graph
{
    public class Room
    {
        public Room(Dictionary<Exit, ExitType> exits, String name, IEnumerable<Location>? locations = null)
        {
            Exits = exits;
            Locations = locations ?? new List<Location>();
            Name = name;
        }

        public Dictionary<Exit, ExitType> Exits { get; protected set; }

        public IEnumerable<Location> Locations { get; protected set; }

        public String Name { get; protected set; }
    }
}
