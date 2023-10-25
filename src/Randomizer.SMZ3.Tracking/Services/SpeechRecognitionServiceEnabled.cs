using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Tracking.Services;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class SpeechRecognitionServiceEnabled : SpeechRecognitionServiceBase
{
    private readonly ILogger<SpeechRecognitionServiceEnabled> _logger;
    private readonly SpeechRecognitionEngine _recognizer = new();

    /// <summary>
    /// Dll to get the number of microphones
    /// </summary>
    /// <returns></returns>
    [DllImport("winmm.dll")]
    private static extern int waveInGetNumDevs();

    public SpeechRecognitionServiceEnabled(ILogger<SpeechRecognitionServiceEnabled> logger)
    {
        _logger = logger;
        _recognizer.SpeechRecognized += RecognizerOnSpeechRecognized;
    }

    private void RecognizerOnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        OnSpeechRecognized(sender, e);
    }

    public override void SetInputToDefaultAudioDevice() => _recognizer.SetInputToDefaultAudioDevice();

    public override SpeechRecognitionEngine? RecognitionEngine => _recognizer;

    public override void RecognizeAsyncStop() =>_recognizer.RecognizeAsyncStop();

    public override void RecognizeAsync(RecognizeMode mode) => _recognizer.RecognizeAsync(mode);

    public override bool InitializeMicrophone()
    {
        try
        {
            if (waveInGetNumDevs() == 0)
            {
                _logger.LogWarning("No microphone device found.");
                return false;
            }
            _recognizer.SetInputToDefaultAudioDevice();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error initializing microphone");
            return false;
        }
    }

    public override void Dispose() => _recognizer.Dispose();
}
