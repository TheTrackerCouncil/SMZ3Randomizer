using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Tracking.Services.Speech;

/// <summary>
/// Represents a speech recognition service that monitors the microphone for
/// possible voice commands.
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class AlwaysOnSpeechRecognitionService : SpeechRecognitionServiceBase
{
    private readonly ILogger<AlwaysOnSpeechRecognitionService> _logger;
    private readonly SpeechRecognitionEngine _recognizer = new();

    public AlwaysOnSpeechRecognitionService(ILogger<AlwaysOnSpeechRecognitionService> logger)
    {
        _logger = logger;
        _recognizer.SpeechRecognized += RecognizerOnSpeechRecognized;
    }

    public override SpeechRecognitionEngine? RecognitionEngine => _recognizer;

    public override void ResetInputDevice() => _recognizer.SetInputToDefaultAudioDevice();

    public override void StopRecognition() => _recognizer.RecognizeAsyncStop();

    public override void StartRecognition() => _recognizer.RecognizeAsync(RecognizeMode.Multiple);

    public override bool Initialize()
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

    /// <summary>
    /// Dll to get the number of microphones
    /// </summary>
    /// <returns></returns>
    [DllImport("winmm.dll")]
    private static extern int waveInGetNumDevs();

    private void RecognizerOnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        OnSpeechRecognized(sender, e);
    }
}
