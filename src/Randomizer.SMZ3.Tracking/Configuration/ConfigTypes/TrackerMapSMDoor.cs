using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    public class TrackerMapSMDoor
    {
        public TrackerMapSMDoor(string item, int x, int y)
        {
            Item = item;
            X = x;
            Y = y;
        }

        public string Item { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
