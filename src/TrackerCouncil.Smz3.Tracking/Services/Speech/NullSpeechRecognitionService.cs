using System;
using System.Collections.Generic;
using PySpeechService.Recognition;

namespace TrackerCouncil.Smz3.Tracking.Services.Speech;

/// <summary>
/// Represents a speech recognition service that does nothing.
/// </summary>
public sealed class NullSpeechRecognitionService : ISpeechRecognitionService
{

#pragma warning disable CS0067
    public event EventHandler<SpeechRecognitionResultEventArgs>? SpeechRecognized;
#pragma warning restore CS0067

    public void ResetInputDevice()
    {
    }

    public void StopRecognition()
    {
    }

    public void AddGrammar(List<SpeechRecognitionGrammar> grammars)
    {
    }

    public void StartRecognition()
    {
    }

    public bool Initialize(float minRequiredConfidence, out bool foundRequestedDevice)
    {
        foundRequestedDevice = true;
        return false;
    }
}
