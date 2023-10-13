using System;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.Services;

public class SpeechRecognitionServiceDisabled : ISpeechRecognitionService
{
    public event EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;

    public void SetInputToDefaultAudioDevice()
    {

    }

    public SpeechRecognitionEngine? RecognitionEngine => null;

    public void RecognizeAsyncStop()
    {
    }

    public void RecognizeAsync(RecognizeMode Mode)
    {

    }

    public bool InitializeMicrophone()
    {
        return false;
    }

    public void Dispose()
    {

    }
}
