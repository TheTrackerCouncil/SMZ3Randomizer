using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Tracking.Services;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class SpeechRecognitionServiceEnabled : ISpeechRecognitionService
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
        SpeechRecognized?.Invoke(sender, e);
    }

    public event EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;

    public void SetInputToDefaultAudioDevice() => _recognizer.SetInputToDefaultAudioDevice();

    public SpeechRecognitionEngine? RecognitionEngine => _recognizer;

    public void RecognizeAsyncStop() =>_recognizer.RecognizeAsyncStop();

    public void RecognizeAsync(RecognizeMode mode) => _recognizer.RecognizeAsync(mode);

    public bool InitializeMicrophone()
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

    public void Dispose() => _recognizer.Dispose();
}
