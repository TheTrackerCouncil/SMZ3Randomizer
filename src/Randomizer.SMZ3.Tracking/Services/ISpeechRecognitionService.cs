using System;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.Services;

/// <summary>
/// Service for recognizing speech from a microphone
/// </summary>
public abstract class SpeechRecognitionServiceBase
{
    /// <summary>
    /// Event fired when speech was successfully understood
    /// </summary>
    public event System.EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;


    protected virtual void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs args)
    {
        if (OperatingSystem.IsWindows())
        {
            SpeechRecognized?.Invoke(sender, args);
        }

    }

    /// <summary>
    /// Updates the microphone to be the default Windows mic
    /// </summary>
    public abstract void SetInputToDefaultAudioDevice();

    /// <summary>
    /// The internal speech recognition engine used by the service, if applicable
    /// </summary>
    public abstract SpeechRecognitionEngine? RecognitionEngine { get; }

    /// <summary>
    /// Stop recognizing speech
    /// </summary>
    public abstract void RecognizeAsyncStop();

    /// <summary>
    /// Start recognizing speech
    /// </summary>
    /// <param name="Mode">The recognition mode (single/multiple)</param>
    public abstract void RecognizeAsync(RecognizeMode Mode);

    /// <summary>
    /// Initializes the microphone to the default Windows mic
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public abstract bool InitializeMicrophone();

    /// <summary>
    /// Disposes of the service and speech recognition engine
    /// </summary>
    public abstract void Dispose();
}
