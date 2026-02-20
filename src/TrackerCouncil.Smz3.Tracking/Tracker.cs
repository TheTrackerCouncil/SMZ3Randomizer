using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BunLabs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.Tracking.Services.Speech;
using TrackerCouncil.Smz3.Tracking.TrackingServices;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;

namespace TrackerCouncil.Smz3.Tracking;

/// <summary>
/// Tracks items and locations in a playthrough by listening for voice
/// commands and responding with text-to-speech.
/// </summary>
public sealed partial class Tracker : TrackerBase, IDisposable
{
    private static readonly Random s_random = new();

    private readonly IWorldAccessor _worldAccessor;
    private readonly TrackerModuleFactory _moduleFactory;
    private readonly IChatClient _chatClient;
    private readonly ILogger<Tracker> _logger;
    private readonly TrackerOptionsAccessor _trackerOptions;
    private readonly Dictionary<string, Timer> _idleTimers;
    private readonly Stack<(Action Action, DateTime UndoTime)> _undoHistory = new();
    private readonly ICommunicator _communicator;
    private readonly ITrackerStateService _stateService;
    private readonly ITrackerTimerService _timerService;
    private readonly ISpeechRecognitionService _recognizer;
    private readonly HashSet<SchrodingersString> _saidLines = new();

    private bool _disposed;
    private string? _lastSpokenText;
    private bool _isAltMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tracker"/> class.
    /// </summary>
    /// <param name="worldAccessor">
    /// Used to get the world to track in.
    /// </param>
    /// <param name="moduleFactory">
    /// Used to provide the tracking speech recognition syntax.
    /// </param>
    /// <param name="chatClient"></param>
    /// <param name="logger">Used to write logging information.</param>
    /// <param name="trackerOptions">Provides Tracker preferences.</param>
    /// <param name="playerProgressionService"></param>
    /// <param name="communicator"></param>
    /// <param name="historyService">Service for</param>
    /// <param name="stateService"></param>
    /// <param name="timerService"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="metadata"></param>
    public Tracker(IWorldAccessor worldAccessor,
        TrackerModuleFactory moduleFactory,
        IChatClient chatClient,
        ILogger<Tracker> logger,
        TrackerOptionsAccessor trackerOptions,
        IPlayerProgressionService playerProgressionService,
        ICommunicator communicator,
        IHistoryService historyService,
        ITrackerStateService stateService,
        ITrackerTimerService timerService,
        IServiceProvider serviceProvider,
        IMetadataService metadata)
    {
        Options = trackerOptions.Options ?? throw new InvalidOperationException("Tracker options have not yet been activated.");

        LoadServices(serviceProvider);

        _worldAccessor = worldAccessor;
        _moduleFactory = moduleFactory;
        _chatClient = chatClient;
        _logger = logger;
        _trackerOptions = trackerOptions;
        PlayerProgressionService = playerProgressionService;
        _communicator = communicator;
        _stateService = stateService;
        _timerService = timerService;
        _isAltMode = metadata.IsAltTrackerPack;

        // Initialize the tracker configuration
        _communicator.UseAlternateVoice(metadata.TrackerSpriteProfile?.UseAltVoice ?? false);
        Responses = metadata.Responses;
        Requests = metadata.Requests;
        PlayerProgressionService.ResetProgression();

        History = historyService;

        // Initalize the timers used to trigger idle responses
        if (Responses.Idle != null)
        {
            _idleTimers = Responses.Idle.ToDictionary(
                x => x.Key,
                x => new Timer(IdleTimerElapsed, x.Key, Timeout.Infinite, Timeout.Infinite));
        }
        else
        {
            _idleTimers = new();
        }

        // Initialize the speech recognition engine
        if (_trackerOptions.Options.SpeechRecognitionMode == SpeechRecognitionMode.Disabled || OperatingSystem.IsMacOS())
        {
            _recognizer = serviceProvider.GetRequiredService<NullSpeechRecognitionService>();
        }
        else if (_trackerOptions.Options.SpeechRecognitionMode == SpeechRecognitionMode.PushToTalk && OperatingSystem.IsWindows())
        {
            _recognizer = serviceProvider.GetRequiredService<PushToTalkSpeechRecognitionService>();
        }
        else if (_trackerOptions.Options.SpeechRecognitionMode == SpeechRecognitionMode.PySpeechService ||
                 OperatingSystem.IsLinux())
        {
            _recognizer = serviceProvider.GetRequiredService<PySpeechRecognitionService>();
        }
        else
        {
            _recognizer = serviceProvider.GetRequiredService<AlwaysOnSpeechRecognitionService>();
        }

        _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        InitializeMicrophone();
        World = _worldAccessor.World;

        Mood = metadata.Mood;
    }

