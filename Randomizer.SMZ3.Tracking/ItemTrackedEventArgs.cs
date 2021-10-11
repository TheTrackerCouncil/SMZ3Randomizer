using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking
{
    public class ItemTrackedEventArgs : EventArgs
    {
        public ItemTrackedEventArgs(ItemType itemType, float confidence)
        {
            ItemType = itemType;
            Confidence = confidence;
        }

        public ItemType ItemType { get; }

        public float Confidence { get; }
    }
}
