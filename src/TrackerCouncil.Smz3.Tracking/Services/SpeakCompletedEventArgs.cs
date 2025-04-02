using System;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Event for when the communicator has finished speaking
/// </summary>
/// <param name="speechDuration">How long the speech was going on for</param>
/// <param name="speechRequest">The speech request that is ending</param>
public class SpeakCompletedEventArgs(TimeSpan speechDuration, SpeechRequest? speechRequest) : EventArgs
{
    /// <summary>
    /// How long the speech was going on for
    /// </summary>
    public TimeSpan SpeechDuration => speechDuration;

    /// <summary>
    /// The speech request that is ending
    /// </summary>
    public SpeechRequest? SpeechRequest => speechRequest;
}
