using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BunLabs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Configs;
using Randomizer.Abstractions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.Contracts;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Data.Logic;
using Randomizer.Data.WorldData;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.Tracking;
using Randomizer.SMZ3.Tracking.Services.Speech;

namespace Randomizer.SMZ3.Tracking;

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
    private readonly IWorldService _worldService;
    private readonly ITrackerTimerService _timerService;
    private readonly IMetadataService _metadataService;
    private readonly ISpeechRecognitionService _recognizer;
    private bool _disposed;
    private string? _lastSpokenText;
    private readonly bool _alternateTracker;
    private readonly HashSet<SchrodingersString> _saidLines = new();
    private IEnumerable<ItemType>? _previousMissingItems;
    private List<Location> _lastMarkedLocations = new();

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
    /// <param name="worldService"></param>
    /// <param name="timerService"></param>
    /// <param name="metadataService"></param>
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
        IWorldService worldService,
        ITrackerTimerService timerService,
        IMetadataService metadataService,
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
        _worldService = worldService;
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
        _metadataService = metadataService;
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
    /// Attempts to replace a user name with a pronunciation-corrected
    /// version of it.
    /// </summary>
    /// <param name="userName">The user name to correct.</param>
    /// <returns>
    /// The corrected user name, or <paramref name="userName"/>.
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
                Say(Responses.ActionUndone);
                undoLast.Action();
                OnActionUndone(new TrackerEventArgs(confidence));
            }
            else
            {
                Say(Responses.UndoExpired);
                _undoHistory.Push(undoLast);
            }
        }
        else
        {
            Say(Responses.NothingToUndo);
        }
    }

    /// <summary>
    /// Toggles Go Mode on.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void ToggleGoMode(float? confidence = null)
    {
        ShutUp();
        Say("Toggled Go Mode <break time='1s'/>", wait: true);
        GoMode = true;
        OnGoModeToggledOn(new TrackerEventArgs(confidence));
        Say("on.");

        AddUndo(() =>
        {
            GoMode = false;
            if (Responses.GoModeToggledOff != null)
                Say(Responses.GoModeToggledOff);
        });
    }

    /// <summary>
    /// Removes one or more items from the available treasure in the
    /// specified dungeon.
    /// </summary>
    /// <param name="dungeon">The dungeon.</param>
    /// <param name="amount">The number of treasures to track.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was called by the auto tracker</param>
    /// <param name="stateResponse">If tracker should state the treasure ammount</param>
    /// <returns>
    /// <c>true</c> if treasure was tracked; <c>false</c> if there is no
    /// treasure left to track.
    /// </returns>
    /// <remarks>
    /// This method adds to the undo history if the return value is
    /// <c>true</c>.
    /// </remarks>
    /// <exception cref=" ArgumentOutOfRangeException">
    /// <paramref name="amount"/> is less than 1.
    /// </exception>
    public override bool TrackDungeonTreasure(IDungeon dungeon, float? confidence = null, int amount = 1, bool autoTracked = false, bool stateResponse = true)
    {
        if (amount < 1)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount of items must be greater than zero.");

        if (amount > dungeon.DungeonState.RemainingTreasure && !dungeon.DungeonState.HasManuallyClearedTreasure)
        {
            _logger.LogWarning("Trying to track {Amount} treasures in a dungeon with only {Left} treasures left.", amount, dungeon.DungeonState.RemainingTreasure);
            Say(Responses.DungeonTooManyTreasuresTracked?.Format(dungeon.DungeonMetadata.Name, dungeon.DungeonState.RemainingTreasure, amount));
            return false;
        }

        if (dungeon.DungeonState.RemainingTreasure > 0)
        {
            dungeon.DungeonState.RemainingTreasure -= amount;

            // If there are no more treasures and the boss is defeated, clear all locations in the dungeon
            var clearedLocations = new List<Location>();
            if (dungeon.DungeonState is { RemainingTreasure: 0, Cleared: true })
            {
                foreach (var location in ((Region)dungeon).Locations.Where(x => !x.State.Cleared))
                {
                    location.State.Cleared = true;
                    if (autoTracked)
                    {
                        location.State.Autotracked = true;
                    }
                    clearedLocations.Add(location);
                }
            }

            // Always add a response if there's treasure left, even when
            // clearing a dungeon (because that means it was out of logic
            // and could be relevant)
            if (stateResponse && (confidence != null || dungeon.DungeonState.RemainingTreasure >= 1 || autoTracked))
            {
                // Try to get the response based on the amount of items left
                if (Responses.DungeonTreasureTracked?.TryGetValue(dungeon.DungeonState.RemainingTreasure, out var response) == true)
                    Say(response.Format(dungeon.DungeonMetadata.Name, dungeon.DungeonState.RemainingTreasure));
                // If we don't have a response for the exact amount and we
                // have multiple left, get the one for 2 (considered
                // generic)
                else if (dungeon.DungeonState.RemainingTreasure >= 2 && Responses.DungeonTreasureTracked?.TryGetValue(2, out response) == true)
                    Say(response.Format(dungeon.DungeonMetadata.Name, dungeon.DungeonState.RemainingTreasure));
            }

            OnDungeonUpdated(new DungeonTrackedEventArgs(dungeon, confidence, autoTracked));
            AddUndo(() =>
            {
                dungeon.DungeonState.RemainingTreasure += amount;
                foreach (var location in clearedLocations)
                {
                    location.State.Cleared = false;
                }
            });

            return true;
        }
        else if (stateResponse && confidence != null && Responses.DungeonTreasureTracked?.TryGetValue(-1, out var response) == true)
        {
            // Attempted to track treasure when all treasure items were
            // already cleared out
            Say(response.Format(dungeon.DungeonMetadata.Name));
        }

        return false;
    }

    /// <summary>
    /// Sets the dungeon's reward to the specific pendant or crystal.
    /// </summary>
    /// <param name="dungeon">The dungeon to mark.</param>
    /// <param name="reward">
    /// The type of pendant or crystal, or <c>null</c> to cycle through the
    /// possible rewards.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was called by the auto tracker</param>
    public override void SetDungeonReward(IDungeon dungeon, RewardType? reward = null, float? confidence = null, bool autoTracked = false)
    {
        var originalReward = dungeon.DungeonState.MarkedReward;
        if (reward == null)
        {
            var currentValue = dungeon.DungeonState.MarkedReward ?? RewardType.None;
            dungeon.DungeonState.MarkedReward = Enum.IsDefined(currentValue + 1) ? currentValue + 1 : RewardType.None;
            // Cycling through rewards is done via UI, so speaking the
            // reward out loud for multiple clicks is kind of annoying
        }
        else
        {
            dungeon.DungeonState.MarkedReward = reward.Value;
            var rewardObj = ItemService.FirstOrDefault(reward.Value);
            Say(Responses.DungeonRewardMarked?.Format(dungeon.DungeonMetadata.Name, rewardObj?.Metadata.Name ?? reward.GetDescription()));
        }

        OnDungeonUpdated(new DungeonTrackedEventArgs(dungeon, confidence, autoTracked));

        if (!autoTracked) AddUndo(() => dungeon.DungeonState.MarkedReward = originalReward);
    }

    /// <summary>
    /// Sets the reward of all unmarked dungeons.
    /// </summary>
    /// <param name="reward">The reward to set.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void SetUnmarkedDungeonReward(RewardType reward, float? confidence = null)
    {
        var unmarkedDungeons = World.Dungeons
            .Where(x => x.DungeonState is { HasReward: true, HasMarkedReward: false })
            .ToImmutableList();

        if (unmarkedDungeons.Count > 0)
        {
            Say(Responses.RemainingDungeonsMarked?.Format(ItemService.GetName(reward)));
            unmarkedDungeons.ForEach(dungeon => dungeon.DungeonState.MarkedReward = reward);
            AddUndo(() => unmarkedDungeons.ForEach(dungeon => dungeon.DungeonState.MarkedReward = RewardType.None));
            OnDungeonUpdated(new DungeonTrackedEventArgs(null, confidence, false));
        }
        else
        {
            Say(Responses.NoRemainingDungeons);
        }
    }

    /// <summary>
    /// Sets the dungeon's medallion requirement to the specified item.
    /// </summary>
    /// <param name="dungeon">The dungeon to mark.</param>
    /// <param name="medallion">The medallion that is required.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked dungeon requirement was autotracked</param>
    public override void SetDungeonRequirement(IDungeon dungeon, ItemType? medallion = null, float? confidence = null, bool autoTracked = false)
    {
        var region = World.Regions.SingleOrDefault(x => dungeon.DungeonMetadata.Name?.Contains(x.Name, StringComparison.OrdinalIgnoreCase) == true);
        if (region == null)
        {
            Say("Strange, I can't find that dungeon in this seed.");
        }
        else if (region is not INeedsMedallion)
        {
            Say(Responses.DungeonRequirementInvalid?.Format(dungeon.DungeonMetadata.Name));
            return;
        }

        var originalRequirement = dungeon.DungeonState.MarkedMedallion ?? ItemType.Nothing;
        if (medallion == null)
        {
            var medallionItems = new List<ItemType>(Enum.GetValues<ItemType>());
            medallionItems.Insert(0, ItemType.Nothing);
            var index = (medallionItems.IndexOf(originalRequirement) + 1) % medallionItems.Count;
            dungeon.DungeonState.MarkedMedallion = medallionItems[index];
            OnDungeonUpdated(new DungeonTrackedEventArgs(null, confidence, false));
        }
        else
        {
            if (region is INeedsMedallion medallionRegion
                && medallionRegion.Medallion != ItemType.Nothing
                && medallionRegion.Medallion != medallion.Value
                && confidence >= Options.MinimumSassConfidence)
            {
                Say(Responses.DungeonRequirementMismatch?.Format(
                    HintsEnabled ? "a different medallion" : medallionRegion.Medallion.ToString(),
                    dungeon.DungeonMetadata.Name,
                    medallion.Value.ToString()));
            }

            dungeon.DungeonState.MarkedMedallion = medallion.Value;
            Say(Responses.DungeonRequirementMarked?.Format(medallion.ToString(), dungeon.DungeonMetadata.Name));
            OnDungeonUpdated(new DungeonTrackedEventArgs(dungeon, confidence, false));
        }

        AddUndo(() => dungeon.DungeonState.MarkedMedallion = originalRequirement);
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

        Say(_alternateTracker ? Responses.StartingTrackingAlternate : Responses.StartedTracking);
        RestartIdleTimers();
        return !loadError;
    }

    /// <summary>
    /// Connects Tracker to chat.
    /// </summary>
    /// <param name="userName">The user name to connect as.</param>
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
            Say(Responses.TimerResumed);
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
            Say(Responses.TimerReset);
            AddUndo(() => _timerService.Undo());
        }
    }

    /// <summary>
    /// Pauses the timer, saving the elapsed time
    /// </summary>
    public override Action? PauseTimer(bool addUndo = true)
    {
        _timerService.StopTimer();

        Say(Responses.TimerPaused);

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
        Say(GoMode ? Responses.StoppedTrackingPostGoMode : Responses.StoppedTracking, wait: true);

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
        }
    }

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="text">The possible sentences to speak.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if <paramref
    /// name="text"/> was <c>null</c>.
    /// </returns>
    public override bool Say(SchrodingersString? text)
    {
        if (text == null)
            return false;

        return Say(text.ToString());
    }

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public override bool Say(Func<ResponseConfig, SchrodingersString?> selectResponse)
    {
        return Say(selectResponse(Responses));
    }

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="text">The possible sentences to speak.</param>
    /// <param name="args">The arguments used to format the text.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if <paramref
    /// name="text"/> was <c>null</c>.
    /// </returns>
    public override bool Say(SchrodingersString? text, params object?[] args)
    {
        if (text == null)
            return false;

        return Say(text.Format(args), wait: false);
    }

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <param name="args">The arguments used to format the text.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public override bool Say(Func<ResponseConfig, SchrodingersString?> selectResponse, params object?[] args)
    {
        return Say(selectResponse(Responses), args);
    }

    /// <summary>
    /// Speak a sentence using text-to-speech only one time.
    /// </summary>
    /// <param name="text">The possible sentences to speak.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if <paramref
    /// name="text"/> was <c>null</c>.
    /// </returns>
    public override bool SayOnce(SchrodingersString? text)
    {
        if (text == null)
            return false;

        if (!_saidLines.Contains(text))
        {
            _saidLines.Add(text);
            return Say(text.ToString());
        }

        return true;
    }

    /// <summary>
    /// Speak a sentence using text-to-speech only one time.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public override bool SayOnce(Func<ResponseConfig, SchrodingersString?> selectResponse)
    {
        return SayOnce(selectResponse(Responses));
    }

    /// <summary>
    /// Speak a sentence using text-to-speech only one time.
    /// </summary>
    /// <param name="text">The possible sentences to speak.</param>
    /// <param name="args">The arguments used to format the text.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if <paramref
    /// name="text"/> was <c>null</c>.
    /// </returns>
    public override bool SayOnce(SchrodingersString? text, params object?[] args)
    {
        if (text == null)
            return false;

        if (!_saidLines.Contains(text))
        {
            _saidLines.Add(text);
            return Say(text.Format(args), wait: false);
        }

        return true;
    }

    /// <summary>
    /// Speak a sentence using text-to-speech only one time.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <param name="args">The arguments used to format the text.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public override bool SayOnce(Func<ResponseConfig, SchrodingersString?> selectResponse, params object?[] args)
    {
        return SayOnce(selectResponse(Responses), args);
    }

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="text">The phrase to speak.</param>
    /// <param name="wait">
    /// <c>true</c> to wait until the text has been spoken completely or
    /// <c>false</c> to immediately return. The default is <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public override bool Say(string? text, bool wait = false)
    {
        if (text == null)
            return false;

        var formattedText = FormatPlaceholders(text);
        if (wait)
            _communicator.SayWait(formattedText);
        else
            _communicator.Say(formattedText);
        _lastSpokenText = text;
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
            Say("I haven't said anything yet.");
            return;
        }

        _communicator.SayWait("I said");
        _communicator.SlowDown();
        Say(_lastSpokenText, wait: true);
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
        Say(Responses.Error);
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
    /// Tracks the specifies item.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="tryClear">
    /// <see langword="true"/> to attempt to clear a location for the
    /// tracked item; <see langword="false"/> if that is done by the caller.
    /// </param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    /// <param name="location">The location an item was tracked from</param>
    /// <param name="giftedItem">If the item was gifted to the player by tracker or another player</param>
    /// <param name="silent">If tracker should not say anything</param>
    /// <returns>
    /// <see langword="true"/> if the item was actually tracked; <see
    /// langword="false"/> if the item could not be tracked, e.g. when
    /// tracking Bow twice.
    /// </returns>
    public override bool TrackItem(Item item, string? trackedAs = null, float? confidence = null, bool tryClear = true, bool autoTracked = false, Location? location = null, bool giftedItem = false, bool silent = false)
    {
        var didTrack = false;
        var accessibleBefore = _worldService.AccessibleLocations(false);
        var itemName = item.Name;
        var originalTrackingState = item.State.TrackingState;
        ItemService.ResetProgression();

        var isGTPreBigKey = !World.Config.ZeldaKeysanity
                            && autoTracked
                            && location?.Region.GetType() == typeof(GanonsTower)
                            && !ItemService.GetProgression(false).BigKeyGT;
        var stateResponse = !isGTPreBigKey && !silent && (!autoTracked
                                                          || !item.Metadata.IsDungeonItem()
                                                          || World.Config.ZeldaKeysanity);

        // Actually track the item if it's for the local player's world
        if (item.World == World)
        {
            if (item.Metadata.HasStages)
            {
                if (trackedAs != null && item.Metadata.GetStage(trackedAs) != null)
                {
                    var stage = item.Metadata.GetStage(trackedAs)!;

                    // Tracked by specific stage name (e.g. Tempered Sword), set
                    // to that stage specifically
                    var stageName = item.Metadata.Stages[stage.Value].ToString();

                    didTrack = item.Track(stage.Value);
                    if (stateResponse)
                    {
                        if (didTrack)
                        {
                            if (item.TryGetTrackingResponse(out var response))
                            {
                                Say(response.Format(item.Counter));
                            }
                            else
                            {
                                Say(Responses.TrackedItemByStage?.Format(itemName, stageName));
                            }
                        }
                        else
                        {
                            Say(Responses.TrackedOlderProgressiveItem?.Format(itemName, item.Metadata.Stages[item.State.TrackingState].ToString()));
                        }
                    }
                }
                else
                {
                    // Tracked by regular name, upgrade by one step
                    didTrack = item.Track();
                    if (stateResponse)
                    {
                        if (didTrack)
                        {
                            if (item.TryGetTrackingResponse(out var response))
                            {
                                Say(response.Format(item.Counter));
                            }
                            else
                            {
                                var stageName = item.Metadata.Stages[item.State.TrackingState].ToString();
                                Say(Responses.TrackedProgressiveItem?.Format(itemName, stageName));
                            }
                        }
                        else
                        {
                            Say(Responses.TrackedTooManyOfAnItem?.Format(itemName));
                        }
                    }
                }
            }
            else if (item.Metadata.Multiple)
            {
                didTrack = item.Track();
                if (item.TryGetTrackingResponse(out var response))
                {
                    if (stateResponse)
                        Say(response.Format(item.Counter));
                }
                else if (item.Counter == 1)
                {
                    if (stateResponse)
                        Say(Responses.TrackedItem?.Format(itemName, item.Metadata.NameWithArticle));
                }
                else if (item.Counter > 1)
                {
                    if (stateResponse)
                        Say(Responses.TrackedItemMultiple?.Format(item.Metadata.Plural ?? $"{itemName}s", item.Counter, item.Name));
                }
                else
                {
                    _logger.LogWarning("Encountered multiple item with counter 0: {Item} has counter {Counter}", item, item.Counter);
                    if (stateResponse)
                        Say(Responses.TrackedItem?.Format(itemName, item.Metadata.NameWithArticle));
                }
            }
            else
            {
                didTrack = item.Track();
                if (stateResponse)
                {
                    if (didTrack)
                    {
                        if (item.TryGetTrackingResponse(out var response))
                        {
                            Say(response.Format(item.Counter));
                        }
                        else
                        {
                            Say(Responses.TrackedItem?.Format(itemName, item.Metadata.NameWithArticle));
                        }
                    }
                    else
                    {
                        Say(Responses.TrackedAlreadyTrackedItem?.Format(itemName));
                    }
                }
            }
        }

        var undoTrack = () => { item.State.TrackingState = originalTrackingState; ItemService.ResetProgression(); };
        OnItemTracked(new ItemTrackedEventArgs(item, trackedAs, confidence, autoTracked));

        // Check if we can clear a location
        Action? undoClear = null;
        Action? undoTrackDungeonTreasure = null;


        // If this was not gifted to the player, try to clear the location
        if (!giftedItem && item.Type != ItemType.Nothing)
        {
            if (location == null && !World.Config.MultiWorld)
            {
                location = _worldService.Locations(outOfLogic: true, itemFilter: item.Type).TrySingle();
            }

            // Clear the location if it's for the local player's world
            if (location != null && location.World == World && location.State.Cleared == false)
            {
                if (stateResponse)
                {
                    GiveLocationComment(item.Type, location, isTracking: true, confidence, item.Metadata);
                    GivePreConfiguredLocationSass(location);
                }

                if (tryClear)
                {
                    // If this item was in a dungeon, track treasure count
                    undoTrackDungeonTreasure = TryTrackDungeonTreasure(location, confidence, autoTracked, stateResponse);

                    // Important: clear only after tracking dungeon treasure, as
                    // the "guess dungeon from location" algorithm excludes
                    // cleared items
                    location.State.Cleared = true;
                    World.LastClearedLocation = location;
                    OnLocationCleared(new(location, confidence, autoTracked));

                    undoClear = () => location.State.Cleared = false;
                    if ((location.State.MarkedItem ?? ItemType.Nothing) != ItemType.Nothing)
                    {
                        location.State.MarkedItem = null;
                        OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
                    }
                }

                var isKeysanityForLocation = (location.Region is Z3Region && World.Config.ZeldaKeysanity) || (location.Region is SMRegion && World.Config.MetroidKeysanity);
                var items = ItemService.GetProgression(!isKeysanityForLocation);
                if (stateResponse && !location.IsAvailable(items) && (confidence >= Options.MinimumSassConfidence || autoTracked))
                {
                    var locationInfo = location.Metadata;
                    var roomInfo = location.Room?.Metadata;
                    var regionInfo = location.Region.Metadata;

                    if (locationInfo.OutOfLogic != null)
                    {
                        Say(locationInfo.OutOfLogic);
                    }
                    else if (roomInfo?.OutOfLogic != null)
                    {
                        Say(roomInfo.OutOfLogic);
                    }
                    else if (regionInfo.OutOfLogic != null)
                    {
                        Say(regionInfo.OutOfLogic);
                    }
                    else
                    {
                        var allMissingCombinations = Logic.GetMissingRequiredItems(location, items, out var allMissingItems);
                        allMissingItems = allMissingItems.OrderBy(x => x);

                        var missingItems = allMissingCombinations.MinBy(x => x.Length);
                        var allPossibleMissingItems = allMissingItems.ToList();
                        if (missingItems == null)
                        {
                            Say(x => x.TrackedOutOfLogicItemTooManyMissing, item.Metadata.Name, locationInfo.Name);
                        }
                        // Do not say anything if the only thing missing are keys
                        else
                        {
                            var itemsChanged = _previousMissingItems == null || !allPossibleMissingItems.SequenceEqual(_previousMissingItems);
                            var onlyKeys = allPossibleMissingItems.All(x => x.IsInAnyCategory(ItemCategory.BigKey, ItemCategory.SmallKey, ItemCategory.Keycard));
                            _previousMissingItems = allPossibleMissingItems;

                            if (itemsChanged && !onlyKeys)
                            {
                                var missingItemNames = NaturalLanguage.Join(missingItems.Select(ItemService.GetName));
                                Say(x => x.TrackedOutOfLogicItem, item.Metadata.Name, locationInfo.Name, missingItemNames);
                            }
                        }

                        _previousMissingItems = allPossibleMissingItems;
                    }

                }
            }
        }

        var addedEvent = History.AddEvent(
            HistoryEventType.TrackedItem,
            item.Metadata.IsProgression(World.Config),
            item.Metadata.NameWithArticle,
            location
        );

        IsDirty = true;

        if (!autoTracked)
        {
            AddUndo(() =>
            {
                undoTrack();
                undoClear?.Invoke();
                undoTrackDungeonTreasure?.Invoke();
                ItemService.ResetProgression();
                addedEvent.IsUndone = true;
            });
        }

        GiveLocationHint(accessibleBefore);
        RestartIdleTimers();

        return didTrack;
    }

    /// <summary>
    /// Makes Tracker respond to a location if it was pre-configured by the user.
    /// </summary>
    /// <param name="location">The location at which an item was marked or tracked.</param>
    /// <param name="marking"><see langword="true"/> if marking, <see langword="false"/> if tracking.</param>
    private void GivePreConfiguredLocationSass(Location location, bool marking = false)
    {
        // "What a surprise."
        if (LocalConfig != null && LocalConfig.LocationItems.ContainsKey(location.Id))
        {
            // I guess we're not storing the other options? We could respond to those, too, if we had those here.
            Say(x => marking ? x.LocationMarkedPreConfigured : x.TrackedPreConfigured, location.Metadata.Name);
        }
    }

    /// <summary>
    /// Tracks multiple items at the same time
    /// </summary>
    /// <param name="items">The items to track</param>
    /// <param name="autoTracked">If the items were tracked via auto tracker</param>
    /// <param name="giftedItem">If the items were gifted to the player</param>
    public override void TrackItems(List<Item> items, bool autoTracked, bool giftedItem)
    {
        if (items.Count == 1)
        {
            TrackItem(items.First(), null, null, false, autoTracked, null, giftedItem);
            return;
        }

        ItemService.ResetProgression();

        foreach (var item in items)
        {
            item.Track();
        }

        if (items.Count == 2)
        {
            Say(x => x.TrackedTwoItems, items[0].Metadata.Name, items[1].Metadata.Name);
        }
        else if (items.Count == 3)
        {
            Say(x => x.TrackedThreeItems, items[0].Metadata.Name, items[1].Metadata.Name, items[2].Metadata.Name);
        }
        else
        {
            var itemsToSay = items.Where(x => x.Type.IsPossibleProgression(World.Config.ZeldaKeysanity, World.Config.MetroidKeysanity)).Take(2).ToList();
            if (itemsToSay.Count() < 2)
            {
                var numToTake = 2 - itemsToSay.Count();
                itemsToSay.AddRange(items.Where(x => !x.Type.IsPossibleProgression(World.Config.ZeldaKeysanity, World.Config.MetroidKeysanity)).Take(numToTake));
            }

            Say(x => x.TrackedManyItems, itemsToSay[0].Metadata.Name, itemsToSay[1].Metadata.Name, items.Count - 2);
        }

        OnItemTracked(new ItemTrackedEventArgs(null, null, null, true));
        IsDirty = true;
        RestartIdleTimers();
    }

    /// <summary>
    /// Removes an item from the tracker.
    /// </summary>
    /// <param name="item">The item to untrack.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void UntrackItem(Item item, float? confidence = null)
    {
        var originalTrackingState = item.State.TrackingState;
        ItemService.ResetProgression();

        if (!item.Untrack())
        {
            Say(Responses.UntrackedNothing?.Format(item.Name, item.Metadata.NameWithArticle));
            return;
        }

        if (item.Metadata.HasStages)
        {
            Say(Responses.UntrackedProgressiveItem?.Format(item.Name, item.Metadata.NameWithArticle));
        }
        else if (item.Metadata.Multiple)
        {
            if (item.State.TrackingState > 0)
            {
                if (item.Metadata.CounterMultiplier > 1)
                    Say(Responses.UntrackedItemMultiple?.Format($"{item.Metadata.CounterMultiplier} {item.Metadata.Plural}", $"{item.Metadata.CounterMultiplier} {item.Metadata.Plural}"));
                else
                    Say(Responses.UntrackedItemMultiple?.Format(item.Name, item.Metadata.NameWithArticle));
            }
            else
                Say(Responses.UntrackedItemMultipleLast?.Format(item.Name, item.Metadata.NameWithArticle));
        }
        else
        {
            Say(Responses.UntrackedItem?.Format(item.Name, item.Metadata.NameWithArticle));
        }

        IsDirty = true;
        OnItemTracked(new(item, null, confidence, false));
        AddUndo(() => { item.State.TrackingState = originalTrackingState; ItemService.ResetProgression(); });
    }

    /// <summary>
    /// Tracks the specifies item and clears it from the specified dungeon.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="dungeon">The dungeon the item was tracked in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void TrackItem(Item item, IDungeon dungeon, string? trackedAs = null, float? confidence = null)
    {
        var tracked = TrackItem(item, trackedAs, confidence, tryClear: false);
        var undoTrack = _undoHistory.Pop();
        ItemService.ResetProgression();

        // Check if we can remove something from the remaining treasures in
        // a dungeon
        Action? undoTrackTreasure = null;
        if (tracked) // Only track treasure if we actually tracked anything
        {
            dungeon = GetDungeonFromItem(item, dungeon)!;
            if (TrackDungeonTreasure(dungeon, confidence))
                undoTrackTreasure = _undoHistory.Pop().Action;
        }

        IsDirty = true;

        // Check if we can remove something from the marked location
        var location = _worldService.Locations(itemFilter: item.Type, inRegion: dungeon as Region).TrySingle();
        if (location != null)
        {
            location.State.Cleared = true;
            World.LastClearedLocation = location;
            OnLocationCleared(new(location, confidence, false));

            if (location.State.HasMarkedItem)
            {
                location.State.MarkedItem = null;
                OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
            }

            AddUndo(() =>
            {
                undoTrack.Action();
                undoTrackTreasure?.Invoke();
                location.State.Cleared = false;
                ItemService.ResetProgression();
            });
        }
        else
        {
            AddUndo(() =>
            {
                undoTrack.Action();
                undoTrackTreasure?.Invoke();
                ItemService.ResetProgression();
            });
        }
    }

    /// <summary>
    /// Tracks the specified item and clears it from the specified room.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="area">The area the item was found in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void TrackItem(Item item, IHasLocations area, string? trackedAs = null, float? confidence = null)
    {
        var locations = area.Locations
            .Where(x => x.Item.Type == item.Type)
            .ToImmutableList();
        ItemService.ResetProgression();

        if (locations.Count == 0)
        {
            Say(Responses.AreaDoesNotHaveItem?.Format(item.Name, area.Name, item.Metadata.NameWithArticle));
        }
        else if (locations.Count > 1)
        {
            // Consider tracking/clearing everything?
            Say(Responses.AreaHasMoreThanOneItem?.Format(item.Name, area.Name, item.Metadata.NameWithArticle));
        }

        IsDirty = true;

        TrackItem(item, trackedAs, confidence, tryClear: false);
        if (locations.Count == 1)
        {
            Clear(locations.Single());
            var undoClear = _undoHistory.Pop();
            var undoTrack = _undoHistory.Pop();
            AddUndo(() =>
            {
                undoClear.Action();
                undoTrack.Action();
                ItemService.ResetProgression();
            });
        }
    }

    /// <summary>
    /// Sets the item count for the specified item.
    /// </summary>
    /// <param name="item">The item to track.</param>
    /// <param name="count">
    /// The amount of the item that is in the player's inventory now.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void TrackItemAmount(Item item, int count, float confidence)
    {
        ItemService.ResetProgression();

        var newItemCount = count;
        if (item.Metadata.CounterMultiplier > 1
            && count % item.Metadata.CounterMultiplier == 0)
        {
            newItemCount = count / item.Metadata.CounterMultiplier.Value;
        }

        var oldItemCount = item.State.TrackingState;
        if (newItemCount == oldItemCount)
        {
            Say(Responses.TrackedExactAmountDuplicate?.Format(item.Metadata.Plural, count));
            return;
        }

        item.State.TrackingState = newItemCount;
        if (item.TryGetTrackingResponse(out var response))
        {
            Say(response.Format(item.Counter));
        }
        else if (newItemCount > oldItemCount)
        {
            Say(Responses.TrackedItemMultiple?.Format(item.Metadata.Plural ?? $"{item.Name}s", item.Counter, item.Name));
        }
        else
        {
            Say(Responses.UntrackedItemMultiple?.Format(item.Metadata.Plural ?? $"{item.Name}s", item.Metadata.Plural ?? $"{item.Name}s"));
        }

        IsDirty = true;

        AddUndo(() => { item.State.TrackingState = oldItemCount; ItemService.ResetProgression(); });
        OnItemTracked(new(item, null, confidence, false));
    }

    /// <summary>
    /// Clears every item in the specified area, optionally tracking the
    /// cleared items.
    /// </summary>
    /// <param name="area">The area whose items to clear.</param>
    /// <param name="trackItems">
    /// <c>true</c> to track any items found; <c>false</c> to only clear the
    /// affected locations.
    /// </param>
    /// <param name="includeUnavailable">
    /// <c>true</c> to include every item in <paramref name="area"/>, even
    /// those that are not in logic. <c>false</c> to only include chests
    /// available with current items.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="assumeKeys">
    /// Set to true to ignore keys when clearing the location.
    /// </param>
    public override void ClearArea(IHasLocations area, bool trackItems, bool includeUnavailable = false, float? confidence = null, bool assumeKeys = false)
    {
        var locations = area.Locations
            .Where(x => x.State.Cleared == false)
            .WhereUnless(includeUnavailable, x => x.IsAvailable(ItemService.GetProgression(area)))
            .ToImmutableList();

        ItemService.ResetProgression();

        if (locations.Count == 0)
        {
            var outOfLogicLocations = area.Locations
                .Count(x => x.State.Cleared == false);

            if (outOfLogicLocations > 1)
                Say(Responses.TrackedNothingOutOfLogic?[2].Format(area.Name, outOfLogicLocations));
            else if (outOfLogicLocations > 0)
                Say(Responses.TrackedNothingOutOfLogic?[1].Format(area.Name, outOfLogicLocations));
            else
                Say(Responses.TrackedNothing?.Format(area.Name));
        }
        else
        {
            // If there is only one (available) item here, just call the
            // regular TrackItem instead
            var onlyLocation = locations.TrySingle();
            if (onlyLocation != null)
            {
                if (!trackItems)
                {
                    Clear(onlyLocation, confidence);
                }
                else
                {
                    var item = onlyLocation.Item;
                    TrackItem(item: item, trackedAs: null, confidence: confidence, tryClear: true, autoTracked: false, location: onlyLocation);
                }
            }
            else
            {
                // Otherwise, start counting
                var itemsCleared = 0;
                var itemsTracked = new List<Item>();
                var treasureTracked = 0;
                foreach (var location in locations)
                {
                    itemsCleared++;
                    if (!trackItems)
                    {
                        if (IsTreasure(location.Item) || World.Config.ZeldaKeysanity)
                            treasureTracked++;
                        location.State.Cleared = true;
                        World.LastClearedLocation = location;
                        OnLocationCleared(new(location, confidence, false));
                        continue;
                    }

                    var item = location.Item;
                    if (!item.Track())
                        _logger.LogWarning("Failed to track {ItemType} in {Area}.", item.Name, area.Name); // Probably the compass or something, who cares
                    else
                        itemsTracked.Add(item);
                    if (IsTreasure(location.Item) || World.Config.ZeldaKeysanity)
                        treasureTracked++;

                    location.State.Cleared = true;
                }

                if (trackItems)
                {
                    var itemNames = confidence >= Options.MinimumSassConfidence
                        ? NaturalLanguage.Join(itemsTracked, World.Config)
                        : $"{itemsCleared} items";
                    Say(x => x.TrackedMultipleItems, itemsCleared, area.Name, itemNames);

                    var roomInfo = area is Room room ? room.Metadata : null;
                    var regionInfo = area is Region region ? region.Metadata : null;

                    if (roomInfo?.OutOfLogic != null)
                    {
                        Say(roomInfo.OutOfLogic);
                    }
                    else if (regionInfo?.OutOfLogic != null)
                    {
                        Say(regionInfo.OutOfLogic);
                    }
                    else
                    {
                        var progression = ItemService.GetProgression(area);
                        var someOutOfLogicLocation = locations.Where(x => !x.IsAvailable(progression)).Random(s_random);
                        if (someOutOfLogicLocation != null && confidence >= Options.MinimumSassConfidence)
                        {
                            var someOutOfLogicItem = someOutOfLogicLocation.Item;
                            var missingItems = Logic.GetMissingRequiredItems(someOutOfLogicLocation, progression, out _).MinBy(x => x.Length);
                            if (missingItems != null)
                            {
                                var missingItemNames = NaturalLanguage.Join(missingItems.Select(ItemService.GetName));
                                Say(x => x.TrackedOutOfLogicItem, someOutOfLogicItem.Metadata.Name, someOutOfLogicLocation.Metadata.Name, missingItemNames);
                            }
                            else
                            {
                                Say(x => x.TrackedOutOfLogicItemTooManyMissing, someOutOfLogicItem.Metadata.Name, someOutOfLogicLocation.Metadata.Name);
                            }
                        }
                    }
                }
                else
                {
                    Say(x => x.ClearedMultipleItems, itemsCleared, area.Name);
                }

                if (treasureTracked > 0)
                {
                    var dungeon = GetDungeonFromArea(area);
                    if (dungeon != null)
                    {
                        TrackDungeonTreasure(dungeon, amount: treasureTracked);
                    }
                }
            }

            OnItemTracked(new ItemTrackedEventArgs(null, null, confidence, false));
        }

        IsDirty = true;

        AddUndo(() =>
        {
            foreach (var location in locations)
            {
                if (trackItems)
                {
                    var item = location.Item;
                    if (item.Type != ItemType.Nothing && item.State.TrackingState > 0)
                        item.State.TrackingState--;
                }

                location.State.Cleared = false;
            }
            ItemService.ResetProgression();
        });
    }

    /// <summary>
    /// Marks all locations and treasure within a dungeon as cleared.
    /// </summary>
    /// <param name="dungeon">The dungeon to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void ClearDungeon(IDungeon dungeon, float? confidence = null)
    {
        var remaining = dungeon.DungeonState.RemainingTreasure;
        if (remaining > 0)
        {
            dungeon.DungeonState.RemainingTreasure = 0;
        }

        // Clear the dungeon only if there's no bosses to defeat
        if (!dungeon.DungeonState.HasReward)
            dungeon.DungeonState.Cleared = true;

        var region = (Region)dungeon;
        var progress = ItemService.GetProgression(assumeKeys: !World.Config.ZeldaKeysanity);
        var locations = region.Locations.Where(x => x.State.Cleared == false).ToList();
        var inaccessibleLocations = locations.Where(x => !x.IsAvailable(progress)).ToList();
        if (locations.Count > 0)
        {
            foreach (var state in locations.Select(x => x.State).NonNull())
            {
                state.Cleared = true;
            }
        }

        if (remaining <= 0 && locations.Count <= 0)
        {
            // We didn't do anything
            Say(x => x.DungeonAlreadyCleared, dungeon.DungeonMetadata.Name);
            return;
        }

        Say(x => x.DungeonCleared, dungeon.DungeonMetadata.Name);
        if (inaccessibleLocations.Count > 0 && confidence >= Options.MinimumSassConfidence)
        {
            var anyMissedLocation = inaccessibleLocations.Random(s_random) ?? inaccessibleLocations.First();
            var locationInfo = anyMissedLocation.Metadata;
            var missingItemCombinations = Logic.GetMissingRequiredItems(anyMissedLocation, progress, out _).ToList();
            if (missingItemCombinations.Any())
            {
                var missingItems = (missingItemCombinations.Random(s_random) ?? missingItemCombinations.First())
                    .Select(ItemService.FirstOrDefault)
                    .NonNull();
                var missingItemsText = NaturalLanguage.Join(missingItems, World.Config);
                Say(x => x.DungeonClearedWithInaccessibleItems, dungeon.DungeonMetadata.Name, locationInfo.Name, missingItemsText);
            }
            else
            {
                Say(x => x.DungeonClearedWithTooManyInaccessibleItems, dungeon.DungeonMetadata.Name, locationInfo.Name);
            }
        }
        ItemService.ResetProgression();

        OnDungeonUpdated(new(dungeon, confidence, false));
        AddUndo(() =>
        {
            dungeon.DungeonState.RemainingTreasure = remaining;
            if (remaining > 0 && !dungeon.DungeonState.HasReward)
                dungeon.DungeonState.Cleared = false;
            foreach (var state in locations.Select(x => x.State).NonNull())
            {
                state.Cleared = false;
            }
            ItemService.ResetProgression();
        });
    }

    /// <summary>
    /// Clears an item from the specified location.
    /// </summary>
    /// <param name="location">The location to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    public override void Clear(Location location, float? confidence = null, bool autoTracked = false)
    {
        ItemService.ResetProgression();
        location.State.Cleared = true;

        if (confidence != null)
        {
            // Only use TTS if called from a voice command
            var locationName = location.Metadata.Name;
            Say(Responses.LocationCleared?.Format(locationName));
        }

        ItemType? prevMarkedItem = null;
        if (location.State.HasMarkedItem)
        {
            prevMarkedItem = location.State.MarkedItem;
            location.State.MarkedItem = null;
            OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
        }

        var undoTrackTreasure = TryTrackDungeonTreasure(location, confidence);

        Action? undoStopPegWorldMode = null;
        if (location == World.DarkWorldNorthWest.PegWorld)
        {
            StopPegWorldMode();

            if (!autoTracked)
            {
                undoStopPegWorldMode = _undoHistory.Pop().Action;
            }
        }

        IsDirty = true;

        if (!autoTracked)
        {
            AddUndo(() =>
            {
                location.State.Cleared = false;
                location.State.MarkedItem = prevMarkedItem;
                if (prevMarkedItem != null)
                {
                    OnMarkedLocationsUpdated(new TrackerEventArgs(null));
                }
                undoTrackTreasure?.Invoke();
                undoStopPegWorldMode?.Invoke();
                ItemService.ResetProgression();
            });
        }

        World.LastClearedLocation = location;
        OnLocationCleared(new(location, confidence, autoTracked));
    }

    /// <summary>
    /// Clears an item from the specified locations.
    /// </summary>
    /// <param name="locations">The locations to clear.</param>
    /// <param name="confidence">The speech recognition confidence</param>
    public override void Clear(List<Location> locations, float? confidence = null)
    {
        if (locations.Count == 1)
        {
            Clear(locations.First(), confidence);
            return;
        }

        ItemService.ResetProgression();

        var originalClearedValues = new Dictionary<Location, bool>();
        var originalMarkedItems = new Dictionary<Location, ItemType>();

        // Clear the locations
        foreach (var location in locations)
        {
            originalClearedValues.Add(location, location.State.Cleared);
            if (location.State.MarkedItem != null)
            {
                originalMarkedItems.Add(location, location.State.MarkedItem.Value);
            }
            location.State.Cleared = true;
            location.State.MarkedItem = null;
        }

        Action? undoDungeonTreasure = null;
        if (locations.Select(x => x.Region).Distinct().Count() == 1)
        {
            Say(x => x.LocationsClearedSameRegion, locations.Count, locations.First().Region.GetName());
            if (locations.First().Region is IDungeon dungeon)
            {
                var treasureCount = locations.Count(x => IsTreasure(x.Item) || World.Config.ZeldaKeysanity);
                if (TrackDungeonTreasure(dungeon, confidence, treasureCount))
                    undoDungeonTreasure = _undoHistory.Pop().Action;
            }
        }
        else
        {
            Say(x => x.LocationsCleared, locations.Count);
        }

        AddUndo(() =>
        {
            foreach (var location in locations)
            {
                location.State.Cleared = originalClearedValues[location];
                if (originalMarkedItems.TryGetValue(location, out var item))
                {
                    location.State.MarkedItem = item;
                }
            }
            undoDungeonTreasure?.Invoke();
            ItemService.ResetProgression();
        });

        World.LastClearedLocation = locations.First();
        if (originalMarkedItems.Count > 0)
        {
            OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
        }
        IsDirty = true;
        OnLocationCleared(new(locations.First(), confidence, false));
    }

    /// <summary>
    /// Marks a dungeon as cleared and, if possible, tracks the boss reward.
    /// </summary>
    /// <param name="dungeon">The dungeon that was cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was cleared by the auto tracker</param>
    public override void MarkDungeonAsCleared(IDungeon dungeon, float? confidence = null, bool autoTracked = false)
    {
        if (dungeon.DungeonState.Cleared)
        {
            if (!autoTracked)
                Say(Responses.DungeonBossAlreadyCleared?.Format(dungeon.DungeonMetadata.Name, dungeon.DungeonMetadata.Boss));
            else
                OnDungeonUpdated(new DungeonTrackedEventArgs(dungeon, confidence, autoTracked));

            return;
        }

        ItemService.ResetProgression();

        var addedEvent = History.AddEvent(
            HistoryEventType.BeatBoss,
            true,
            dungeon.DungeonMetadata.Boss?.ToString() ?? $"boss of {dungeon.DungeonMetadata.Name}"
        );

        // If all treasures have been retrieved and the boss is defeated, clear all locations in the dungeon
        var clearedLocations = new List<Location>();
        if (dungeon.DungeonState.RemainingTreasure == 0)
        {
            foreach (var location in ((Region)dungeon).Locations.Where(x => !x.State.Cleared))
            {
                location.State.Cleared = true;
                if (autoTracked)
                {
                    location.State.Autotracked = true;
                }
                clearedLocations.Add(location);
            }
        }

        // Auto track the dungeon reward if not already marked
        if (autoTracked && dungeon.DungeonState.MarkedReward != dungeon.DungeonState.Reward)
        {
            dungeon.DungeonState.MarkedReward = dungeon.DungeonState.Reward;
            SetDungeonReward(dungeon, dungeon.DungeonState.Reward);
        }

        dungeon.DungeonState.Cleared = true;
        Say(Responses.DungeonBossCleared?.Format(dungeon.DungeonMetadata.Name, dungeon.DungeonMetadata.Boss));
        IsDirty = true;
        RestartIdleTimers();
        OnDungeonUpdated(new DungeonTrackedEventArgs(dungeon, confidence, autoTracked));

        if (!autoTracked)
        {
            AddUndo(() =>
            {
                ItemService.ResetProgression();
                dungeon.DungeonState.Cleared = false;
                addedEvent.IsUndone = true;
                foreach (var location in clearedLocations)
                {
                    location.State.Cleared = false;
                }
            });
        }
    }

    /// <summary>
    /// Marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that was defeated.</param>
    /// <param name="admittedGuilt">
    /// <see langword="true"/> if the command implies the boss was killed;
    /// <see langword="false"/> if the boss was simply "tracked".
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    public override void MarkBossAsDefeated(Boss boss, bool admittedGuilt = true, float? confidence = null, bool autoTracked = false)
    {
        if (boss.State.Defeated)
        {
            if (!autoTracked)
                Say(x => x.BossAlreadyDefeated, boss.Name);
            else
                OnBossUpdated(new(boss, confidence, autoTracked));
            return;
        }

        boss.State.Defeated = true;

        if (!admittedGuilt && boss.Metadata.WhenTracked != null)
            Say(boss.Metadata.WhenTracked, boss.Name);
        else
            Say(boss.Metadata.WhenDefeated ?? Responses.BossDefeated, boss.Name);

        var addedEvent = History.AddEvent(
            HistoryEventType.BeatBoss,
            true,
            boss.Name
        );

        IsDirty = true;
        ItemService.ResetProgression();

        RestartIdleTimers();
        OnBossUpdated(new(boss, confidence, autoTracked));

        if (!autoTracked)
        {
            AddUndo(() =>
            {
                boss.State.Defeated = false;
                addedEvent.IsUndone = true;
            });
        }
    }

    /// <summary>
    /// Un-marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that should be 'revived'.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void MarkBossAsNotDefeated(Boss boss, float? confidence = null)
    {
        if (boss.State.Defeated != true)
        {
            Say(x => x.BossNotYetDefeated, boss.Name);
            return;
        }

        boss.State.Defeated = false;
        Say(Responses.BossUndefeated, boss.Name);

        IsDirty = true;
        ItemService.ResetProgression();

        OnBossUpdated(new(boss, confidence, false));
        AddUndo(() => boss.State.Defeated = true);
    }

    /// <summary>
    /// Un-marks a dungeon as cleared and, if possible, untracks the boss
    /// reward.
    /// </summary>
    /// <param name="dungeon">The dungeon that should be un-cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void MarkDungeonAsIncomplete(IDungeon dungeon, float? confidence = null)
    {
        if (!dungeon.DungeonState.Cleared)
        {
            Say(Responses.DungeonBossNotYetCleared?.Format(dungeon.DungeonMetadata.Name, dungeon.DungeonMetadata.Boss));
            return;
        }

        ItemService.ResetProgression();
        dungeon.DungeonState.Cleared = false;
        Say(Responses.DungeonBossUncleared?.Format(dungeon.DungeonMetadata.Name, dungeon.DungeonMetadata.Boss));

        // Try to untrack the associated boss reward item
        Action? undoUnclear = null;
        Action? undoUntrackTreasure = null;
        Action? undoUntrack = null;
        if (dungeon.BossLocationId != null)
        {
            var rewardLocation = _worldService.Location(dungeon.BossLocationId.Value);
            if (rewardLocation.Item.Type != ItemType.Nothing)
            {
                var item = rewardLocation.Item;
                if (item.Type != ItemType.Nothing && item.State.TrackingState > 0)
                {
                    UntrackItem(item);
                    undoUntrack = _undoHistory.Pop().Action;
                }

                if (!rewardLocation.Item.IsDungeonItem)
                {
                    dungeon.DungeonState.RemainingTreasure++;
                    undoUntrackTreasure = () => dungeon.DungeonState.RemainingTreasure--;
                }
            }

            if (rewardLocation.State.Cleared)
            {
                rewardLocation.State.Cleared = false;
                OnLocationCleared(new(rewardLocation, null, false));
                undoUnclear = () => rewardLocation.State.Cleared = true;
            }
        }

        IsDirty = true;

        OnDungeonUpdated(new(dungeon, confidence, false));
        AddUndo(() =>
        {
            dungeon.DungeonState.Cleared = false;
            undoUntrack?.Invoke();
            undoUntrackTreasure?.Invoke();
            undoUnclear?.Invoke();
            ItemService.ResetProgression();
        });
    }

    /// <summary>
    /// Marks an item at the specified location.
    /// </summary>
    /// <param name="location">The location to mark.</param>
    /// <param name="item">
    /// The item that is found at <paramref name="location"/>.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked location was auto tracked</param>
    public override void MarkLocation(Location location, Item item, float? confidence = null, bool autoTracked = false)
    {
        MarkLocation(location, item.Type, confidence, autoTracked, item.Metadata);
    }

    /// <summary>
    /// Marks an item at the specified location.
    /// </summary>
    /// <param name="location">The location to mark.</param>
    /// <param name="item">
    /// The item that is found at <paramref name="location"/>.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked location was auto tracked</param>
    /// <param name="metadata">The metadata of the item</param>
    public override void MarkLocation(Location location, ItemType item, float? confidence = null, bool autoTracked = false, ItemData? metadata = null)
    {
        var locationName = location.Metadata.Name;

        metadata ??= _metadataService.Item(item);

        GiveLocationComment(location.Item.Type, location, isTracking: false, confidence, metadata);

        if (item == ItemType.Nothing)
        {
            Clear(location);
            Say(Responses.LocationMarkedAsBullshit?.Format(locationName));
        }
        else if (location.State.MarkedItem != null)
        {
            var oldType = location.State.MarkedItem;
            location.State.MarkedItem = item;
            Say(x => x.LocationMarkedAgain, locationName, metadata?.Name ?? item.GetDescription(), oldType.GetDescription());
            if (!autoTracked)
            {
                AddUndo(() => location.State.MarkedItem = oldType);
            }
        }
        else
        {
            location.State.MarkedItem = item;
            Say(x => x.LocationMarked, locationName, metadata?.Name ?? item.GetDescription());
            if (!autoTracked)
            {
                AddUndo(() => location.State.MarkedItem = null);
            }
        }

        GivePreConfiguredLocationSass(location);

        IsDirty = true;

        OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
    }

    /// <summary>
    /// Pegs a Peg World peg.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void Peg(float? confidence = null)
    {
        if (!PegWorldMode)
            return;

        PegsPegged++;

        if (PegsPegged < PegWorldModeModule.TotalPegs)
            Say(Responses.PegWorldModePegged);
        else
            Say(Responses.PegWorldModeDone);
        OnPegPegged(new TrackerEventArgs(confidence));
        AddUndo(() => PegsPegged--);

        RestartIdleTimers();
    }

    /// <summary>
    /// Starts Peg World mode.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void StartPegWorldMode(float? confidence = null)
    {
        ShutUp();
        PegWorldMode = true;
        Say(Responses.PegWorldModeOn, wait: true);
        OnToggledPegWorldModeOn(new TrackerEventArgs(confidence));
        AddUndo(() => PegWorldMode = false);
    }

    /// <summary>
    /// Turns Peg World mode off.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void StopPegWorldMode(float? confidence = null)
    {
        PegWorldMode = false;
        Say(Responses.PegWorldModeDone);
        OnToggledPegWorldModeOn(new TrackerEventArgs(confidence));
        AddUndo(() => PegWorldMode = true);
    }

    /// <summary>
    /// Starts Peg World mode.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void StartShaktoolMode(float? confidence = null)
    {
        ShaktoolMode = true;
        OnToggledShaktoolMode(new TrackerEventArgs(confidence));
        AddUndo(() => ShaktoolMode = false);
    }

    /// <summary>
    /// Turns Peg World mode off.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public override void StopShaktoolMode(float? confidence = null)
    {
        ShaktoolMode = false;
        OnToggledShaktoolMode(new TrackerEventArgs(confidence));
        AddUndo(() => ShaktoolMode = true);
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
                region?.Name?.ToString() ?? "new region"
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
        if (hintTile.State == null)
        {
            return;
        }
        else if (hintTile.State.HintState == HintState.Cleared)
        {
            OnHintTileUpdated(new HintTileUpdatedEventArgs(hintTile));
            return;
        }

        LastViewedHintTile = hintTile;

        if (hintTile.Type == HintTileType.Location)
        {
            var locationId = hintTile.Locations!.First();
            var location = World.FindLocation(locationId);
            if (location.State is { Cleared: false, Autotracked: false } && location.State.MarkedItem != location.State.Item)
            {
                MarkLocation(location, location.Item, null, true);
                hintTile.State.HintState = HintState.Viewed;
            }
            else
            {
                hintTile.State.HintState = HintState.Cleared;
            }
        }
        else if (hintTile.Type == HintTileType.Requirement)
        {
            var dungeon = World.Dungeons.First(x => x.DungeonName == hintTile.LocationKey);
            if (dungeon.DungeonState.MarkedMedallion != dungeon.DungeonState.RequiredMedallion)
            {
                SetDungeonRequirement(dungeon, hintTile.MedallionType, null, true);
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
                Say(Configs.HintTileConfig.ViewedHintTileAlreadyVisited, hintTile.LocationKey);
            }
            else if (hintTile.Usefulness == LocationUsefulness.Mandatory)
            {
                hintTile.State.HintState = HintState.Viewed;
                Say(Configs.HintTileConfig.ViewedHintTileMandatory, hintTile.LocationKey);
            }
            else if (hintTile.Usefulness == LocationUsefulness.Useless)
            {
                hintTile.State.HintState = HintState.Viewed;
                Say(Configs.HintTileConfig.ViewedHintTileUseless, hintTile.LocationKey);
            }
            else
            {
                hintTile.State.HintState = HintState.Viewed;
                Say(Configs.HintTileConfig.ViewedHintTile);
            }
        }

        OnHintTileUpdated(new HintTileUpdatedEventArgs(hintTile));
    }

    public override void UpdateLastMarkedLocations(List<Location> locations)
    {
        _lastMarkedLocations = locations;
    }

    public override void ClearLastMarkedLocations(float confidence)
    {
        if (_lastMarkedLocations.Count == 0)
        {
            Say(x => x.NoMarkedLocations);
            return;
        }
        Clear(_lastMarkedLocations, confidence);
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
    /// Determines whether or not the specified reward is worth getting.
    /// </summary>
    /// <param name="reward">The dungeon reward.</param>
    /// <returns>
    /// <see langword="true"/> if the reward leads to something good;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsWorth(RewardType reward)
    {
        var sahasrahlaItem = World.FindLocation(LocationId.Sahasrahla).Item;
        if (sahasrahlaItem.Type != ItemType.Nothing && reward == RewardType.PendantGreen)
        {
            _logger.LogDebug("{Reward} leads to {Item}...", reward, sahasrahlaItem);
            if (IsWorth(sahasrahlaItem))
            {
                _logger.LogDebug("{Reward} leads to {Item}, which is worth it", reward, sahasrahlaItem);
                return true;
            }
            _logger.LogDebug("{Reward} leads to {Item}, which is junk", reward, sahasrahlaItem);
        }

        var pedItem = World.LightWorldNorthWest.MasterSwordPedestal.Item;
        if (pedItem.Type != ItemType.Nothing && (reward is RewardType.PendantGreen or RewardType.PendantRed or RewardType.PendantBlue))
        {
            _logger.LogDebug("{Reward} leads to {Item}...", reward, pedItem);
            if (IsWorth(pedItem))
            {
                _logger.LogDebug("{Reward} leads to {Item}, which is worth it", reward, pedItem);
                return true;
            }
            _logger.LogDebug("{Reward} leads to {Item}, which is junk", reward, pedItem);
        }

        return false;
    }

    /// <summary>
    /// Determines whether or not the specified item is worth getting.
    /// </summary>
    /// <param name="item">The item whose worth to consider.</param>
    /// <returns>
    /// <see langword="true"/> is the item is worth getting or leads to
    /// another item that is worth getting; otherwise, <see
    /// langword="false"/>.
    /// </returns>
    public override bool IsWorth(Item item)
    {
        var leads = new Dictionary<ItemType, Location[]>()
        {
            [ItemType.Mushroom] = new[] { World.LightWorldNorthEast.MushroomItem },
            [ItemType.Powder] = new[] { World.LightWorldNorthWest.MagicBat },
            [ItemType.Book] = new[]
            {
                World.LightWorldDeathMountainWest.EtherTablet,
                World.LightWorldSouth.BombosTablet
            },
            [ItemType.Bottle] = new[] { World.LightWorldNorthWest.SickKid },
            [ItemType.BottleWithBee] = new[] { World.LightWorldNorthWest.SickKid },
            [ItemType.BottleWithFairy] = new[] { World.LightWorldNorthWest.SickKid },
            [ItemType.BottleWithBluePotion] = new[] { World.LightWorldNorthWest.SickKid },
            [ItemType.BottleWithGoldBee] = new[] { World.LightWorldNorthWest.SickKid },
            [ItemType.BottleWithGreenPotion] = new[] { World.LightWorldNorthWest.SickKid },
            [ItemType.BottleWithRedPotion] = new[] { World.LightWorldNorthWest.SickKid },
        };

        if (leads.TryGetValue(item.Type, out var leadsToLocation) && !ItemService.IsTracked(item.Type))
        {
            foreach (var location in leadsToLocation)
            {
                var reward = location.Item;
                if (reward.Type != ItemType.Nothing)
                {
                    _logger.LogDebug("{Item} leads to {OtherItem}...", item, reward);
                    if (IsWorth(reward))
                    {
                        _logger.LogDebug("{Item} leads to {OtherItem}, which is worth it", item, reward);
                        return true;
                    }
                    _logger.LogDebug("{Item} leads to {OtherItem}, which is junk", item, reward);
                }
            }
        }

        return item.Metadata.IsGood(World.Config);
    }

    /// <summary>
    /// Adds an action to be invoked to undo the last operation.
    /// </summary>
    /// <param name="undo">
    /// The action to invoke to undo the last operation.
    /// </param>
    public override void AddUndo(Action undo) => _undoHistory.Push((undo, DateTime.Now));

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

    private static bool IsTreasure(Item? item)
        => item is { IsDungeonItem: false };

    private IDungeon? GetDungeonFromLocation(Location location)
    {
        if (location.Type == LocationType.NotInDungeon)
            return null;

        return location.Region as IDungeon;
    }

    private IDungeon? GetDungeonFromArea(IHasLocations area)
    {
        return area switch
        {
            Room room => room.Region as IDungeon,
            Region region => region as IDungeon,
            _ => null
        };
    }

    private Action? TryTrackDungeonTreasure(Location location, float? confidence, bool autoTracked = false, bool stateResponse = true)
    {
        if (confidence < Options.MinimumSassConfidence)
        {
            // Tracker response could give away the location of an item if
            // it is in a dungeon but tracker misheard.
            return null;
        }

        var dungeon = GetDungeonFromLocation(location);
        if (dungeon != null && (IsTreasure(location.Item) || World.Config.ZeldaKeysanity))
        {
            if (TrackDungeonTreasure(dungeon, confidence, 1, autoTracked, stateResponse))
                return _undoHistory.Pop().Action;
        }

        IsDirty = true;

        return null;
    }

    private void GiveLocationComment(ItemType item, Location location, bool isTracking, float? confidence, ItemData? metadata)
    {
        metadata ??= _metadataService.Item(item);

        // If the plando config specifies a specific line for this location, say it
        if (World.Config.PlandoConfig?.TrackerLocationLines.ContainsKey(location.ToString()) == true)
        {
            Say(World.Config.PlandoConfig?.TrackerLocationLines[location.ToString()]);
        }
        // Give some sass if the user tracks or marks the wrong item at a
        // location unless the user is clearing a useless item like missiles
        else if (location.Item.Type != ItemType.Nothing && !item.IsEquivalentTo(location.Item.Type) && (item != ItemType.Nothing || location.Item.Metadata.IsProgression(World.Config)))
        {
            if (confidence == null || confidence < Options.MinimumSassConfidence)
                return;

            var actualItemName = ItemService.GetName(location.Item.Type);
            if (HintsEnabled) actualItemName = "another item";

            Say(Responses.LocationHasDifferentItem?.Format(metadata?.NameWithArticle ?? item.GetDescription(), actualItemName));
        }
        else
        {
            if (item == location.VanillaItem && item != ItemType.Nothing)
            {
                Say(x => x.TrackedVanillaItem);
                return;
            }

            var locationInfo = location.Metadata;
            var isJunk = metadata?.IsJunk(World.Config) ?? true;
            if (isJunk)
            {
                if (!isTracking && locationInfo.WhenMarkingJunk?.Count > 0)
                {
                    Say(locationInfo.WhenMarkingJunk.Random(s_random)!);
                }
                else if (locationInfo.WhenTrackingJunk?.Count > 0)
                {
                    Say(locationInfo.WhenTrackingJunk.Random(s_random)!);
                }
            }
            else if (!isJunk)
            {
                if (!isTracking && locationInfo.WhenMarkingProgression?.Count > 0)
                {
                    Say(locationInfo.WhenMarkingProgression.Random(s_random)!);
                }
                else if (locationInfo.WhenTrackingProgression?.Count > 0)
                {
                    Say(locationInfo.WhenTrackingProgression.Random(s_random)!);
                }
            }
        }
    }

    private IDungeon? GetDungeonFromItem(Item item, IDungeon? dungeon = null)
    {
        var locations = _worldService.Locations(itemFilter: item.Type)
            .Where(x => x.Type != LocationType.NotInDungeon)
            .ToImmutableList();

        if (locations.Count == 1 && dungeon == null)
        {
            // User didn't have a guess and there's only one location that
            // has the tracker item
            return GetDungeonFromLocation(locations[0]);
        }

        if (locations.Count > 0 && dungeon != null)
        {
            // Does the dungeon even have that item?
            if (locations.All(x => dungeon != x.Region))
            {
                // Be a smart-ass about it
                Say(Responses.ItemTrackedInIncorrectDungeon?.Format(dungeon.DungeonMetadata.Name, item.Metadata.NameWithArticle));
            }
        }

        // - If tracker was started before generating a seed, we don't know
        // better.
        // - If we do know better, we should still go with the user's
        // choice.
        // - If there are multiple copies of the item, we don't know which
        // was tracked. Either way, we have to assume `dungeon` is correct.
        // If it's `null`, nobody knows.
        return dungeon;
    }

    private void IdleTimerElapsed(object? state)
    {
        var key = (string)state!;
        Say(Responses.Idle?[key]);
    }

    private void Recognizer_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            RestartIdleTimers();
            OnSpeechRecognized(new(e.Result.Confidence, e.Result.Text));
        }
    }

    private void GiveLocationHint(IEnumerable<Location> accessibleBefore)
    {
        var accessibleAfter = _worldService.AccessibleLocations(false);
        var newlyAccessible = accessibleAfter.Except(accessibleBefore).ToList();
        if (newlyAccessible.Any())
        {
            if (newlyAccessible.Contains(World.FindLocation(LocationId.InnerMaridiaSpringBall)))
                Say(Responses.ShaktoolAvailable);

            if (newlyAccessible.Contains(World.DarkWorldNorthWest.PegWorld))
                Say(Responses.PegWorldAvailable);
        }
        else if (Responses.TrackedUselessItem != null)
        {
            Say(Responses.TrackedUselessItem);
        }
    }

}
