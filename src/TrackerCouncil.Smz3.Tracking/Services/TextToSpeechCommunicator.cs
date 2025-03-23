using System;
using System.Collections.Concurrent;
using System.Speech.Synthesis;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Facilitates communication with the player using Windows' built-in
/// text-to-speech engine.
/// </summary>
public class TextToSpeechCommunicator : ICommunicator, IDisposable
{
    private readonly SpeechSynthesizer _tts = null!;
    private bool _canSpeak;
    private DateTime _startSpeakingTime;
    private ConcurrentQueue<SpeechRequest> _pendingRequests = [];
    private SpeechRequest? _currentSpeechRequest;
    private ILogger<ICommunicator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="TextToSpeechCommunicator"/> class.
    /// </summary>
    public TextToSpeechCommunicator(TrackerOptionsAccessor trackerOptionsAccessor, ILogger<ICommunicator> logger)
    {
        _logger = logger;

        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        _tts = new SpeechSynthesizer();
        _tts.SelectVoiceByHints(VoiceGender.Female);

        _tts.SpeakStarted += (sender, args) =>
        {
            if (IsSpeaking) return;
            _startSpeakingTime = DateTime.Now;
            IsSpeaking = true;
            SpeakStarted?.Invoke(this, EventArgs.Empty);
        };

        _tts.StateChanged += (sender, args) =>
        {
            if (!OperatingSystem.IsWindows() || !_canSpeak || args.State != SynthesizerState.Ready) return;

            if (_pendingRequests.IsEmpty)
            {
                IsSpeaking = false;
                var duration = DateTime.Now - _startSpeakingTime;
                _currentSpeechRequest = null;
                SpeakCompleted?.Invoke(this, new SpeakCompletedEventArgs(duration));
            }
            else
            {
                SayNext();
            }
        };

        _tts.VisemeReached += (sender, args) =>
        {
            if (!OperatingSystem.IsWindows()) return;
            VisemeReached?.Invoke(this, new SpeakVisemeReachedEventArgs(args, _currentSpeechRequest));
        };

        _canSpeak = trackerOptionsAccessor.Options?.VoiceFrequency != Shared.Enums.TrackerVoiceFrequency.Disabled;
    }

    /// <summary>
    /// Selects a different text-to-speech voice.
    /// </summary>
    public void UseAlternateVoice(bool useAlt = true)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        _tts.SelectVoiceByHints(useAlt ? VoiceGender.Male : VoiceGender.Female);
    }

    /// <summary>
    /// Aborts all current and queued up text-to-speech actions.
    /// </summary>
    public void Abort()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        IsSpeaking = false;
        _pendingRequests.Clear();
        _tts.SpeakAsyncCancelAll();
    }

    /// <summary>
    /// Speaks the specified text or SSML.
    /// </summary>
    /// <param name="request">
    /// The request object containing details about what to communicate
    /// </param>
    public void Say(SpeechRequest request)
    {
        _logger.LogDebug($"Speech: {request.Text}");

        if (!OperatingSystem.IsWindows() || !_canSpeak)
        {
            return;
        }

        if (_pendingRequests.IsEmpty && !IsSpeaking)
        {
            SayNext(request);
        }
        else
        {
            _pendingRequests.Enqueue(request);
        }
    }

    /// <summary>
    /// Increases the voice speed.
    /// </summary>
    public void SpeedUp()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        _tts.Rate += 2;
    }

    /// <summary>
    /// Decreases the voice speed.
    /// </summary>
    public void SlowDown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        _tts.Rate -= 2;
    }

    public bool IsSpeaking { get; private set; }

    public event EventHandler? SpeakStarted;

    public event EventHandler<SpeakCompletedEventArgs>? SpeakCompleted;

    public event EventHandler<SpeakVisemeReachedEventArgs>? VisemeReached;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates a new <see cref="Prompt"/> based on the specified text.
    /// </summary>
    /// <param name="text">The plain text or SSML markup to parse.</param>
    /// <returns>A new <see cref="Prompt"/>.</returns>
    protected static Prompt? GetPromptFromText(string text)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        // If text does not contain any XML elements, just interpret it as
        // text
        if (!text.Contains("<") && !text.Contains("/>"))
            return new Prompt(text);

        var prompt = new PromptBuilder();
        prompt.AppendSsmlMarkup(text);
        return new Prompt(prompt);
    }

    /// <summary>
    /// Cleans up managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        _canSpeak = false;
        if (disposing && OperatingSystem.IsWindows())
        {
            _tts.Dispose();
        }
    }

    /// <summary>
    /// Enables communicating
    /// </summary>
    public void Enable()
    {
        Abort();
        _canSpeak = true;
    }

    /// <summary>
    /// Disables communicating
    /// </summary>
    public void Disable()
    {
        _canSpeak = false;
    }

    /// <summary>
    /// If the communicator is currently enabled
    /// </summary>
    /// <returns>True if enabled, false otherwise</returns>
    public bool IsEnabled => _canSpeak;

    private void SayNext(SpeechRequest? request = null)
    {
        if (!_canSpeak || !OperatingSystem.IsWindows())
        {
            return;
        }

        if (request == null)
        {
            if (!_pendingRequests.TryDequeue(out var nextRequest))
            {
                return;
            }
            request = nextRequest;
        }

        var prompt = GetPromptFromText(request.Text);
        if (prompt == null)
        {
            return;
        }

        _currentSpeechRequest = request;

        if (request.Wait)
        {
            _tts.Speak(prompt);
        }
        else
        {
            _tts.SpeakAsync(prompt);
        }
    }
}
