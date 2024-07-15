using System;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Event for when the communicator has finished speaking
/// </summary>
/// <param name="speechDuration">How long the speech was going on for</param>
public class SpeakCompletedEventArgs(TimeSpan speechDuration) : EventArgs
{
    /// <summary>
    /// How long the speech was going on for
    /// </summary>
    public TimeSpan SpeechDuration => speechDuration;
}
