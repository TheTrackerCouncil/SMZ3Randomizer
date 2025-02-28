using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Threading.Tasks;
using PySpeechServiceClient;
using PySpeechServiceClient.Grammar;

namespace TrackerCouncil.Smz3.Tracking.Services.Speech;

public class PySpeechRecognitionService(IPySpeechService pySpeechService) : SpeechRecognitionServiceBase
{
    private bool _isEnabled;

    public override SpeechRecognitionEngine? RecognitionEngine => null;

    public override bool Initialize(out bool foundRequestedDevice)
    {
        if (!pySpeechService.IsConnected)
        {
            _ = pySpeechService.StartAsync();
        }

        pySpeechService.Initialized += async (_, _) =>
        {
            if (_isEnabled)
            {
                await StartPySpeechRecognition();
            }
        };

        pySpeechService.SpeechRecognized += OnSpeechRecognized;

        foundRequestedDevice = true;
        return true;
    }

    public override void AddGrammar(List<SpeechRecognitionGrammar> grammars)
    {
        foreach (var grammar in grammars)
        {
            pySpeechService.AddSpeechRecognitionCommand(grammar);
        }
    }

    public override void ResetInputDevice()
    {
        // Do nothing
    }

    public override void StartRecognition()
    {
        _isEnabled = true;

        if (pySpeechService.IsConnected)
        {
            _ = StartPySpeechRecognition();
        }
        else
        {
            _ = pySpeechService.StartAsync();
        }
    }

    public override void StopRecognition()
    {
        if (_isEnabled)
        {
            _isEnabled = false;
            pySpeechService.StopSpeechRecognitionAsync();
        }
    }

    private async Task StartPySpeechRecognition()
    {
        try
        {
            await pySpeechService.StartSpeechRecognitionAsync();
        }
        catch (Exception)
        {
            // do nothing
        }

    }
}
