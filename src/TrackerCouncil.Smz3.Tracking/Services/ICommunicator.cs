using System;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Defines a mechanism to communicate with the player.
/// </summary>
public interface ICommunicator
{
    /// <summary>
    /// Communicates the specified text to the player
    /// </summary>
    /// <param name="request">
    /// The request object containing details about what to communicate
    /// </param>
    void Say(SpeechRequest request);

    /// <summary>
    /// If the communicator is currently enabled
    /// </summary>
    /// <returns>True if enabled, false otherwise</returns>
    bool IsEnabled { get; }

    /// <summary>
    /// When overriden in an implementing class, it will enable communicating
    /// </summary>
    void Enable();

    /// <summary>
    /// When overriden in an implementing class, it will disable communicating
    /// </summary>
    void Disable();

    /// <summary>
    /// When overridden in an implementing class, aborts all ongoing
    /// communications.
    /// </summary>
    public void Abort() { }

    /// <summary>
    /// When overridden in an implementing class, uses an alternate voice
    /// when communicating with the player.
    /// </summary>
    public void UseAlternateVoice() { }

    /// <summary>
    /// When overridden in an implementing class, increases the
    /// communication rate.
    /// </summary>
    public void SpeedUp() { }

    /// <summary>
    /// When overridden in an implementing class, decreases the
    /// communication rate.
    /// </summary>
    public void SlowDown() { }

    /// <summary>
    /// If the TTS is currently speaking
    /// </summary>
    public bool IsSpeaking { get; }

    /// <summary>
    /// Updates the volume of the text to speech engine.
    /// </summary>
    /// <param name="volume">New volume between 0-100 with 0 being silent and 100 being the default max value.</param>
    public void UpdateVolume(int volume);

    /// <summary>
    /// Event for when the communicator has started speaking
    /// </summary>
    public event EventHandler SpeakStarted;

    /// <summary>
    /// Event for when the communicator has finished speaking
    /// </summary>
    public event EventHandler<SpeakCompletedEventArgs> SpeakCompleted;

    /// <summary>
    /// Event for when the communicator has reached a new viseme
    /// </summary>
    public event EventHandler<SpeakingUpdatedEventArgs> VisemeReached;


}
