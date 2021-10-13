﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking
{
    public class ItemTrackedEventArgs : EventArgs
    {
        public ItemTrackedEventArgs(ItemData itemData, string trackedAs, float confidence)
        {
            Item = itemData;
            TrackedAs = trackedAs;
            Confidence = confidence;
        }

        public string TrackedAs { get; }

        public ItemData Item { get; }

        public float Confidence { get; }
    }
}