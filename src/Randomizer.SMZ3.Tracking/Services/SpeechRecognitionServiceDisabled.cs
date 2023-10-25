using System;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.Services;

public class SpeechRecognitionServiceDisabled : SpeechRecognitionServiceBase
{
    public override void SetInputToDefaultAudioDevice()
    {

    }

    public override SpeechRecognitionEngine? RecognitionEngine => null;

    public override void RecognizeAsyncStop()
    {
    }

    public override void RecognizeAsync(RecognizeMode Mode)
    {

    }

    public override bool InitializeMicrophone()
    {
        return false;
    }

    public override void Dispose()
    {

    }
}
