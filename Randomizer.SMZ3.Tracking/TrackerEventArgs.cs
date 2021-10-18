using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking
{
    public class TrackerEventArgs : EventArgs
    {
        public TrackerEventArgs(float? confidence)
        {
            Confidence = confidence;
        }

        public float? Confidence { get; }
    }
}
