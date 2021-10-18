using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking
{
    public class ItemTrackedEventArgs : TrackerEventArgs
    {
        public ItemTrackedEventArgs(ItemData itemData, string? trackedAs, float? confidence)
            : base(confidence)
        {
            Item = itemData;
            TrackedAs = trackedAs;
        }

        public string? TrackedAs { get; }

        public ItemData Item { get; }
    }
}
