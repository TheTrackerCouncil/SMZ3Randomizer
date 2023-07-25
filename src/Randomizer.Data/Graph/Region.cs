using System;
using System.Collections.Generic;

namespace Randomizer.Data.Graph
{
    public class Region
    {
        protected Region()
        {
            Layout = new List<(Exit, Exit)>();
            Rooms = new List<Room>();
        }

        public IEnumerable<(Exit, Exit)> Layout { get; protected set; }

        public IEnumerable<Room> Rooms { get; protected set; }
    }
}
