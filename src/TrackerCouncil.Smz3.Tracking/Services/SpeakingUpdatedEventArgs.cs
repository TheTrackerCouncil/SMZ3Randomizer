using System;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Event for when tracker says a new line
/// </summary>
/// <param name="isSpeaking">If the TTS service is currently speaking</param>
/// <param name="request">The original speech request</param>
public class SpeakingUpdatedEventArgs(bool isSpeaking, SpeechRequest? request) : EventArgs
{
    /// <summary>
    /// If the TTS service is currently speaking
    /// </summary>
    public bool IsSpeaking => isSpeaking;

    /// <summary>
    /// The original speech request
    /// </summary>
    public SpeechRequest? Request => request;
}
