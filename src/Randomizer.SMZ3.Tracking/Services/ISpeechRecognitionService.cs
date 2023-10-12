using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.Services;

/// <summary>
/// Service for recognizing speech from a microphone
/// </summary>
public interface ISpeechRecognitionService
{
    /// <summary>
    /// Event fired when speech was successfully understood
    /// </summary>
    public event System.EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;

    /// <summary>
    /// Updates the microphone to be the default Windows mic
    /// </summary>
    public void SetInputToDefaultAudioDevice();

    /// <summary>
    /// The internal speech recognition engine used by the service, if applicable
    /// </summary>
    public SpeechRecognitionEngine? RecognitionEngine { get; }

    /// <summary>
    /// Stop recognizing speech
    /// </summary>
    public void RecognizeAsyncStop();

    /// <summary>
    /// Start recognizing speech
    /// </summary>
    /// <param name="Mode">The recognition mode (single/multiple)</param>
    public void RecognizeAsync(RecognizeMode Mode);

    /// <summary>
    /// Initializes the microphone to the default Windows mic
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public bool InitializeMicrophone();

    /// <summary>
    /// Disposes of the service and speech recognition engine
    /// </summary>
    public void Dispose();
}
