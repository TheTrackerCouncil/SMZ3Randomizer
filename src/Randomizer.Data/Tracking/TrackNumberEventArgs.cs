using System;

namespace Randomizer.Data.Tracking;

public class TrackNumberEventArgs : EventArgs
{
    public TrackNumberEventArgs(int trackNumber)
    {
        TrackNumber = trackNumber;
    }

    public int TrackNumber { get; set; }
}
