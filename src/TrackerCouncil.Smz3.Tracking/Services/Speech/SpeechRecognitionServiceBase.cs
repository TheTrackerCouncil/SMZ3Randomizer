using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using PySpeechServiceClient.Grammar;
using PySpeechServiceClient.Models;

namespace TrackerCouncil.Smz3.Tracking.Services.Speech;

/// <summary>
/// Provides a base implementation for speech recognition services using the
/// built-in System.Speech.Recognition library.
/// </summary>
public abstract class SpeechRecognitionServiceBase : ISpeechRecognitionService, IDisposable
{
    /// <summary>
    /// Event fired when speech was successfully understood
    /// </summary>
    public event EventHandler<SpeechRecognitionResultEventArgs>? SpeechRecognized;

    /// <summary>
    /// The internal speech recognition engine used by the service, if
    /// applicable
    /// </summary>
    public abstract SpeechRecognitionEngine? RecognitionEngine { get; }

    /// <summary>
    /// Updates the microphone to be the default Windows mic
    /// </summary>
    public abstract void ResetInputDevice();

    /// <summary>
    /// Stop recognizing speech
    /// </summary>
    public abstract void StopRecognition();

    /// <summary>
    /// Start recognizing speech
    /// </summary>
    public abstract void StartRecognition();

    /// <summary>
    /// Initializes the microphone to the default Windows mic
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public abstract bool Initialize(float minRequiredConfidence, out bool foundRequestedDevice);

    ////// <summary>
    /// Adds a series of grammar rules to the recognition service
    /// </summary>
    public abstract void AddGrammar(List<SpeechRecognitionGrammar> grammars);

    /// <summary>
    /// Releases all resources used by the service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the service and speech recognition engine
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from Dispose and managed resources
    /// should be disposed of, or <see langword="false"/> when called from the
    /// finalizer and only unmanaged resources should be released.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (OperatingSystem.IsWindows())
                RecognitionEngine?.Dispose();
        }
    }

    /// <summary>
    /// Raises the SpeechRecognized event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected virtual void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs args)
    {
        if (!OperatingSystem.IsWindows()) return;

        SpeechRecognized?.Invoke(sender, new SpeechRecognitionResultEventArgs(new SpeechRecognitionResult()
        {
            Text = args.Result.Text ?? string.Empty,
            Confidence = args.Result.Confidence,
#pragma warning disable CA1416
            Semantics = args.Result.Semantics.ToDictionary(x => x.Key, x => new SpeechRecognitionSemantic(x.Key.ToString(), x.Value.ToString() ?? string.Empty)),
#pragma warning restore CA1416
            NativeResult = args.Result
        }));
    }

    protected virtual void OnSpeechRecognized(object? sender, SpeechRecognitionResultEventArgs args)
    {
        SpeechRecognized?.Invoke(sender, args);
    }
}
