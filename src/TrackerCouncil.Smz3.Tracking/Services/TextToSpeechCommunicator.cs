﻿using System;
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

    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="TextToSpeechCommunicator"/> class.
    /// </summary>
    public TextToSpeechCommunicator(TrackerOptionsAccessor trackerOptionsAccessor, ILogger<ICommunicator> logger)
    {
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
        };

        _tts.SpeakCompleted += (sender, args) =>
        {
            if (!OperatingSystem.IsWindows() || !_canSpeak || _tts.State != SynthesizerState.Ready) return;
            IsSpeaking = false;
            var duration = DateTime.Now - _startSpeakingTime;
            SpeakCompleted?.Invoke(this, new SpeakCompletedEventArgs(duration));
        };

        _canSpeak = trackerOptionsAccessor.Options?.VoiceFrequency != Shared.Enums.TrackerVoiceFrequency.Disabled;
    }

    /// <summary>
    /// Selects a different text-to-speech voice.
    /// </summary>
    public void UseAlternateVoice()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        _tts.SelectVoiceByHints(VoiceGender.Male);
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
        _tts.SpeakAsyncCancelAll();
    }

    /// <summary>
    /// Speaks the specified text or SSML.
    /// </summary>
    /// <param name="text">
    /// The plain text or SSML representation of the text to communicate.
    /// </param>
    public void Say(string text)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        if (!_canSpeak) return;
        var prompt = GetPromptFromText(text);
        if (prompt != null)
        {
            _tts.SpeakAsync(prompt);
        }
    }

    /// <summary>
    /// Speaks the specified text or SSML and blocks the calling thread
    /// until the text-to-speech engine has finished speaking the text.
    /// </summary>
    /// <param name="text">
    /// The plain text or SSML representation of the text to communicate.
    /// </param>
    public void SayWait(string text)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        if (!_canSpeak) return;
        var prompt = GetPromptFromText(text);
        if (prompt != null)
        {
            _tts.Speak(prompt);
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

    public event EventHandler<SpeakCompletedEventArgs>? SpeakCompleted;

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
}
