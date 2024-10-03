using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BunLabs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.Tracking.Services.Speech;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;

namespace TrackerCouncil.Smz3.Tracking;

/// <summary>
/// Tracks items and locations in a playthrough by listening for voice
/// commands and responding with text-to-speech.
/// </summary>
public sealed class Tracker : TrackerBase, IDisposable
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
    private bool _disposed;
    private string? _lastSpokenText;
    private readonly bool _alternateTracker;
    private readonly HashSet<SchrodingersString> _saidLines = new();

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
    /// <param name="itemService"></param>
    /// <param name="communicator"></param>
    /// <param name="historyService">Service for</param>
    /// <param name="configs"></param>
    /// <param name="stateService"></param>
    /// <param name="timerService"></param>
    /// <param name="serviceProvider"></param>
    public Tracker(IWorldAccessor worldAccessor,
        TrackerModuleFactory moduleFactory,
        IChatClient chatClient,
        ILogger<Tracker> logger,
        TrackerOptionsAccessor trackerOptions,
        IItemService itemService,
        ICommunicator communicator,
        IHistoryService historyService,
        Configs configs,
        ITrackerStateService stateService,
        ITrackerTimerService timerService,
        IServiceProvider serviceProvider)
    {
        if (trackerOptions.Options == null)
            throw new InvalidOperationException("Tracker options have not yet been activated.");

        _worldAccessor = worldAccessor;
        _moduleFactory = moduleFactory;
        _chatClient = chatClient;
        _logger = logger;
        _trackerOptions = trackerOptions;
        ItemService = itemService;
        _communicator = communicator;
        _stateService = stateService;
        _timerService = timerService;

        // Initialize the tracker configuration
        Configs = configs;
        Responses = configs.Responses;
        Requests = configs.Requests;
        ItemService.ResetProgression();

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


        // Initialize the text-to-speech
        if (s_random.NextDouble() <= 0.01)
        {
            _alternateTracker = true;
            _communicator.UseAlternateVoice();
        }

        // Initialize the speech recognition engine
        if (_trackerOptions.Options.SpeechRecognitionMode == SpeechRecognitionMode.Disabled ||
            !OperatingSystem.IsWindows())
        {
            _recognizer = serviceProvider.GetRequiredService<NullSpeechRecognitionService>();
        }
        else if (_trackerOptions.Options.SpeechRecognitionMode == SpeechRecognitionMode.PushToTalk)
        {
            _recognizer = serviceProvider.GetRequiredService<PushToTalkSpeechRecognitionService>();
        }
        else
        {
            _recognizer = serviceProvider.GetRequiredService<AlwaysOnSpeechRecognitionService>();
        }
        _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        InitializeMicrophone();
        World = _worldAccessor.World;
        Options = _trackerOptions.Options;

        Mood = configs.CurrentMood ?? "";
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
        MicrophoneInitialized = _recognizer.Initialize(out var foundRequestedDevice);
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
            _timerService.SetSavedTime(TimeSpan.FromSeconds(trackerState.SecondsElapsed));
            OnStateLoaded();
            return true;
        }
        return false;
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
            var recognitionEngine = _recognizer is SpeechRecognitionServiceBase recognitionBase
                ? recognitionBase.RecognitionEngine
                : null;
            Syntax = _moduleFactory.LoadAll(this, recognitionEngine, out loadError);
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

        Say(response: _alternateTracker ? Responses.StartingTrackingAlternate : Responses.StartedTracking);
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
    public override void ConnectToChat(string? userName, string? oauthToken, string? channel, string? id)
    {
        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(oauthToken))
        {
            try
            {
                _chatClient.Connect(userName, oauthToken, channel ?? userName, id ?? "");
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
        _chatClient.Disconnect();
        Say(response: ModeTracker.GoMode ? Responses.StoppedTrackingPostGoMode : Responses.StoppedTracking, wait: true);

        foreach (var timer in _idleTimers.Values)
            timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Enables the voice recognizer if the microphone is enabled
    /// </summary>
    public override void EnableVoiceRecognition()
    {
        if (MicrophoneInitialized && !VoiceRecognitionEnabled && OperatingSystem.IsWindows())
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

    /// <inheritdoc/>
    public override bool Say(
        Func<ResponseConfig, SchrodingersString?>? selectResponse = null,
        Func<ResponseConfig, Dictionary<int, SchrodingersString>?>? selectResponses = null,
        SchrodingersString? response = null,
        Dictionary<int, SchrodingersString>? responses = null,
        int? key = null,
        int? tieredKey = null,
        object?[]? args = null,
        string? text = null,
        bool once = false,
        bool wait = false)
    {
        SchrodingersString? selectedResponse = null;
        string? trackerImage = null;

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
            return true;
        }

        if (text == null)
        {
            var trackerSpeechDetails = selectedResponse?.GetSpeechDetails(args ?? []);
            if (trackerSpeechDetails?.SpeechText == null)
            {
                return false;
            }
            text = trackerSpeechDetails.Value.SpeechText;
            trackerImage = trackerSpeechDetails.Value.TrackerImage;
        }

        var formattedText = FormatPlaceholders(text);

        _communicator.Say(new SpeechRequest(formattedText, trackerImage, wait));
        _lastSpokenText = formattedText;

        return true;
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

        if (_lastSpokenText == null)
        {
            Say(text: "I haven't said anything yet.");
            return;
        }

        _communicator.Say(new SpeechRequest("I said"));
        _communicator.SlowDown();
        Say(text: _lastSpokenText, wait: true);
        _communicator.SpeedUp();
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
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Updates the region that the player is in
    /// </summary>
    /// <param name="region">The region the player is in</param>
    /// <param name="updateMap">Set to true to update the map for the player to match the region</param>
    /// <param name="resetTime">If the time should be reset if this is the first region update</param>
    public override void UpdateRegion(Region region, bool updateMap = false, bool resetTime = false)
    {
        if (region != CurrentRegion)
        {
            if (resetTime && !History.GetHistory().Any(x => x is { LocationId: not null, IsUndone: false }))
            {
                ResetTimer(true);
            }

            History.AddEvent(
                HistoryEventType.EnteredRegion,
                true,
                region.Name
            );
        }

        CurrentRegion = region;
        if (updateMap && !string.IsNullOrEmpty(region?.MapName))
        {
            UpdateMap(region.MapName);
        }
    }

    /// <summary>
    /// Updates the map to display for the user
    /// </summary>
    /// <param name="map">The name of the map</param>
    public override void UpdateMap(string map)
    {
        CurrentMap = map;
        OnMapUpdated();
    }

    /// <summary>
    /// Called when the game is beaten by entering triforce room
    /// or entering the ship after beating both bosses
    /// </summary>
    /// <param name="autoTracked">If this was triggered by the auto tracker</param>
    public override void GameBeaten(bool autoTracked)
    {
        if (!HasBeatenGame)
        {
            HasBeatenGame = true;
            var pauseUndo = PauseTimer(false);
            Say(x => x.BeatGame);
            OnBeatGame(new TrackerEventArgs(autoTracked));
            if (!autoTracked)
            {
                AddUndo(() =>
                {
                    HasBeatenGame = false;
                    if (pauseUndo != null)
                    {
                        pauseUndo();
                    }
                    OnBeatGame(new TrackerEventArgs(autoTracked));
                });
            }
        }
    }

    /// <summary>
    /// Called when the player has died
    /// </summary>
    public override void TrackDeath(bool autoTracked)
    {
        OnPlayerDied(new TrackerEventArgs(autoTracked));
    }

    /// <summary>
    /// Updates the current track number being played
    /// </summary>
    /// <param name="number">The number of the track</param>
    public override void UpdateTrackNumber(int number)
    {
        if (number <= 0 || number > 200 || number == CurrentTrackNumber) return;
        CurrentTrackNumber = number;
        OnTrackNumberUpdated(new TrackNumberEventArgs(number));
    }

    /// <summary>
    /// Updates the current track being played
    /// </summary>
    /// <param name="msu">The current MSU pack</param>
    /// <param name="track">The current track</param>
    /// <param name="outputText">Formatted output text matching the requested style</param>
    public override void UpdateTrack(Msu msu, Track track, string outputText)
    {
        OnTrackChanged(new TrackChangedEventArgs(msu, track, outputText));
    }

    public override void UpdateHintTile(PlayerHintTile hintTile)
    {
        if (hintTile.State == null || LastViewedObject?.HintTile == hintTile)
        {
            return;
        }
        else if (hintTile.State.HintState == HintState.Cleared)
        {
            OnHintTileUpdated(new HintTileUpdatedEventArgs(hintTile));
            return;
        }

        LastViewedObject = new ViewedObject { HintTile = hintTile };

        if (hintTile.Type == HintTileType.Location)
        {
            var locationId = hintTile.Locations!.First();
            var location = World.FindLocation(locationId);
            if (location.State is { Cleared: false, Autotracked: false } && location.State.MarkedItem != location.State.Item)
            {
                LocationTracker.MarkLocation(location, location.Item, null, true);
                hintTile.State.HintState = HintState.Viewed;
            }
            else
            {
                hintTile.State.HintState = HintState.Cleared;
            }
        }
        else if (hintTile.Type == HintTileType.Requirement)
        {
            var dungeon = World.PrerequisiteRegions.First(x => x.Name == hintTile.LocationKey);
            if (!dungeon.HasMarkedCorrectly)
            {
                PrerequisiteTracker.SetDungeonRequirement(dungeon, hintTile.MedallionType, null, true);
                hintTile.State.HintState = HintState.Viewed;
            }
            else
            {
                hintTile.State.HintState = HintState.Cleared;
            }
        }
        else
        {
            var locations = hintTile.Locations!.Select(x => World.FindLocation(x)).ToList();
            if (locations.All(x => x.State.Autotracked || x.State.Cleared))
            {
                hintTile.State.HintState = HintState.Cleared;
                Say(response: Configs.HintTileConfig.ViewedHintTileAlreadyVisited, args: [hintTile.LocationKey]);
            }
            else if (hintTile.Usefulness == LocationUsefulness.Mandatory)
            {
                hintTile.State.HintState = HintState.Viewed;
                Say(response: Configs.HintTileConfig.ViewedHintTileMandatory, args: [hintTile.LocationKey]);
            }
            else if (hintTile.Usefulness == LocationUsefulness.Useless)
            {
                hintTile.State.HintState = HintState.Viewed;
                Say(response: Configs.HintTileConfig.ViewedHintTileUseless, args: [hintTile.LocationKey]);
            }
            else
            {
                hintTile.State.HintState = HintState.Viewed;
                Say(response: Configs.HintTileConfig.ViewedHintTile);
            }
        }

        OnHintTileUpdated(new HintTileUpdatedEventArgs(hintTile));
    }

    public override void UpdateLastMarkedLocations(List<Location> locations)
    {
        LastViewedObject = new ViewedObject { ViewedLocations = locations };
    }

    public override void ClearLastViewedObject(float confidence)
    {
        if (LastViewedObject?.ViewedLocations?.Count > 0)
        {
            LocationTracker.Clear(LastViewedObject.ViewedLocations, confidence);
        }
        else if (LastViewedObject?.HintTile != null)
        {
            var hintTile = LastViewedObject.HintTile;

            if (hintTile?.State == null)
            {
                Say(response: Configs.HintTileConfig.NoPreviousHintTile);
            }
            else if (hintTile.State.HintState != HintState.Cleared && hintTile.Locations?.Count() > 0)
            {
                var locations = hintTile.Locations.Select(x => World.FindLocation(x))
                    .Where(x => x.State is { Cleared: false, Autotracked: false }).ToList();
                if (locations.Count != 0)
                {
                    LocationTracker.Clear(locations, confidence);
                    hintTile.State.HintState = HintState.Cleared;
                    UpdateHintTile(hintTile);
                }
                else
                {
                    Say(response: Configs.HintTileConfig.ClearHintTileFailed);
                }
            }
            else
            {
                Say(response: Configs.HintTileConfig.ClearHintTileFailed);
            }
        }
        else
        {
            Say(x => x.NoMarkedLocations);
        }
    }

    public override void CountHyperBeamShots(int count)
    {
        Say(responses: Responses.CountHyperBeamShots, tieredKey: count, args: [count]);
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
                (_communicator as IDisposable)?.Dispose();

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

    private void Recognizer_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            RestartIdleTimers();
            OnSpeechRecognized(new(e.Result.Confidence, e.Result.Text));
        }
    }
}
