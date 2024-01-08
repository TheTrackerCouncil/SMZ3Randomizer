using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NAudio.Wave;
using Randomizer.Data.Options;
using Randomizer.Shared;

using SharpHook;
using SharpHook.Native;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Randomizer.SMZ3.Tracking.Services.Speech;

/// <summary>
/// Represents a speech recognition service that uses a push-to-talk key to
/// monitor microphone input for possible voice commands.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class PushToTalkSpeechRecognitionService : SpeechRecognitionServiceBase, INotifyPropertyChanged
{
    private readonly IGlobalHook _hook;
    private readonly IMicrophoneService _microphoneService;
    private readonly ILogger<PushToTalkSpeechRecognitionService> _logger;
    private readonly WaveFormat _waveFormat;

    private bool _hasInitialized;
    private bool _isEnabled;
    private bool _isListening;
    private KeyCode _pushToTalkKey;
    private Stream? _prevStream;

    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="PushToTalkSpeechRecognitionService"/> class.
    /// </summary>
    /// <param name="hook"></param>
    /// <param name="microphoneService"></param>
    /// <param name="logger"></param>
    /// <param name="trackerOptionsAccessor"></param>
    public PushToTalkSpeechRecognitionService(
        IGlobalHook hook,
        IMicrophoneService microphoneService,
        ILogger<PushToTalkSpeechRecognitionService> logger,
        TrackerOptionsAccessor trackerOptionsAccessor)
    {
        RecognitionEngine = new SpeechRecognitionEngine();
        RecognitionEngine.RecognizeCompleted += RecognitionEngineOnRecognizeCompleted;
        _hook = hook;
        _microphoneService = microphoneService;
        _logger = logger;
        _pushToTalkKey = (KeyCode?)trackerOptionsAccessor.Options?.PushToTalkKey ?? KeyCode.VcLeftControl;
        _microphoneService.DesiredAudioDevice = trackerOptionsAccessor.Options?.PushToTalkDevice;
        _waveFormat = new WaveFormat();
    }

    /// <summary>
    /// Occurs when a property value has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    [NotNull]
    public sealed override SpeechRecognitionEngine? RecognitionEngine { get; }

    /// <summary>
    /// Gets or sets a value indicating whether speech recognition should be
    /// performed.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;

            _isEnabled = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether Tracker is currently listening.
    /// </summary>
    public bool IsListening
    {
        get => _isListening;
        private set
        {
            if (_isListening == value) return;

            _isListening = value;
            RaisePropertyChanged();
        }
    }

    /// <inheritdoc/>
    public override bool Initialize()
    {
        if (!_hasInitialized)
        {
            RecognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;
            _hook.KeyPressed += Hook_KeyPressed;
            _hook.KeyReleased += Hook_KeyReleased;
            _hasInitialized = true;
        }
        return _microphoneService.CanRecord() && SpeechSupportsWaveFormat(_waveFormat);
    }

    /// <inheritdoc/>
    public override void ResetInputDevice()
    {
    }

    /// <inheritdoc/>
    public override void StartRecognition()
    {
        IsEnabled = true;
    }

    /// <inheritdoc/>
    public override void StopRecognition()
    {
        IsEnabled = false;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            using (_logger.TimeDebug("Disposed in {ElapsedMs} ms"))
            {
                RecognitionEngine.SpeechRecognized -= RecognitionEngine_SpeechRecognized;
                _hook.KeyPressed -= Hook_KeyPressed;
                _hook.KeyReleased -= Hook_KeyReleased;
            }
        }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property whose value has changed.
    /// </param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    /// A <see cref="PropertyChangedEventArgs"/> that contains the event data.
    /// </param>
    protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(sender, e);
    }

    private static bool SpeechSupportsWaveFormat(WaveFormat waveFormat)
    {
        return waveFormat.BitsPerSample switch
        {
            8 => true,
            16 => true,
            _ => false
        }
            && waveFormat.Channels switch
            {
                1 => true,
                2 => true,
                _ => false
            };
    }

    private SpeechAudioFormatInfo GetAudioFormatInfo()
    {
        return new SpeechAudioFormatInfo
        (
            _waveFormat.SampleRate,
            _waveFormat.BitsPerSample switch
            {
                8 => AudioBitsPerSample.Eight,
                16 => AudioBitsPerSample.Sixteen,
                _ => throw new InvalidOperationException($"System.Speech does not support {_waveFormat.BitsPerSample} bit audio")
            },
            _waveFormat.Channels switch
            {
                1 => AudioChannel.Mono,
                2 => AudioChannel.Stereo,
                _ => throw new InvalidOperationException($"System.Speech does not support {_waveFormat.Channels} channel audio")
            }
        );
    }

    private void RecognitionEngine_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        OnSpeechRecognized(sender, e);
    }

    private async void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        if (!IsEnabled) return;
        if (e.Data.KeyCode == _pushToTalkKey && IsListening)
        {
            IsListening = false;
            LogPushToTalkFinished(e.Data.KeyCode);

            // Stop recording from input device and run recognition
            await Task.Run(() =>
            {
                _prevStream = _microphoneService.StopRecording();

                if (_prevStream == null)
                {
                    _logger.LogWarning("No audio recording found");
                    return;
                }

                LogPushToTalkRecognitionStarted();

                RecognitionEngine.SetInputToAudioStream(_prevStream, GetAudioFormatInfo());
                RecognitionEngine.RecognizeAsync(RecognizeMode.Single);
            });
        }
    }

    private void RecognitionEngineOnRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        LogPushToTalkRecognitionStopped(e.AudioPosition);
        _prevStream?.Dispose();
        _prevStream = null;
    }

    private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (!IsEnabled) return;
        if (IsListening) return;

        if (e.Data.KeyCode == _pushToTalkKey)
        {
            if (!_microphoneService.CanRecord())
            {
                _logger.LogWarning("Currently unable to record audio");
                return;
            }

            IsListening = true;

            // Start recording from input device
            _microphoneService.StartRecording(new WaveFormat());
            LogPushToTalkActive(e.Data.KeyCode, _microphoneService.DeviceName ?? "unknown device");
        }
    }

    [LoggerMessage(1, LogLevel.Debug, "{KeyCode} pressed, recording from {InputDeviceName}")]
    private partial void LogPushToTalkActive(KeyCode keyCode, string inputDeviceName);

    [LoggerMessage(2, LogLevel.Debug, "{KeyCode} released, finishing recording")]
    private partial void LogPushToTalkFinished(KeyCode keyCode);

    [LoggerMessage(3, LogLevel.Debug, "Starting recognition")]
    private partial void LogPushToTalkRecognitionStarted();

    [LoggerMessage(4, LogLevel.Debug, "Finished recognition at {AudioPosition}")]
    private partial void LogPushToTalkRecognitionStopped(TimeSpan audioPosition);
}
