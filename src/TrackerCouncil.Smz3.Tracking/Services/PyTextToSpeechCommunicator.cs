using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PySpeechService.Client;
using PySpeechService.TextToSpeech;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.Tracking.Services;

[SupportedOSPlatform("linux")]
internal class PyTextToSpeechCommunicator : ICommunicator
{
    private readonly IPySpeechService _pySpeechService;
    private readonly ILogger<PyTextToSpeechCommunicator> _logger;
    private DateTime? _startSpeakingTime;
    private (string onnxPath, string jsonPath)? _defaultPrimaryVoice;
    private (string onnxPath, string jsonPath)? _defaultAltVoice;
    private (string onnxPath, string jsonPath)? _primaryVoice;
    private (string onnxPath, string jsonPath)? _altVoice;
    private double _rate = 1;
    private bool _isEnabled;
    private bool _isSpeaking;
    private int volume;
    private ConcurrentDictionary<string, SpeechRequest> _pendingRequests = [];

    public PyTextToSpeechCommunicator(IPySpeechService pySpeechService, TrackerOptionsAccessor trackerOptionsAccessor, ILogger<PyTextToSpeechCommunicator> logger)
    {
        _pySpeechService = pySpeechService;
        _logger = logger;

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

        _pySpeechService.Initialized += PySpeechServiceOnInitialized;
        _pySpeechService.SpeakCommandResponded += PySpeechServiceOnSpeakCommandResponded;

        _isEnabled = trackerOptionsAccessor.Options?.VoiceFrequency != Shared.Enums.TrackerVoiceFrequency.Disabled;
    }

    private void PySpeechServiceOnSpeakCommandResponded(object? sender, SpeakCommandResponseEventArgs args)
    {
        SpeechRequest? request = null;

        _logger.LogInformation("Response: {Id}", args.Response.MessageId);

        if (string.IsNullOrEmpty(args.Response.MessageId) || !_pendingRequests.TryGetValue(args.Response.MessageId, out request))
        {
            _logger.LogError("Received PySpeechService SpeakCommandResponse with no valid message id");
        }

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
            if (_startSpeakingTime == null)
            {
                _startSpeakingTime = DateTime.Now;
            }

            _isSpeaking = true;
            SpeakStarted?.Invoke(this, EventArgs.Empty);
        }
        else if (args.Response.IsEndOfMessage)
        {
            if (request != null)
            {
                _pendingRequests.TryRemove(
                    new KeyValuePair<string, SpeechRequest>(args.Response.MessageId!, request));
            }

            if (!args.Response.HasAnotherRequest)
            {
                var duration = DateTime.Now - _startSpeakingTime;
                _startSpeakingTime = null;
                SpeakCompleted?.Invoke(this, new SpeakCompletedEventArgs(duration ?? TimeSpan.Zero, request));
                _isSpeaking = false;
            }
        }
    }

    private async void PySpeechServiceOnInitialized(object? sender, EventArgs args)
    {
        try
        {
            await Initialize();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error initializing PySpeechService");
        }
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

        var messageId = Guid.NewGuid().ToString();
        _pendingRequests.TryAdd(messageId, request);

        _logger.LogInformation("Request: {Id}", messageId);

        if (request.Wait)
        {
            _pySpeechService.Speak(request.Text, GetSpeechSettings(), messageId);
        }
        else
        {
            _pySpeechService.SpeakAsync(request.Text, GetSpeechSettings(), messageId);
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

    public void Dispose()
    {
        _pySpeechService.Initialized -= PySpeechServiceOnInitialized;
        _pySpeechService.SpeakCommandResponded -= PySpeechServiceOnSpeakCommandResponded;
    }
}
