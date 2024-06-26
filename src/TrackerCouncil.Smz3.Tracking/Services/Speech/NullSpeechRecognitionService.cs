using System;
using System.Speech.Recognition;

namespace TrackerCouncil.Smz3.Tracking.Services.Speech;

/// <summary>
/// Represents a speech recognition service that does nothing.
/// </summary>
public sealed class NullSpeechRecognitionService : ISpeechRecognitionService
{

#pragma warning disable CS0067
    public event EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;
#pragma warning restore CS0067

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
