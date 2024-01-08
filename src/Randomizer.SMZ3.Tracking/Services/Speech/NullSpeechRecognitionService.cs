using System;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.Services.Speech;

/// <summary>
/// Represents a speech recognition service that does nothing.
/// </summary>
public sealed class NullSpeechRecognitionService : ISpeechRecognitionService
{
    public event EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;

    public void ResetInputDevice()
    {
    }

    public void StopRecognition()
    {
    }

    public void StartRecognition()
    {
    }

    public bool Initialize(out bool foundRequestedDevice)
    {
        foundRequestedDevice = true;
        return false;
    }
}
