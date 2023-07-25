using System;
using System.Collections.Generic;

namespace Randomizer.Data.Graph
{
    public class Area
    {
        protected Area()
        {
            Layout = new List<(Exit, Exit)>();
            Regions = new List<Region>();
        }

        public IEnumerable<(Exit, Exit)> Layout { get; protected set; }

        public IEnumerable<Region> Regions { get; protected set; }
    }
}
