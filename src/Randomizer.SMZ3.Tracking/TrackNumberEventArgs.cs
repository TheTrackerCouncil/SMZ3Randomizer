using System;

namespace Randomizer.SMZ3.Tracking;

public class TrackNumberEventArgs : EventArgs
{
    public TrackNumberEventArgs(int trackNumber)
    {
        TrackNumber = trackNumber;
    }

    public int TrackNumber { get; set; }
}
