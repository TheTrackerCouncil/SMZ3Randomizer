using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Threading.Tasks;
using PySpeechServiceClient;
using PySpeechServiceClient.Grammar;
using TrackerCouncil.Smz3.Data.Configuration;

namespace TrackerCouncil.Smz3.Tracking.Services.Speech;

public class PySpeechRecognitionService(IPySpeechService pySpeechService, Configs configs) : SpeechRecognitionServiceBase
{
    private bool _isEnabled;
    private float _minRequiredConfidence = 0.8f;

    public override SpeechRecognitionEngine? RecognitionEngine => null;

    public override bool Initialize(float minRequiredConfidence, out bool foundRequestedDevice)
    {
        _minRequiredConfidence = minRequiredConfidence;

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

        pySpeechService.AddSpeechRecognitionReplacements(configs.MetadataConfig.PySpeechRecognitionReplacements);

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
            await pySpeechService.StartSpeechRecognitionAsync(requiredConfidence: _minRequiredConfidence * 100);
        }
        catch (Exception)
        {
            // do nothing
        }

    }
}