    ~Tracker()
    {
        Dispose();
    }

    /// <summary>
    /// Attempts to replace a username with a pronunciation-corrected
    /// version of it.
    /// </summary>
    /// <param name="userName">The username to correct.</param>
    /// <returns>
    /// The corrected username, or <paramref name="userName"/>.
    /// </returns>
    public override string CorrectUserNamePronunciation(string userName)
    {
        if (Responses.Chat.UserNamePronunciation == null)
        {
            return userName.Replace('_', ' ');
        }

        var correctedUserName = Responses.Chat.UserNamePronunciation
            .SingleOrDefault(x => x.Key.Equals(userName, StringComparison.OrdinalIgnoreCase));

        return string.IsNullOrEmpty(correctedUserName.Value) ? userName.Replace('_', ' ') : correctedUserName.Value;
    }

    /// <summary>
    /// Initializes the microphone from the default audio device
    /// </summary>
    /// <returns>
    /// True if the microphone is initialized, false otherwise
    /// </returns>
    public override bool InitializeMicrophone()
    {
        if (MicrophoneInitialized) return true;
        MicrophoneInitialized = _recognizer.Initialize(_trackerOptions.Options?.MinimumRecognitionConfidence ?? 0.75f, out var foundRequestedDevice);
        MicrophoneInitializedAsDesiredDevice = foundRequestedDevice;
        return MicrophoneInitialized;
    }

    /// <summary>
    /// Loads the state from the database for a given rom
    /// </summary>
    /// <param name="rom">The rom to load</param>
    /// <param name="romPath">The full path to the rom to load</param>
    /// <returns>True or false if the load was successful</returns>
    public override bool Load(GeneratedRom rom, string romPath)
    {
        IsDirty = false;
        Rom = rom;
        RomPath = romPath;
        var trackerState = _stateService.LoadState(_worldAccessor.Worlds, rom);

        var configs = Config.FromConfigString(rom.Settings);
        LocalConfig = configs.First(x => x.IsLocalConfig);

        if (trackerState != null)
        {
            UpdateAllAccessibility(true);
            _timerService.SetSavedTime(TimeSpan.FromSeconds(trackerState.SecondsElapsed));
            OnStateLoaded();
            return true;
        }

        return false;
    }

    private void LoadServices(IServiceProvider serviceProvider)
    {
        var interfaceNamespace = typeof(TrackerBase).Namespace;
        if (string.IsNullOrEmpty(interfaceNamespace))
        {
            throw new InvalidOperationException("Could not determine TrackerBase namespace");
        }

        var properties = GetType().GetProperties()
            .Where(pi => pi.PropertyType.IsInterface && pi.PropertyType.Namespace == interfaceNamespace)
            .ToDictionary(
                pi => pi.PropertyType,
                pi => pi);

        var trackerService = serviceProvider.GetServices<TrackerService>().ToList();
        foreach (var service in trackerService)
        {
            service.Tracker = this;
            service.Initialize();
            var serviceInterface = service.GetType().GetInterfaces()
                .FirstOrDefault(x => x.Namespace == interfaceNamespace);
            if (serviceInterface != null && properties.TryGetValue(serviceInterface, out var property))
            {
                property.SetValue(this, service);
            }
        }

    }

    /// <summary>
    /// Saves the state of the tracker to the database
    /// </summary>
    /// <returns></returns>
    public override async Task SaveAsync()
    {
        if (Rom == null)
        {
            throw new NullReferenceException("No rom loaded into tracker");
        }

        IsDirty = false;
        await _stateService.SaveStateAsync(_worldAccessor.Worlds, Rom, _timerService.SecondsElapsed);
    }

    /// <summary>
    /// Undoes the last operation.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void Undo(float confidence)
    {
        if (_undoHistory.TryPop(out var undoLast))
        {
            if ((DateTime.Now - undoLast.UndoTime).TotalMinutes <= (_trackerOptions.Options?.UndoExpirationTime ?? 3))
            {
                Say(response: Responses.ActionUndone);
                undoLast.Action();
                OnActionUndone(new TrackerEventArgs(confidence));
            }
            else
            {
                Say(response: Responses.UndoExpired);
                _undoHistory.Push(undoLast);
            }
        }
        else
        {
            Say(response: Responses.NothingToUndo);
        }
    }

