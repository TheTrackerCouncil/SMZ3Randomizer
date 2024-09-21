using System;
using System.Speech.Synthesis;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Event for when tracker says a new line
/// </summary>
/// <param name="visemeDetails">The original viseme details</param>
/// <param name="request">The original speech request</param>
public class SpeakVisemeReachedEventArgs(VisemeReachedEventArgs visemeDetails, SpeechRequest? request) : EventArgs
{
    /// <summary>
    /// Original viseme details
    /// </summary>
    public VisemeReachedEventArgs VisemeDetails => visemeDetails;

    /// <summary>
    /// The original speech request
    /// </summary>
    public SpeechRequest? Request => request;
}
