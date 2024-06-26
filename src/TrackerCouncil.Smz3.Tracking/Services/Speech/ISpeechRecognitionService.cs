using System;
using System.Speech.Recognition;

namespace TrackerCouncil.Smz3.Tracking.Services.Speech;

/// <summary>
/// Defines the methods for speech recognition services.
/// </summary>
public interface ISpeechRecognitionService
{
    /// <summary>
    /// Occurs when speech was successfully understood.
    /// </summary>
    event EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;

    /// <summary>
    /// Performs first-time initialization of the speech recognition service.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if initialization was successful; <see
    /// langword="false"/> to disable speech recognition.
    /// </returns>
    bool Initialize(out bool foundRequestedDevice);

    /// <summary>
    /// Re-initializes the input device to ensure a valid microphone is being
    /// used.
    /// </summary>
    void ResetInputDevice();

    /// <summary>
    /// Starts speech recognition in the background.
    /// </summary>
    void StartRecognition();

    /// <summary>
    /// Stops speech recognition in the background.
    /// </summary>
    void StopRecognition();
}
