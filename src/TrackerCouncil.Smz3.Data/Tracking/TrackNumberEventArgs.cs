using System;

namespace TrackerCouncil.Smz3.Data.Tracking;

public class TrackNumberEventArgs : EventArgs
{
    public TrackNumberEventArgs(int trackNumber)
    {
        TrackNumber = trackNumber;
    }

    public int TrackNumber { get; set; }
}
