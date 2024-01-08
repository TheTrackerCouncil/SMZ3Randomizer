using System;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.Services.Speech;

/// <summary>
/// Provides a base implementation for speech recognition services using the
/// built-in System.Speech.Recognition library.
/// </summary>
public abstract class SpeechRecognitionServiceBase : ISpeechRecognitionService, IDisposable
{
    /// <summary>
    /// Event fired when speech was successfully understood
    /// </summary>
    public event EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;

    /// <summary>
    /// The internal speech recognition engine used by the service, if
    /// applicable
    /// </summary>
    public abstract SpeechRecognitionEngine? RecognitionEngine { get; }

    /// <summary>
    /// Updates the microphone to be the default Windows mic
    /// </summary>
    public abstract void ResetInputDevice();

    /// <summary>
    /// Stop recognizing speech
    /// </summary>
    public abstract void StopRecognition();

    /// <summary>
    /// Start recognizing speech
    /// </summary>
    public abstract void StartRecognition();

    /// <summary>
    /// Initializes the microphone to the default Windows mic
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public abstract bool Initialize(out bool foundRequestedDevice);

    /// <summary>
    /// Releases all resources used by the service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the service and speech recognition engine
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from Dispose and managed resources
    /// should be disposed of, or <see langword="false"/> when called from the
    /// finalizer and only unmanaged resources should be released.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (OperatingSystem.IsWindows())
                RecognitionEngine?.Dispose();
        }
    }

    /// <summary>
    /// Raises the SpeechRecognized event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected virtual void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs args)
    {
        if (OperatingSystem.IsWindows())
            SpeechRecognized?.Invoke(sender, args);
    }
}
