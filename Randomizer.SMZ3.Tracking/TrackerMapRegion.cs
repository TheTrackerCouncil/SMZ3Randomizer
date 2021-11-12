using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking
{
    public class TrackerMapRegion
    {
        public TrackerMapRegion ( string name, IReadOnlyCollection<TrackerMapLocation> rooms )
        {
            Name = name;
            Rooms = rooms;
        }

        public string Name { get; }

        public IReadOnlyCollection<TrackerMapLocation> Rooms { get; }
    }
}
