using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private (string onnxPath, string jsonPath)? _defaultPrimaryVoice;
    private (string onnxPath, string jsonPath)? _defaultAltVoice;
    private (string onnxPath, string jsonPath)? _primaryVoice;
    private (string onnxPath, string jsonPath)? _altVoice;
    private double _rate = 1;
    private bool _isEnabled;
    private bool _isSpeaking;
    private int volume;
    private ConcurrentDictionary<string, SpeechRequest> _pendingRequests = [];

    public PyTextToSpeechCommunicator(IPySpeechService pySpeechService, TrackerOptionsAccessor trackerOptionsAccessor)
    {
        _pySpeechService = pySpeechService;

        // Check to see if the user has the tracker voice files to use
        var piperPath = Path.Combine(Directories.AppDataFolder, "PiperModels");
        if (Directory.Exists(piperPath))
        {
            var femaleDetails = GetModelPath("Tracker_Female");
            var maleDetails = GetModelPath("Tracker_Male");
            _primaryVoice = _defaultPrimaryVoice = femaleDetails ?? maleDetails;
            _altVoice = _defaultAltVoice = maleDetails ?? femaleDetails;
        }

        volume = trackerOptionsAccessor.Options?.TextToSpeechVolume ?? 100;
        _ = Initialize();

        _pySpeechService.Initialized += (_, _) =>
        {
            _ = Initialize();
        };

        _pySpeechService.SpeakCommandResponded += (_, args) =>
        {
            _pendingRequests.TryGetValue(args.Response.FullMessage, out var request);

            if (args.Response.IsStartOfChunk)
            {
                VisemeReached?.Invoke(this, new SpeakingUpdatedEventArgs(true, request));
            }
            else if (args.Response.IsEndOfChunk)
            {
                VisemeReached?.Invoke(this, new SpeakingUpdatedEventArgs(false, request));
            }

            if (args.Response.IsStartOfMessage)
            {
                _isSpeaking = true;
                SpeakStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (args.Response.IsEndOfMessage)
            {
                SpeakCompleted?.Invoke(this, new SpeakCompletedEventArgs(TimeSpan.FromSeconds(3)));

                if (request != null)
                {
                    _pendingRequests.TryRemove(
                        new KeyValuePair<string, SpeechRequest>(args.Response.FullMessage, request));
                }

                if (!args.Response.HasAnotherRequest)
                {
                    _isSpeaking = false;
                }
            }
        };

        _isEnabled = trackerOptionsAccessor.Options?.VoiceFrequency != Shared.Enums.TrackerVoiceFrequency.Disabled;
    }

    public void UseAlternateVoice(bool useAlt = true)
    {
        if (useAlt)
        {
            _primaryVoice = _defaultAltVoice;
            _altVoice = _defaultPrimaryVoice;
        }
        else
        {
            _primaryVoice = _defaultPrimaryVoice;
            _altVoice = _defaultAltVoice;
        }
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

        _pendingRequests.TryAdd(request.Text, request);

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
