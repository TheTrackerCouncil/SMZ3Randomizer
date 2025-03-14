using System;
using System.IO;
using System.Threading.Tasks;
using PySpeechServiceClient;
using PySpeechServiceClient.Models;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.Tracking.Services;

internal class PyTextToSpeechCommunicator : ICommunicator
{
    private readonly IPySpeechService _pySpeechService;
    private (string onnxPath, string jsonPath)? _primaryVoice;
    private (string onnxPath, string jsonPath)? _altVoice;
    private double _rate = 1;
    private bool _isEnabled;
    private bool _isSpeaking;
    private int volume;

    public PyTextToSpeechCommunicator(IPySpeechService pySpeechService, TrackerOptionsAccessor trackerOptionsAccessor)
    {
        _pySpeechService = pySpeechService;

        // Check to see if the user has the tracker voice files to use
        var piperPath = Path.Combine(Directories.AppDataFolder, "PiperModels");
        if (Directory.Exists(piperPath))
        {
            var femaleDetails = GetModelPath("Tracker_Female");
            var maleDetails = GetModelPath("Tracker_Male");
            _primaryVoice = femaleDetails ?? maleDetails;
            _altVoice = maleDetails ?? femaleDetails;
        }

        volume = trackerOptionsAccessor.Options?.TextToSpeechVolume ?? 100;
        _ = Initialize();

        _pySpeechService.Initialized += (_, _) =>
        {
            _ = Initialize();
        };

        _pySpeechService.SpeakCommandResponded += (_, args) =>
        {
            if (args.Response.IsStartOfChunk)
            {
                VisemeReached?.Invoke(this, new SpeakingUpdatedEventArgs(true, null));
            }
            else if (args.Response.IsEndOfChunk)
            {
                VisemeReached?.Invoke(this, new SpeakingUpdatedEventArgs(false, null));
            }

            if (args.Response.IsStartOfMessage)
            {
                _isSpeaking = true;
                SpeakStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (args.Response.IsEndOfMessage)
            {
                SpeakCompleted?.Invoke(this, new SpeakCompletedEventArgs(TimeSpan.FromSeconds(3)));

                if (!args.Response.HasAnotherRequest)
                {
                    _isSpeaking = false;
                }
            }
        };

        _isEnabled = trackerOptionsAccessor.Options?.VoiceFrequency != Shared.Enums.TrackerVoiceFrequency.Disabled;
    }

    public void UseAlternateVoice()
    {
        (_primaryVoice, _altVoice) = (_altVoice, _primaryVoice);
    }

    private async Task Initialize()
    {
        await _pySpeechService.SetSpeechSettingsAsync(GetSpeechSettings());
        await _pySpeechService.SetVolumeAsync(volume / 100.0);
    }

    private SpeechSettings GetSpeechSettings()
    {
        return new SpeechSettings
        {
            OnnxPath = _primaryVoice?.Item1 ?? string.Empty,
            ConfigPath = _primaryVoice?.Item2 ?? string.Empty,
            AltOnnxPath = _altVoice?.Item1 ?? string.Empty,
            AltConfigPath = _altVoice?.Item2 ?? string.Empty,
            Speed = _rate,
        };
    }

    private (string onnxPath, string jsonPath)? GetModelPath(string modelName)
    {
        var onnxPath = Path.Combine(Directories.AppDataFolder, "PiperModels", $"{modelName}.onnx");
        var jsonPath = Path.Combine(Directories.AppDataFolder, "PiperModels", $"{modelName}.json");
        if (File.Exists(onnxPath) && File.Exists(jsonPath))
        {
            return (onnxPath, jsonPath);
        }

        return null;
    }

    public void Say(SpeechRequest request)
    {
        if (!_isEnabled || !_pySpeechService.IsSpeechEnabled) return;

        if (request.Wait)
        {
            _pySpeechService.Speak(request.Text, GetSpeechSettings());
        }
        else
        {
            _pySpeechService.SpeakAsync(request.Text, GetSpeechSettings());
        }
    }

    public bool IsEnabled => _pySpeechService.IsSpeechEnabled && _isEnabled;

    public void Enable()
    {
        if (!_pySpeechService.IsSpeechEnabled)
        {
            _ = _pySpeechService.SetSpeechSettingsAsync(GetSpeechSettings());
        }
        _isEnabled = true;
    }

    public void Disable()
    {
        _ = _pySpeechService.StopSpeakingAsync();
        _isEnabled = false;
    }

    public void SpeedUp()
    {
        _rate = _rate < 1 ? 1 : 2;
    }

    public void SlowDown()
    {
        _rate = _rate > 1 ? 1 : .5;
    }

    public void Abort()
    {
        _ = _pySpeechService.StopSpeakingAsync();
    }

    public bool IsSpeaking => _isSpeaking;

    public void UpdateVolume(int newVolume)
    {
        volume = newVolume;
        _ = _pySpeechService.SetVolumeAsync(newVolume / 100.0f);
    }

    public event EventHandler? SpeakStarted;
    public event EventHandler<SpeakCompletedEventArgs>? SpeakCompleted;
    public event EventHandler<SpeakingUpdatedEventArgs>? VisemeReached;
}
