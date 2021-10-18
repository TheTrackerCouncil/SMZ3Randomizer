using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking
{
    public class ItemTrackedEventArgs : TrackerEventArgs
    {
        public ItemTrackedEventArgs(string? trackedAs, float? confidence)
            : base(confidence)
        {
            TrackedAs = trackedAs;
        }

        public string? TrackedAs { get; }
    }
}
