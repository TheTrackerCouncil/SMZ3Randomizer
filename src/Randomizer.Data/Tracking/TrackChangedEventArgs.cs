using System;
using MSURandomizerLibrary.Configs;

namespace Randomizer.Data.Tracking;

public class TrackChangedEventArgs : EventArgs
{
    public TrackChangedEventArgs(Msu msu, Track track, string outputText)
    {
        Msu = msu;
        Track = track;
        OutputText = outputText;
    }

    public Msu Msu { get; set; }
    public Track Track { get; set; }
    public string OutputText { get; set; }
}
