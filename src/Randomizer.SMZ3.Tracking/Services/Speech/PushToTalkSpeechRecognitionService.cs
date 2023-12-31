using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

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
    private readonly ILogger<PushToTalkSpeechRecognitionService> _logger;

    private Task? _hookRunner;
    private bool _isEnabled;
    private bool _isListening;

    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="PushToTalkSpeechRecognitionService"/> class.
    /// </summary>
    /// <param name="hook"></param>
    /// <param name="logger"></param>
    public PushToTalkSpeechRecognitionService(
        IGlobalHook hook,
        ILogger<PushToTalkSpeechRecognitionService> logger)
    {
        RecognitionEngine = new SpeechRecognitionEngine();
        _hook = hook;
        _logger = logger;
    }

    /// <summary>
    /// Occurs when a property value has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    [NotNull]
    public override SpeechRecognitionEngine? RecognitionEngine { get; }

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
        RecognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;
        _hook.KeyPressed += Hook_KeyPressed;
        _hook.KeyReleased += Hook_KeyReleased;
        _hookRunner = _hook.RunAsync();
        return true;
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
            RecognitionEngine.SpeechRecognized -= RecognitionEngine_SpeechRecognized;
            _hook.KeyPressed -= Hook_KeyPressed;
            _hook.KeyReleased -= Hook_KeyReleased;
            _hook.Dispose();
            _hookRunner?.GetAwaiter().GetResult();
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

    private void RecognitionEngine_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        OnSpeechRecognized(sender, e);
    }

    private void Hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        if (!IsEnabled) return;
        if (e.Data.KeyCode == KeyCode.VcLeftControl && IsListening)
        {
            IsListening = false;

            // Stop recording from input device and run recognition
            LogPushToTalkFinished(e.Data.KeyCode);
        }
    }

    private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (!IsEnabled) return;
        if (IsListening) return;

        if (e.Data.KeyCode == KeyCode.VcLeftControl)
        {
            IsListening = true;

            // Start recording from input device
            LogPushToTalkActive(e.Data.KeyCode, "unknown device");
        }
    }

    [LoggerMessage(1, LogLevel.Debug, "{KeyCode} pressed, recording from {InputDeviceName}")]
    private partial void LogPushToTalkActive(KeyCode keyCode, string inputDeviceName);

    [LoggerMessage(2, LogLevel.Debug, "{KeyCode} released, finishing recording")]
    private partial void LogPushToTalkFinished(KeyCode keyCode);
}