    /// <summary>
    /// Starts voice recognition.
    /// </summary>
    public override bool TryStartTracking()
    {
        // Load the modules for voice recognition
        StartTimer(true);

        bool loadError;
        try
        {
            var grammars = _moduleFactory.RetrieveGrammar(this, out loadError);
            _recognizer.AddGrammar(grammars);
            Syntax = _moduleFactory.Syntax;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to load modules");
            loadError = true;
        }

        try
        {
            EnableVoiceRecognition();
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError(e, "Error enabling voice recognition");
            loadError = true;
        }

        if (_isAltMode)
        {
            Say(response: Responses.StartingTrackingAlternate);
        }
        else
        {
            Say(response: Responses.StartedTracking);
        }

        RestartIdleTimers();
        return !loadError;
    }

    /// <summary>
    /// Connects Tracker to chat.
    /// </summary>
    /// <param name="userName">The username to connect as.</param>
    /// <param name="oauthToken">
    /// The OAuth token for <paramref name="userName"/>.
    /// </param>
    /// <param name="channel">
    /// The channel to monitor for incoming messages.
    /// </param>
    /// <param name="id">
    /// The is for <paramref name="userName"/>.
    /// </param>
    public override async Task ConnectToChatAsync(string? userName, string? oauthToken, string? channel, string? id)
    {
        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(oauthToken))
        {
            try
            {
                await _chatClient.ConnectAsync(userName, oauthToken, channel ?? userName, id ?? "");
            }
            catch (AggregateException e)
            {
                _logger.LogError(e, "Error in connection to Twitch chat");
                Say(x => x.Chat.WhenDisconnected);
            }
        }
    }

    /// <summary>
    /// Sets the start time of the timer
    /// </summary>
    public override void StartTimer(bool isInitial = false)
    {
        _timerService.StartTimer();

        if (!isInitial)
        {
            Say(response: Responses.TimerResumed);
            AddUndo(() => _timerService.Undo());
        }
    }

    /// <summary>
    /// Resets the timer to 0
    /// </summary>
    public override void ResetTimer(bool isInitial = false)
    {
        _timerService.ResetTimer();

        if (!isInitial)
        {
            Say(response: Responses.TimerReset);
            AddUndo(() => _timerService.Undo());
        }
    }

    /// <summary>
    /// Pauses the timer, saving the elapsed time
    /// </summary>
    public override Action? PauseTimer(bool addUndo = true)
    {
        _timerService.StopTimer();

        Say(response: Responses.TimerPaused);

        if (addUndo)
        {
            AddUndo(() => _timerService.Undo());
            return null;
        }
        else
        {
            return () => _timerService.Undo();
        }
    }

    /// <summary>
    /// Pauses or resumes the timer based on if it is
    /// currently paused or not
    /// </summary>
    public override void ToggleTimer()
    {
        if (_timerService.IsTimerPaused)
        {
            StartTimer();
        }
        else
        {
            PauseTimer();
        }
    }

    /// <summary>
    /// Stops voice recognition.
    /// </summary>
    public override void StopTracking()
    {
        DisableVoiceRecognition();
        _communicator.Abort();
        _ = _chatClient.DisconnectAsync();
        Say(response: ModeTracker.GoMode ? Responses.StoppedTrackingPostGoMode : Responses.StoppedTracking, wait: true);

        foreach (var timer in _idleTimers.Values)
            timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Enables the voice recognizer if the microphone is enabled
    /// </summary>
    public override void EnableVoiceRecognition()
    {
        if (MicrophoneInitialized && !VoiceRecognitionEnabled && _recognizer is not NullSpeechRecognitionService)
        {
            _logger.LogInformation("Starting speech recognition");
            _recognizer.ResetInputDevice();
            _recognizer.StopRecognition();
            _recognizer.StartRecognition();
            VoiceRecognitionEnabled = true;
            OnVoiceRecognitionEnabledChanged();
        }
    }

    /// <summary>
    /// Disables voice recognition if it was previously enabled
    /// </summary>
    public override void DisableVoiceRecognition()
    {
        if (VoiceRecognitionEnabled)
        {
            VoiceRecognitionEnabled = false;
            _recognizer.StopRecognition();
            _logger.LogInformation("Stopped speech recognition");
            OnVoiceRecognitionEnabledChanged();
        }
    }

    public override bool IsSpeechAvailable => _communicator.IsEnabled;

    /// <inheritdoc/>
    public override bool Say(
        Func<ResponseConfig, SchrodingersString?>? selectResponse = null,
        Func<ResponseConfig, Dictionary<int, SchrodingersString>?>? selectResponses = null,
        SchrodingersString? response = null,
        Dictionary<int, SchrodingersString>? responses = null,
        TrackerResponseDetails? preGeneratedDetails = null,
        int? key = null,
        int? tieredKey = null,
        object?[]? args = null,
        string? text = null,
        bool once = false,
        bool wait = false)
    {

        var details = preGeneratedDetails ?? GetTrackerResponses(selectResponse, selectResponses, response, responses, key, tieredKey, args, text, once);

        if (!details.Successful)
        {
            return false;
        }
        else if (details.Responses == null || details.Responses.Count == 0)
        {
            return true;
        }

        var speechRequests =
            details.Responses.Select(x => new SpeechRequest(x.SpeechText, x.TrackerImage, wait)).ToList();

        for (var i = 0; i < speechRequests.Count; i++)
        {
            if (i < speechRequests.Count - 1)
            {
                speechRequests[i].FollowedByBlankImage = "blank".Equals(speechRequests[i + 1].TrackerImage,
                    StringComparison.OrdinalIgnoreCase);
            }
            _communicator.Say(speechRequests[i]);
        }

        _lastSpokenText = details.Responses[0].SpeechText;
        return true;
    }

    /// <inheritdoc/>
    public override TrackerResponseDetails GetTrackerResponses(
        Func<ResponseConfig, SchrodingersString?>? selectResponse = null,
        Func<ResponseConfig, Dictionary<int, SchrodingersString>?>? selectResponses = null,
        SchrodingersString? response = null,
        Dictionary<int, SchrodingersString>? responses = null,
        int? key = null,
        int? tieredKey = null,
        object?[]? args = null,
        string? text = null,
        bool once = false)
    {
        SchrodingersString? selectedResponse = null;
        string? trackerImage = null;
        List<PossibilityAdditionalLine>? additionalLines = null;

        if (key != null)
        {
            // Grab the exact response out of the dictionary, if it exists.
            responses ??= selectResponses?.Invoke(Responses);
            responses?.TryGetValue((int)key, out selectedResponse);
        }
        else if (tieredKey != null)
        {
            var responseIndex = 0;

            responses ??= selectResponses?.Invoke(Responses);

            // Find the response with the closest value to the target delta, without being higher.
            for (var i = 0; i <= tieredKey; i++)
            {
                if (responses?.ContainsKey(i) == true)
                {
                    responseIndex = i;
                }
            }

            responses?.TryGetValue(responseIndex, out selectedResponse);
        }
        else
        {
            selectedResponse = response ?? selectResponse?.Invoke(Responses);
        }

        if (once && selectedResponse != null && !_saidLines.Add(selectedResponse))
        {
            // True because we did successfully select a response, even though it was already said before
            return new TrackerResponseDetails
            {
                Successful = true
            };
        }

        if (text == null)
        {
            var trackerSpeechDetails = selectedResponse?.GetSpeechDetails(args ?? []);
            if (trackerSpeechDetails?.SpeechText == null)
            {
                return new TrackerResponseDetails
                {
                    Successful = false
                };
            }

            text = trackerSpeechDetails.Value.SpeechText;
            trackerImage = trackerSpeechDetails.Value.TrackerImage;
            additionalLines = trackerSpeechDetails.Value.AdditionalLines;
        }

        List<TrackerResponseLine> formattedLines = [GetResponseLine(text, trackerImage)];
        if (additionalLines?.Count > 0)
        {
            formattedLines.AddRange(additionalLines.Select(x => GetResponseLine(x.Text, x.TrackerImage)));
        }

        return new TrackerResponseDetails()
        {
            Successful = true,
            Responses = formattedLines
        };
    }

    private string SpeechTextToDisplayText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;
        var noTags = XmlSpeechTags().Replace(input, " ");
        return char.ToUpper(noTags[0], CultureInfo.CurrentCulture) + noTags[1..];
    }

    private TrackerResponseLine GetResponseLine(string text, string? trackerImage)
    {
        var baseText = FormatPlaceholders(text);

        return new TrackerResponseLine()
        {
            SpeechText = baseText, DisplayText = SpeechTextToDisplayText(baseText), TrackerImage = trackerImage,
        };
    }

    /// <summary>
    /// Replaces global placeholders in a given string.
    /// </summary>
    /// <param name="text">The text with placeholders to format.</param>
    /// <returns>The formatted text with placeholders replaced.</returns>
    [return: NotNullIfNotNull("text")]
    private string? FormatPlaceholders(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var builder = new StringBuilder(text);
        builder.Replace("{Link}", CorrectPronunciation(World.Config.LinkName));
        builder.Replace("{Samus}", CorrectPronunciation(World.Config.SamusName));
        builder.Replace("{User}", CorrectUserNamePronunciation(Options.UserName ?? "someone"));

        // Just in case some text doesn't pass a string.Format
        builder.Replace("{{Link}}", CorrectPronunciation(World.Config.LinkName));
        builder.Replace("{{Samus}}", CorrectPronunciation(World.Config.SamusName));
        builder.Replace("{{User}}", CorrectUserNamePronunciation(Options.UserName ?? "someone"));
        return builder.ToString();
    }

    /// <summary>
    /// Repeats the most recently spoken sentence using text-to-speech at a
    /// slower rate.
    /// </summary>
    public override void Repeat()
    {
        if (Options.VoiceFrequency == TrackerVoiceFrequency.Disabled)
        {
            return;
        }

        if (string.IsNullOrEmpty(_lastSpokenText))
        {
            Say(text: "I haven't said anything yet.");
            return;
        }

        _communicator.Say(new SpeechRequest("I said"));
        _communicator.SlowDown();
        var prevLastSpokenText = _lastSpokenText;
        Say(text: _lastSpokenText, wait: true);
        _communicator.SpeedUp();
        _lastSpokenText = prevLastSpokenText;
    }

    /// <summary>
    /// Makes Tracker stop talking.
    /// </summary>
    public override void ShutUp()
    {
        _communicator.Abort();
    }

    /// <summary>
    /// Notifies the user an error occurred.
    /// </summary>
    public override void Error()
    {
        Say(response: Responses.Error);
    }

    /// <summary>
    /// Cleans up resources used by this class.
    /// </summary>
    public override void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Resets the timers for tracker mentioning nothing has happened
    /// </summary>
    public override void RestartIdleTimers()
    {
        foreach (var item in _idleTimers)
        {
            var timeout = Parse.AsTimeSpan(item.Key, s_random) ?? Timeout.InfiniteTimeSpan;
            var timer = item.Value;

            timer.Change(timeout, Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// Adds an action to be invoked to undo the last operation.
    /// </summary>
    /// <param name="undo">
    /// The action to invoke to undo the last operation.
    /// </param>
    public override void AddUndo(Action undo) => _undoHistory.Push((undo, DateTime.Now));

    public override (Action Action, DateTime UndoTime) PopUndo() => _undoHistory.Pop();

    public override void MarkAsDirty(bool isDirty = true) => IsDirty = isDirty;

    public override void UpdateAllAccessibility(bool forceRefreshAll, params Item[] items)
    {
        // Skip if the items don't affect anything
        if (items.Length > 0 && !forceRefreshAll && items.All(x =>
                x.Type.IsInCategory(ItemCategory.NeverProgression) ||
                (x.TrackingState > 3 && x.Type.IsInCategory(ItemCategory.ProgressionOnLimitedAmount))))
        {
            return;
        }

        PlayerProgressionService.ResetProgression();

        var actualProgression = PlayerProgressionService.GetProgression(false);
        var assumedKeysProgression = PlayerProgressionService.GetProgression(true);

        World.DarkWorldNorthEast.UpdateGanonAccessibility(actualProgression, assumedKeysProgression);
        World.WestCrateria.UpdateMotherBrainAccessibility(actualProgression);
        LocationTracker.UpdateAccessibility(!forceRefreshAll, actualProgression, assumedKeysProgression);
        RewardTracker.UpdateAccessibility(actualProgression, assumedKeysProgression);
        BossTracker.UpdateAccessibility(actualProgression, assumedKeysProgression);
    }

    /// <summary>
    /// Cleans up resources used by this class.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to dispose of managed resources.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                (_recognizer as IDisposable)?.Dispose();
                _communicator.Dispose();
                _moduleFactory.Dispose();

                foreach (var timer in _idleTimers.Values)
                    timer.Dispose();
            }

            _disposed = true;
        }
    }

    private void IdleTimerElapsed(object? state)
    {
        var key = (string)state!;
        Say(response: Responses.Idle?[key]);
    }

    private void Recognizer_SpeechRecognized(object? sender, SpeechRecognitionResultEventArgs e)
    {
        RestartIdleTimers();
        OnSpeechRecognized(new(e.Result.Confidence, e.Result.Text));
    }

    [GeneratedRegex(@"\s?<.*?>\s?")]
    private static partial Regex XmlSpeechTags();
}
