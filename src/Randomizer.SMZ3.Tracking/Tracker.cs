using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BunLabs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Tracking.AutoTracking;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Tracks items and locations in a playthrough by listening for voice
    /// commands and responding with text-to-speech.
    /// </summary>
    public class Tracker : IDisposable
    {
        private const int RepeatRateModifier = 2;
        private static readonly Random s_random = new();

        private readonly SpeechRecognitionEngine _recognizer;
        private readonly IWorldAccessor _worldAccessor;
        private readonly TrackerModuleFactory _moduleFactory;
        private readonly IChatClient _chatClient;
        private readonly ILogger<Tracker> _logger;
        private readonly TrackerOptionsAccessor _trackerOptions;
        private readonly Dictionary<string, Timer> _idleTimers;
        private readonly Stack<Action> _undoHistory = new();
        private readonly RandomizerContext _dbContext;
        private readonly ICommunicator _communicator;
        private DateTime _startTime = DateTime.MinValue;
        private DateTime _undoStartTime = DateTime.MinValue;
        private TimeSpan _undoSavedTime;
        private bool _disposed;
        private string? _mood;
        private string? _lastSpokenText;
        private Dictionary<string, Progression> _progression = new();
        private bool _alternateTracker;
        private HashSet<SchrodingersString> _saidLines = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="configProvider">Used to access external configuration.</param>
        /// <param name="worldAccessor">
        /// Used to get the world to track in.
        /// </param>
        /// <param name="moduleFactory">
        /// Used to provide the tracking speech recognition syntax.
        /// </param>
        /// <param name="chatClient"></param>
        /// <param name="logger">Used to write logging information.</param>
        /// <param name="trackerOptions">Provides Tracker preferences.</param>
        /// <param name="dbContext">The database context</param>
        /// <param name="historyService">Service for</param>
        public Tracker(TrackerConfigProvider configProvider,
            IWorldAccessor worldAccessor,
            TrackerModuleFactory moduleFactory,
            IChatClient chatClient,
            ILogger<Tracker> logger,
            TrackerOptionsAccessor trackerOptions,
            RandomizerContext dbContext,
            IItemService itemService,
            ICommunicator communicator,
            IHistoryService historyService,
            TrackerConfigs configs,
            IWorldService worldService)
        {
            if (trackerOptions.Options == null)
                throw new InvalidOperationException("Tracker options have not yet been activated.");

            _worldAccessor = worldAccessor;
            _moduleFactory = moduleFactory;
            _chatClient = chatClient;
            _logger = logger;
            _trackerOptions = trackerOptions;
            _dbContext = dbContext;
            ItemService = itemService;
            _communicator = communicator;

            // Initialize the tracker configuration
            Responses = configs.Responses;
            Requests = configs.Requests;
            WorldInfo = worldService;
            GetTreasureCounts(WorldInfo.Dungeons, World);
            UpdateTrackerProgression = true;

            History = historyService;

            // Initalize the timers used to trigger idle responses
            _idleTimers = Responses.Idle.ToDictionary(
                x => x.Key,
                x => new Timer(IdleTimerElapsed, x.Key, Timeout.Infinite, Timeout.Infinite));

            // Initialize the text-to-speech
            if (s_random.NextDouble() <= 0.01)
            {
                _alternateTracker = true;
                _communicator.UseAlternateVoice();
            }

            // Initialize the speech recognition engine
            _recognizer = new SpeechRecognitionEngine();
            _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            InitializeMicrophone();
        }

        /// <summary>
        /// Occurs when any speech was recognized, regardless of configured
        /// thresholds.
        /// </summary>
        public event EventHandler<TrackerEventArgs>? SpeechRecognized;

        /// <summary>
        /// Occurs when one more more items have been tracked.
        /// </summary>
        public event EventHandler<ItemTrackedEventArgs>? ItemTracked;

        /// <summary>
        /// Occurs when a location has been cleared.
        /// </summary>
        public event EventHandler<LocationClearedEventArgs>? LocationCleared;

        /// <summary>
        /// Occurs when Peg World mode has been toggled on.
        /// </summary>
        public event EventHandler<TrackerEventArgs>? ToggledPegWorldModeOn;

        /// <summary>
        /// Occurs when a Peg World peg has been pegged.
        /// </summary>
        public event EventHandler<TrackerEventArgs>? PegPegged;

        /// <summary>
        /// Occurs when the properties of a dungeon have changed.
        /// </summary>
        public event EventHandler<TrackerEventArgs>? DungeonUpdated;

        /// <summary>
        /// Occurs when the properties of a boss have changed.
        /// </summary>
        public event EventHandler<TrackerEventArgs>? BossUpdated;

        /// <summary>
        /// Occurs when the <see cref="MarkedLocations"/> collection has
        /// changed.
        /// </summary>
        public event EventHandler<TrackerEventArgs>? MarkedLocationsUpdated;

        /// <summary>
        /// Occurs when Go mode has been turned on.
        /// </summary>
        public event EventHandler<TrackerEventArgs>? GoModeToggledOn;

        /// <summary>
        /// Occurs when the last action was undone.
        /// </summary>
        public event EventHandler<TrackerEventArgs>? ActionUndone;

        /// <summary>
        /// Occurs when the tracker state has been loaded.
        /// </summary>
        public event EventHandler? StateLoaded;

        /// <summary>
        /// Occurs when the map has been updated
        /// </summary>
        public event EventHandler? MapUpdated;

        /// <summary>
        /// Set when the progression needs to be updated for the current tracker
        /// instance
        /// </summary>
        public bool UpdateTrackerProgression { get; set; }

        /// <summary>
        /// Gets extra information about locations.
        /// </summary>
        public IWorldService WorldInfo { get; }

        /// <summary>
        /// Gets a reference to the <see cref="ItemService"/>.
        /// </summary>
        public IItemService ItemService { get; }

        /// <summary>
        /// The number of pegs that have been pegged for Peg World mode
        /// </summary>
        public int PegsPegged { get; set; }

        /// <summary>
        /// Gets the world for the currently tracked playthrough.
        /// </summary>
        public World World => _worldAccessor.World;

        /// <summary>
        /// Indicates whether Tracker is in Go Mode.
        /// </summary>
        public bool GoMode { get; private set; }

        /// <summary>
        /// Indicates whether Tracker is in Peg World mode.
        /// </summary>
        public bool PegWorldMode { get; set; }

        /// <summary>
        /// If the speech recognition engine was fully initialized
        /// </summary>
        public bool MicrophoneInitialized { get; private set; }

        /// <summary>
        /// If voice recognition has been enabled or not
        /// </summary>
        public bool VoiceRecognitionEnabled { get; private set; }

        /// <summary>
        /// Gets a dictionary that contains the locations that are marked with
        /// items.
        /// </summary>
        public Dictionary<int, ItemData> MarkedLocations { get; } = new();

        /// <summary>
        /// Gets the configured responses.
        /// </summary>
        public ResponseConfig Responses { get; }

        /// <summary>
        /// Gets a collection of basic requests and responses.
        /// </summary>
        public IReadOnlyCollection<BasicVoiceRequest> Requests { get; }

        /// <summary>
        /// Gets a dictionary containing the rules and the various speech
        /// recognition syntaxes.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<string>> Syntax { get; private set; }
            = new Dictionary<string, IEnumerable<string>>();

        /// <summary>
        /// Gets the tracking preferences.
        /// </summary>
        public TrackerOptions Options => _trackerOptions.Options!;

        /// <summary>
        /// The generated rom
        /// </summary>
        public GeneratedRom? Rom { get; private set; }

        /// <summary>
        /// The region the player is currently in
        /// </summary>
        public RegionInfo? CurrentRegion { get; private set; }

        /// <summary>
        /// The map to display for the player
        /// </summary>
        public string CurrentMap { get; private set; } = "";

        /// <summary>
        /// Gets a string describing tracker's mood.
        /// </summary>
        public string Mood
        {
            get
            {
                if (_mood == null)
                {
                    _mood = Responses.Moods.Keys.Random(Rng.Current);
                }
                return _mood;
            }
        }

        /// <summary>
        /// The previous saved elapsed time
        /// </summary>
        public TimeSpan UndoSavedElapsedTime { get; set; }

        /// <summary>
        /// The previous saved elapsed time
        /// </summary>
        public TimeSpan SavedElapsedTime { get; set; }

        /// <summary>
        /// The total elapsed time including the previously saved time
        /// </summary>
        public TimeSpan TotalElapsedTime => SavedElapsedTime + (DateTime.Now - (_startTime == DateTime.MinValue ? DateTime.Now : _startTime));

        /// <summary>
        /// Get if the Tracker has been updated since it was last saved
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// The Auto Tracker for the Tracker
        /// </summary>
        public AutoTracker? AutoTracker { get; set; }

        /// <summary>
        /// Service that handles modifying the game via auto tracker
        /// </summary>
        public GameService GameService { get; set; }

        /// <summary>
        /// Module that houses the history
        /// </summary>
        public IHistoryService History { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Tracker may give hints when
        /// asked about items or locations.
        /// </summary>
        public bool HintsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Tracker may give spoilers
        /// when asked about items or locations.
        /// </summary>
        public bool SpoilersEnabled { get; set; }

        /// <summary>
        /// Formats a string so that it will be pronounced correctly by the
        /// text-to-speech engine.
        /// </summary>
        /// <param name="name">The text to correct.</param>
        /// <returns>A string with the pronunciations replaced.</returns>
        public static string CorrectPronunciation(string name)
            => name.Replace("Samus", "Sammus");

        /// <summary>
        /// Attempts to replace a user name with a pronunciation-corrected
        /// version of it.
        /// </summary>
        /// <param name="userName">The user name to correct.</param>
        /// <returns>
        /// The corrected user name, or <paramref name="userName"/>.
        /// </returns>
        public string CorrectUserNamePronunciation(string userName)
        {
            var correctedUserName = Responses.Chat.UserNamePronunciation
                .SingleOrDefault(x => x.Key.Equals(userName, StringComparison.OrdinalIgnoreCase));

            return correctedUserName.Value ?? userName.Replace('_', ' ');
        }

        /// <summary>
        /// Initializes the microphone from the default audio device
        /// </summary>
        /// <returns>
        /// True if the microphone is initialized, false otherwise
        /// </returns>
        public bool InitializeMicrophone()
        {
            if (MicrophoneInitialized) return true;

            try
            {
                _recognizer.SetInputToDefaultAudioDevice();
                MicrophoneInitialized = true;
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error initializing microphone");
                return false;
            }
        }

        /// <summary>
        /// Loads the tracker state from the specified saved state.
        /// </summary>
        /// <param name="stream">A stream containing the saved state.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="stream"/> is not a valid Tracker saved state.
        /// </exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadAsync(Stream stream)
        {
            IsDirty = false;
            var state = await TrackerState.LoadAsync(stream);
            state.Apply(this, _worldAccessor, ItemService);
            History.LoadHistory(this, state);
            OnStateLoaded();
        }

        /// <summary>
        /// Loads the state from the database for a given rom
        /// </summary>
        /// <param name="rom">The rom to load</param>
        /// <returns>True or false if the load was successful</returns>
        public bool Load(GeneratedRom rom)
        {
            IsDirty = false;
            Rom = rom;
            var state = TrackerState.Load(_dbContext, rom);
            if (state != null)
            {
                state.Apply(this, _worldAccessor, ItemService);
                History.LoadHistory(this, state);
                OnStateLoaded();
                
                return true;
            }
            History.StartHistory(this);
            return false;
        }

        /// <summary>
        /// Saves the tracker state.
        /// </summary>
        /// <param name="destination">The stream to save the state to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SaveAsync(Stream destination)
        {
            IsDirty = false;
            var state = TrackerState.TakeSnapshot(this, ItemService);
            return state.SaveAsync(destination);
        }

        /// <summary>
        /// Saves the state of the tracker to the database
        /// </summary>
        /// <param name="rom">The rom to save</param>
        /// <returns></returns>
        public Task SaveAsync(GeneratedRom rom)
        {
            IsDirty = false;
            var state = TrackerState.TakeSnapshot(this, ItemService);
            return state.SaveAsync(_dbContext, rom);
        }

        /// <summary>
        /// Undoes the last operation.
        /// </summary>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void Undo(float confidence)
        {
            if (_undoHistory.TryPop(out var undoLast))
            {
                Say(Responses.ActionUndone);
                undoLast();
                OnActionUndone(new TrackerEventArgs(confidence));
            }
            else
            {
                Say(Responses.NothingToUndo);
            }
        }

        /// <summary>
        /// Returns a collection of the points of interest in the specified
        /// region.
        /// </summary>
        /// <param name="region">
        /// The region whose points of interest to enumerate.
        /// </param>
        /// <returns>
        /// A collection of the points of interest in <paramref name="region"/>.
        /// </returns>
        public IEnumerable<IPointOfInterest> EnumeratePointsOfInterest(Region region)
        {
            foreach (var room in region.Rooms)
            {
                yield return WorldInfo.Room(room);
            }

            foreach (var location in region.GetStandaloneLocations())
            {
                yield return WorldInfo.Location(location);
            }

            foreach (var dungeon in WorldInfo.Dungeons)
            {
                if (dungeon.IsInRegion(region))
                    yield return dungeon;
            }
        }

        /// <summary>
        /// Returns info about locations associated with the specified point of
        /// interest.
        /// </summary>
        /// <param name="poi">
        /// The point of interest whose locations to get information about.
        /// </param>
        /// <returns>
        /// A collection of <see cref="LocationInfo"/> associated with <paramref
        /// name="poi"/>.
        /// </returns>
        public IReadOnlyCollection<LocationInfo> GetLocations(IPointOfInterest poi)
        {
            return poi.GetLocations(World)
                .Select(x => WorldInfo.Location(x))
                .ToImmutableList();
        }

        /// <summary>
        /// Toggles Go Mode on.
        /// </summary>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void ToggleGoMode(float? confidence = null)
        {
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
        public bool TrackDungeonTreasure(DungeonInfo dungeon, float? confidence = null, int amount = 1, bool autoTracked = false, bool stateResponse = true)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount of items must be greater than zero.");
            if (amount > dungeon.TreasureRemaining && !dungeon.HasManuallyClearedTreasure)
            {
                _logger.LogWarning("Trying to track {amount} treasures in a dungeon with only {left} treasures left.", amount, dungeon.TreasureRemaining);
                Say(Responses.DungeonTooManyTreasuresTracked?.Format(dungeon.Name, dungeon.TreasureRemaining, amount));
                return false;
            }

            if (dungeon.TreasureRemaining > 0)
            {
                dungeon.TreasureRemaining -= amount;

                // Always add a response if there's treasure left, even when
                // clearing a dungeon (because that means it was out of logic
                // and could be relevant)
                if (stateResponse && (confidence != null || dungeon.TreasureRemaining >= 1 || autoTracked))
                {
                    // Try to get the response based on the amount of items left
                    if (Responses.DungeonTreasureTracked.TryGetValue(dungeon.TreasureRemaining, out var response))
                        Say(response.Format(dungeon.Name, dungeon.TreasureRemaining));
                    // If we don't have a response for the exact amount and we
                    // have multiple left, get the one for 2 (considered
                    // generic)
                    else if (dungeon.TreasureRemaining >= 2 && Responses.DungeonTreasureTracked.TryGetValue(2, out response))
                        Say(response.Format(dungeon.Name, dungeon.TreasureRemaining));
                }

                OnDungeonUpdated(new TrackerEventArgs(confidence));
                AddUndo(() => dungeon.TreasureRemaining += amount);
                return true;
            }
            else if (stateResponse && confidence != null && Responses.DungeonTreasureTracked.TryGetValue(-1, out var response))
            {
                // Attempted to track treasure when all treasure items were
                // already cleared out
                Say(response.Format(dungeon.Name));
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
        public void SetDungeonReward(DungeonInfo dungeon, RewardItem? reward = null, float? confidence = null)
        {
            var originalReward = dungeon.Reward;
            if (reward == null)
            {
                dungeon.Reward = Enum.IsDefined(dungeon.Reward + 1) ? dungeon.Reward + 1 : RewardItem.Unknown;
                // Cycling through rewards is done via UI, so speaking the
                // reward out loud for multiple clicks is kind of annoying
            }
            else
            {
                dungeon.Reward = reward.Value;
                
                Say(Responses.DungeonRewardMarked.Format(dungeon.Name, ItemService.GetName(dungeon.Reward)));
            }

            OnDungeonUpdated(new TrackerEventArgs(confidence));
            AddUndo(() => dungeon.Reward = originalReward);
        }

        /// <summary>
        /// Sets the reward of all unmarked dungeons.
        /// </summary>
        /// <param name="reward">The reward to set.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void SetUnmarkedDungeonReward(RewardItem reward, float? confidence = null)
        {
            var unmarkedDungeons = WorldInfo.Dungeons
                .Where(x => x.HasReward && x.Reward == RewardItem.Unknown)
                .ToImmutableList();

            if (unmarkedDungeons.Count > 0)
            {
                unmarkedDungeons.ForEach(dungeon => dungeon.Reward = reward);
                Say(Responses.RemainingDungeonsMarked.Format(ItemService.GetName(reward)));

                AddUndo(() => unmarkedDungeons.ForEach(dungeon => dungeon.Reward = RewardItem.Unknown));
                OnDungeonUpdated(new(confidence));
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
        public void SetDungeonRequirement(DungeonInfo dungeon, Medallion? medallion = null, float? confidence = null)
        {
            var region = World?.Regions.SingleOrDefault(x => dungeon.Name.Contains(x.Name, StringComparison.OrdinalIgnoreCase));
            if (region == null)
            {
                Say("Strange, I can't find that dungeon in this seed.");
            }
            else if (region is not INeedsMedallion medallionRegion)
            {
                Say(Responses.DungeonRequirementInvalid.Format(dungeon.Name));
                return;
            }

            var originalRequirement = dungeon.Requirement;
            if (medallion == null)
            {
                dungeon.Requirement = Enum.IsDefined(dungeon.Requirement + 1) ? dungeon.Requirement + 1 : Medallion.None;
                OnDungeonUpdated(new TrackerEventArgs(confidence));
            }
            else
            {
                if (region is INeedsMedallion medallionRegion
                    && medallionRegion.Medallion != ItemType.Nothing
                    && medallionRegion.Medallion != medallion.Value.ToItemType()
                    && confidence >= Options.MinimumSassConfidence)
                {
                    Say(Responses.DungeonRequirementMismatch?.Format(
                        HintsEnabled ? "a different medallion" : medallionRegion.Medallion.ToString(),
                        dungeon.Name,
                        medallion.Value.ToString()));
                }

                dungeon.Requirement = medallion.Value;
                Say(Responses.DungeonRequirementMarked.Format(medallion.ToString(), dungeon.Name));
                OnDungeonUpdated(new TrackerEventArgs(confidence));
            }

            AddUndo(() => dungeon.Requirement = originalRequirement);
        }

        /// <summary>
        /// Gets the currently available items for a particular region or room
        /// </summary>
        /// <param name="area">The area to get progression for.</param>    
        /// <returns>
        /// A new <see cref="Progression"/> object representing the currently
        /// available items.
        /// </returns>
        /// <remarks>
        /// Keycards and dungeon items such as keys are assumed to be owned,
        /// unless playing on a keysanity world for that particular game, in
        /// which case keys and keycards must be tracked manually
        /// </remarks>
        public Progression GetProgression(IHasLocations area)
        {
            if (area is Z3Region || (area is Room room1 && room1.Region is Z3Region))
                return GetProgression(assumeKeys: !World.Config.ZeldaKeysanity);
            else if (area is SMRegion || (area is Room room2 && room2.Region is SMRegion))
                return GetProgression(assumeKeys: !World.Config.MetroidKeysanity);
            else
                return GetProgression(assumeKeys: World.Config.KeysanityMode == KeysanityMode.None);
        }

        /// <summary>
        /// Gets the currently available items.
        /// </summary>
        /// <param name="assumeKeys">
        /// Indicates whether to add small and big dungeon keys to the
        /// progression pool. If keysanity is enabled for the current <see
        /// cref="World"/>, this setting also adds keycards, which are otherwise
        /// always added.
        /// </param>
        /// <returns>
        /// A new <see cref="Progression"/> object representing the currently
        /// available items.
        /// </returns>
        public Progression GetProgression(bool assumeKeys)
        {
            if (UpdateTrackerProgression)
            {
                _progression.Clear();
                UpdateTrackerProgression = false;
            }

            var mapKey = $"{assumeKeys}{!World.Config.Keysanity}";

            if (_progression.ContainsKey(mapKey))
            {
                return _progression[mapKey];
            }

            var progression = new Progression();

            if (!World.Config.MetroidKeysanity || assumeKeys)
            {
                progression.AddRange(Item.CreateKeycards(World));
                if (assumeKeys)
                    progression.AddRange(Item.CreateDungeonPool(World));
            }

            foreach (var item in ItemService.TrackedItems())
            {
                progression.AddRange(Enumerable.Repeat(item.InternalItemType, item.TrackingState));
            }

            progression.AddRange(GetCurrentRewards());

            _progression[mapKey] = progression;
            return progression;
        }

        /// <summary>
        /// Starts voice recognition.
        /// </summary>
        public virtual bool TryStartTracking()
        {
            // Load the modules for voice recognition
            StartTimer(true);
            Syntax = _moduleFactory.LoadAll(this, _recognizer, out var loadError);

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
        public void ConnectToChat(string? userName, string? oauthToken, string? channel, string? id)
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
        public virtual void StartTimer(bool isInitial = false)
        {
            _undoStartTime = _startTime;
            _startTime = DateTime.Now;

            if (!isInitial)
            {
                Say(Responses.TimerResumed);
                AddUndo(() => _startTime = _undoStartTime);
            }
        }

        /// <summary>
        /// Resets the timer to 0
        /// </summary>
        public virtual void ResetTimer(bool isInitial = false)
        {
            _undoSavedTime = SavedElapsedTime;
            _undoStartTime = _startTime;

            SavedElapsedTime = TimeSpan.Zero;
            _startTime = DateTime.Now;

            if (!isInitial)
            {
                Say(Responses.TimerReset);
                AddUndo(() =>
                {
                    SavedElapsedTime = _undoSavedTime;
                    _startTime = _undoStartTime;
                });
            }
        }

        /// <summary>
        /// Pauses the timer, saving the elapsed time
        /// </summary>
        public virtual void PauseTimer()
        {
            _undoSavedTime = SavedElapsedTime;
            _undoStartTime = _startTime;

            SavedElapsedTime = TotalElapsedTime;
            _startTime = DateTime.MinValue;

            Say(Responses.TimerPaused);

            AddUndo(() =>
            {
                SavedElapsedTime = _undoSavedTime;
                _startTime = _undoStartTime;
            });
        }

        /// <summary>
        /// If the timer is currently paused
        /// </summary>
        public virtual bool IsTimerPaused => _startTime == DateTime.MinValue;

        /// <summary>
        /// Stops voice recognition.
        /// </summary>
        public virtual void StopTracking()
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
        public void EnableVoiceRecognition()
        {
            if (MicrophoneInitialized && !VoiceRecognitionEnabled)
            {
                _logger.LogInformation("Starting speech recognition");
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                VoiceRecognitionEnabled = true;
            }
        }

        /// <summary>
        /// Disables voice recognition if it was previously enabled
        /// </summary>
        public void DisableVoiceRecognition()
        {
            if (VoiceRecognitionEnabled)
            {
                VoiceRecognitionEnabled = false;
                _recognizer.RecognizeAsyncStop();
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
        public virtual bool Say(SchrodingersString? text)
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
        public virtual bool Say(Func<ResponseConfig, SchrodingersString?> selectResponse)
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
        public virtual bool Say(SchrodingersString? text, params object?[] args)
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
        public virtual bool Say(Func<ResponseConfig, SchrodingersString?> selectResponse, params object?[] args)
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
        public virtual bool SayOnce(SchrodingersString? text)
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
        public virtual bool SayOnce(Func<ResponseConfig, SchrodingersString?> selectResponse)
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
        public virtual bool SayOnce(SchrodingersString? text, params object?[] args)
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
        public virtual bool SayOnce(Func<ResponseConfig, SchrodingersString?> selectResponse, params object?[] args)
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
        public virtual bool Say(string? text, bool wait = false)
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
        public virtual string? FormatPlaceholders(string? text)
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
        public virtual void Repeat()
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
        public virtual void ShutUp()
        {
            _communicator.Abort();
        }

        /// <summary>
        /// Notifies the user an error occurred.
        /// </summary>
        public virtual void Error()
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
        /// <returns>
        /// <see langword="true"/> if the item was actually tracked; <see
        /// langword="false"/> if the item could not be tracked, e.g. when
        /// tracking Bow twice.
        /// </returns>
        public bool TrackItem(ItemData item, string? trackedAs = null, float? confidence = null, bool tryClear = true, bool autoTracked = false, Location? location = null, bool giftedItem = false)
        {
            var didTrack = false;
            var accessibleBefore = GetAccessibleLocations();
            var itemName = item.Name;
            var originalTrackingState = item.TrackingState;
            UpdateTrackerProgression = true;

            var isGTPreBigKey = !World.Config.ZeldaKeysanity
                                && autoTracked
                                && location?.Region.GetType() == typeof(GanonsTower)
                                && !GetProgression(false).BigKeyGT;
            var stateResponse = !isGTPreBigKey && (!autoTracked
                                               || !item.IsDungeonItem()
                                               || World.Config.ZeldaKeysanity);

            if (item.HasStages)
            {
                if (trackedAs != null && item.GetStage(trackedAs) != null)
                {
                    var stage = item.GetStage(trackedAs)!;

                    // Tracked by specific stage name (e.g. Tempered Sword), set
                    // to that stage specifically
                    var stageName = item.Stages[stage.Value].ToString();

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
                                Say(Responses.TrackedItemByStage.Format(itemName, stageName));
                            }
                        }
                        else
                        {
                            Say(Responses.TrackedOlderProgressiveItem?.Format(itemName, item.Stages[item.TrackingState].ToString()));
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
                                var stageName = item.Stages[item.TrackingState].ToString();
                                Say(Responses.TrackedProgressiveItem.Format(itemName, stageName));
                            }
                        }
                        else
                        {
                            Say(Responses.TrackedTooManyOfAnItem?.Format(itemName));
                        }
                    }
                }
            }
            else if (item.Multiple)
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
                        Say(Responses.TrackedItem.Format(itemName, item.NameWithArticle));
                }
                else if (item.Counter > 1)
                {
                    if (stateResponse)
                        Say(Responses.TrackedItemMultiple.Format(item.Plural ?? $"{itemName}s", item.Counter, item.Name));
                }
                else
                {
                    _logger.LogWarning("Encountered multiple item with counter 0: {item} has counter {counter}", item, item.Counter);
                    if (stateResponse)
                        Say(Responses.TrackedItem.Format(itemName, item.NameWithArticle));
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
                            Say(Responses.TrackedItem.Format(itemName, item.NameWithArticle));
                        }
                    }
                    else
                    {
                        Say(Responses.TrackedAlreadyTrackedItem?.Format(itemName));
                    }
                }
            }

            Action undoTrack = () => { item.TrackingState = originalTrackingState; UpdateTrackerProgression = true; };
            OnItemTracked(new ItemTrackedEventArgs(trackedAs, confidence));

            // Check if we can clear a location
            Action? undoClear = null;
            Action? undoTrackDungeonTreasure = null;

            // If this was not gifted to the player, try to clear the location
            if (!giftedItem)
            {
                if (location == null)
                {
                    location = World.Locations.TrySingle(x => x.Cleared == false && x.Item.Type == item.InternalItemType);
                }

                if (location != null)
                {
                    if (stateResponse)
                    {
                        GiveLocationComment(item, location, isTracking: true, confidence);
                    }

                    if (tryClear)
                    {
                        // If this item was in a dungeon, track treasure count
                        undoTrackDungeonTreasure = TryTrackDungeonTreasure(location, confidence, autoTracked, stateResponse);

                        // Important: clear only after tracking dungeon treasure, as
                        // the "guess dungeon from location" algorithm excludes
                        // cleared items
                        location.Cleared = true;
                        OnLocationCleared(new(location, confidence));

                        undoClear = () => location.Cleared = false;
                        if (MarkedLocations.ContainsKey(location.Id))
                        {
                            MarkedLocations.Remove(location.Id);
                            OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
                        }
                    }

                    var isKeysanityForLocation = (location.Region is Z3Region && World.Config.ZeldaKeysanity) || (location.Region is SMRegion && World.Config.MetroidKeysanity);
                    var items = GetProgression(!isKeysanityForLocation);
                    if (stateResponse && !location.IsAvailable(items) && (confidence >= Options.MinimumSassConfidence || autoTracked))
                    {
                        var locationInfo = WorldInfo.Location(location);
                        var roomInfo = location.Room != null ? WorldInfo.Room(location.Room) : null;
                        var regionInfo = WorldInfo.Region(location.Region);

                        if (locationInfo.OutOfLogic != null)
                        {
                            Say(locationInfo.OutOfLogic);
                        }
                        else if (roomInfo?.OutOfLogic != null)
                        {
                            Say(roomInfo.OutOfLogic);
                        }
                        else if (regionInfo?.OutOfLogic != null)
                        {
                            Say(regionInfo.OutOfLogic);
                        }
                        else
                        {
                            var missingItems = Logic.GetMissingRequiredItems(location, items)
                                .OrderBy(x => x.Length)
                                .FirstOrDefault();
                            if (missingItems == null)
                            {
                                Say(x => x.TrackedOutOfLogicItemTooManyMissing, item.Name, locationInfo.Name ?? location.Name);
                            }
                            else
                            {
                                var missingItemNames = NaturalLanguage.Join(missingItems.Select(ItemService.GetName));
                                Say(x => x.TrackedOutOfLogicItem, item.Name, locationInfo?.Name ?? location.Name, missingItemNames);
                            }
                        }
                        
                    }
                }
            }
            
            var addedEvent = History.AddEvent(
                HistoryEventType.TrackedItem,
                item.IsProgression(World.Config),
                item.NameWithArticle,
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
                    UpdateTrackerProgression = true;
                    addedEvent.IsUndone = true;
                });
            }
            
            GiveLocationHint(accessibleBefore);
            RestartIdleTimers();

            return didTrack;
        }

        /// <summary>
        /// Removes an item from the tracker.
        /// </summary>
        /// <param name="item">The item to untrack.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void UntrackItem(ItemData item, float? confidence = null)
        {
            var originalTrackingState = item.TrackingState;
            UpdateTrackerProgression = true;

            if (!item.Untrack())
            {
                Say(Responses.UntrackedNothing.Format(item.Name, item.NameWithArticle));
                return;
            }

            if (item.HasStages)
            {
                Say(Responses.UntrackedProgressiveItem.Format(item.Name, item.NameWithArticle));
            }
            else if (item.Multiple)
            {
                if (item.TrackingState > 0)
                {
                    if (item.CounterMultiplier > 1)
                        Say(Responses.UntrackedItemMultiple.Format($"{item.CounterMultiplier} {item.Plural}", $"{item.CounterMultiplier} {item.Plural}"));
                    else
                        Say(Responses.UntrackedItemMultiple.Format(item.Name, item.NameWithArticle));
                }
                else
                    Say(Responses.UntrackedItemMultipleLast.Format(item.Name, item.NameWithArticle));
            }
            else
            {
                Say(Responses.UntrackedItem.Format(item.Name, item.NameWithArticle));
            }

            IsDirty = true;
            OnItemTracked(new(null, confidence));
            AddUndo(() => { item.TrackingState = originalTrackingState; UpdateTrackerProgression = true; });
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
        public void TrackItem(ItemData item, DungeonInfo dungeon, string? trackedAs = null, float? confidence = null)
        {
            var tracked = TrackItem(item, trackedAs, confidence, tryClear: false);
            var undoTrack = _undoHistory.Pop();
            UpdateTrackerProgression = true;

            // Check if we can remove something from the remaining treasures in
            // a dungeon
            Action? undoTrackTreasure = null;
            if (tracked) // Only track treasure if we actually tracked anything
            {
                dungeon = GetDungeonFromItem(item, dungeon)!;
                if (TrackDungeonTreasure(dungeon, confidence))
                    undoTrackTreasure = _undoHistory.Pop();
            }

            IsDirty = true;

            // Check if we can remove something from the marked location
            var location = World.Locations
                .Where(x => dungeon.Is(x.Region))
                .TrySingle(x => x.ItemIs(item.InternalItemType, World));
            if (location != null)
            {
                location.Cleared = true;
                OnLocationCleared(new(location, confidence));

                if (MarkedLocations.ContainsKey(location.Id))
                {
                    MarkedLocations.Remove(location.Id);
                    OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
                }

                AddUndo(() =>
                {
                    undoTrack();
                    undoTrackTreasure?.Invoke();
                    location.Cleared = false;
                    UpdateTrackerProgression = true;
                });
            }
            else
            {
                AddUndo(() =>
                {
                    undoTrack();
                    undoTrackTreasure?.Invoke();
                    UpdateTrackerProgression = true;
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
        public void TrackItem(ItemData item, IHasLocations area, string? trackedAs = null, float? confidence = null)
        {
            var locations = area.Locations
                .Where(x => x.Item.Type == item.InternalItemType)
                .ToImmutableList();
            UpdateTrackerProgression = true;

            if (locations.Count == 0)
            {
                Say(Responses.AreaDoesNotHaveItem?.Format(item.Name, area.GetName(), item.NameWithArticle));
            }
            else if (locations.Count > 1)
            {
                // Consider tracking/clearing everything?
                Say(Responses.AreaHasMoreThanOneItem?.Format(item.Name, area.GetName(), item.NameWithArticle));
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
                    undoClear();
                    undoTrack();
                    UpdateTrackerProgression = true;
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
        public void TrackItemAmount(ItemData item, int count, float confidence)
        {
            UpdateTrackerProgression = true;

            var newItemCount = count;
            if (item.CounterMultiplier > 1
                && count % item.CounterMultiplier == 0)
            {
                newItemCount = count / item.CounterMultiplier.Value;
            }

            var oldItemCount = item.TrackingState;
            if (newItemCount == oldItemCount)
            {
                Say(Responses.TrackedExactAmountDuplicate.Format(item.Plural, count));
                return;
            }

            item.TrackingState = newItemCount;
            if (item.TryGetTrackingResponse(out var response))
            {
                Say(response.Format(item.Counter));
            }
            else if (newItemCount > oldItemCount)
            {
                Say(Responses.TrackedItemMultiple.Format(item.Plural ?? $"{item.Name}s", item.Counter, item.Name));
            }
            else
            {
                Say(Responses.UntrackedItemMultiple.Format(item.Plural ?? $"{item.Name}s", item.Plural ?? $"{item.Name}s"));
            }

            IsDirty = true;

            AddUndo(() => { item.TrackingState = oldItemCount; UpdateTrackerProgression = true; });
            OnItemTracked(new(null, confidence));
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
        public void ClearArea(IHasLocations area, bool trackItems, bool includeUnavailable = false, float? confidence = null, bool assumeKeys = false)
        {
            var locations = area.Locations
                .Where(x => !x.Cleared)
                .WhereUnless(includeUnavailable, x => x.IsAvailable(GetProgression(area)))
                .ToImmutableList();

            UpdateTrackerProgression = true;

            if (locations.Count == 0)
            {
                var outOfLogicLocations = area.Locations
                    .Where(x => !x.Cleared)
                    .Count();

                if (outOfLogicLocations > 1)
                    Say(Responses.TrackedNothingOutOfLogic[2].Format(area.Name, outOfLogicLocations));
                else if (outOfLogicLocations > 0)
                    Say(Responses.TrackedNothingOutOfLogic[1].Format(area.Name, outOfLogicLocations));
                else
                    Say(Responses.TrackedNothing.Format(area.Name));
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
                        var item = ItemService.GetOrDefault(onlyLocation);
                        if (item == null)
                        {
                            // Probably just the compass or something. Clear the
                            // location still, even if we can't track the item.
                            Clear(onlyLocation, confidence);
                        }
                        else
                        {
                            TrackItem(item: item, trackedAs: null, confidence: confidence, tryClear: true, autoTracked: false, location: onlyLocation);
                        }
                    }
                }
                else
                {
                    // Otherwise, start counting
                    var itemsCleared = 0;
                    var itemsTracked = new List<ItemData>();
                    var treasureTracked = 0;
                    foreach (var location in locations)
                    {
                        itemsCleared++;
                        if (!trackItems)
                        {
                            if (IsTreasure(location.Item) || World.Config.ZeldaKeysanity)
                                treasureTracked++;
                            location.Cleared = true;
                            OnLocationCleared(new(location, confidence));
                            continue;
                        }

                        var itemType = location.Item?.Type;
                        var item = itemType != null ? ItemService.GetOrDefault(itemType.Value) : null;
                        if (item == null || !item.Track())
                            _logger.LogWarning("Failed to track {itemType} in {area}.", itemType, area.Name); // Probably the compass or something, who cares
                        else
                            itemsTracked.Add(item);
                        if (IsTreasure(location.Item) || World.Config.ZeldaKeysanity)
                            treasureTracked++;

                        location.Cleared = true;
                    }

                    if (trackItems)
                    {
                        var itemNames = confidence >= Options.MinimumSassConfidence
                            ? NaturalLanguage.Join(itemsTracked, World.Config)
                            : $"{itemsCleared} items";
                        Say(x => x.TrackedMultipleItems, itemsCleared, area.GetName(), itemNames);

                        var roomInfo = area is Room room ? WorldInfo.Room(room) : null;
                        var regionInfo = area is Region region ?WorldInfo.Region(region) : null;

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
                            var progression = GetProgression(area);
                            var someOutOfLogicLocation = locations.Where(x => !x.IsAvailable(progression)).Random(s_random);
                            if (someOutOfLogicLocation != null && confidence >= Options.MinimumSassConfidence)
                            {
                                var someOutOfLogicItem = ItemService.GetOrDefault(someOutOfLogicLocation);
                                var missingItems = Logic.GetMissingRequiredItems(someOutOfLogicLocation, progression)
                                    .OrderBy(x => x.Length)
                                    .FirstOrDefault();
                                if (missingItems != null)
                                {
                                    var missingItemNames = NaturalLanguage.Join(missingItems.Select(ItemService.GetName));
                                    Say(x => x.TrackedOutOfLogicItem, someOutOfLogicItem?.Name, GetName(someOutOfLogicLocation), missingItemNames);
                                }
                                else
                                {
                                    Say(x => x.TrackedOutOfLogicItemTooManyMissing, someOutOfLogicItem?.Name, GetName(someOutOfLogicLocation));
                                }
                            }
                        }
                    }
                    else
                    {
                        Say(x => x.ClearedMultipleItems, itemsCleared, area.GetName());
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
                OnItemTracked(new ItemTrackedEventArgs(null, confidence));
            }

            IsDirty = true;

            AddUndo(() =>
            {
                foreach (var location in locations)
                {
                    if (trackItems)
                    {
                        var item = ItemService.GetOrDefault(location);
                        if (item != null && item.TrackingState > 0)
                            item.TrackingState--;
                    }
                    location.Cleared = false;
                }
                UpdateTrackerProgression = true;
            });
        }

        /// <summary>
        /// Marks all locations and treasure within a dungeon as cleared.
        /// </summary>
        /// <param name="dungeon">The dungeon to clear.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void ClearDungeon(DungeonInfo dungeon, float? confidence = null)
        {
            var remaining = dungeon.TreasureRemaining;
            if (remaining > 0)
            {
                dungeon.TreasureRemaining = 0;
            }

            // Clear the dungeon only if there's no bosses to defeat
            if (!dungeon.HasReward)
                dungeon.Cleared = true;

            var progress = GetProgression(assumeKeys: !World.Config.ZeldaKeysanity);
            var locations = dungeon.GetLocations(World).Where(x => !x.Cleared).ToList();
            var inaccessibleLocations = locations.Where(x => !x.IsAvailable(progress)).ToList();
            if (locations.Count > 0)
            {
                locations.ForEach(x => x.Cleared = true);
            }

            if (remaining <= 0 && locations.Count <= 0)
            {
                // We didn't do anything
                Say(x => x.DungeonAlreadyCleared, dungeon.Name);
                return;
            }

            Say(x => x.DungeonCleared, dungeon.Name);
            if (inaccessibleLocations.Count > 0 && confidence >= Options.MinimumSassConfidence)
            {
                var anyMissedLocation = inaccessibleLocations.Random(s_random);
                var locationInfo = WorldInfo.Location(anyMissedLocation);
                var missingItemCombinations = Logic.GetMissingRequiredItems(anyMissedLocation, progress);
                if (missingItemCombinations.Any())
                {
                    var missingItems = missingItemCombinations.Random(s_random)
                            .Select(ItemService.GetOrDefault)
                            .NonNull();
                    var missingItemsText = NaturalLanguage.Join(missingItems, World.Config);
                    Say(x => x.DungeonClearedWithInaccessibleItems, dungeon.Name, locationInfo.Name, missingItemsText);
                }
                else
                {
                    Say(x => x.DungeonClearedWithTooManyInaccessibleItems, dungeon.Name, locationInfo.Name);
                }
            }

            OnDungeonUpdated(new(confidence));
            AddUndo(() =>
            {
                dungeon.TreasureRemaining = remaining;
                if (remaining > 0 && !dungeon.HasReward)
                    dungeon.Cleared = false;
                locations.ForEach(x => x.Cleared = false);
            });
        }

        /// <summary>
        /// Clears an item from the specified location.
        /// </summary>
        /// <param name="location">The location to clear.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        /// <param name="autoTracked">If this was tracked by the auto tracker</param>
        public void Clear(Location location, float? confidence = null, bool autoTracked = false)
        {
            UpdateTrackerProgression = true;
            location.Cleared = true;

            if (confidence != null)
            {
                // Only use TTS if called from a voice command
                var locationName = GetName(location);
                Say(Responses.LocationCleared.Format(locationName));
            }

            if (MarkedLocations.ContainsKey(location.Id))
            {
                MarkedLocations.Remove(location.Id);
                OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
            }

            Action? undoTrackTreasure = TryTrackDungeonTreasure(location, confidence);

            Action? undoStopPegWorldMode = null;
            if (location == World.DarkWorldNorthWest.PegWorld)
            {
                StopPegWorldMode();

                if (!autoTracked)
                {
                    undoStopPegWorldMode = _undoHistory.Pop();
                }
            }

            IsDirty = true;

            if (!autoTracked)
            {
                AddUndo(() =>
                {
                    location.Cleared = false;
                    undoTrackTreasure?.Invoke();
                    undoStopPegWorldMode?.Invoke();
                    UpdateTrackerProgression = true;
                });
            }
            
            OnLocationCleared(new(location, confidence));
        }

        /// <summary>
        /// Marks a dungeon as cleared and, if possible, tracks the boss reward.
        /// </summary>
        /// <param name="dungeon">The dungeon that was cleared.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void MarkDungeonAsCleared(DungeonInfo dungeon, float? confidence = null)
        {
            UpdateTrackerProgression = true;

            if (dungeon.Cleared)
            {
                Say(Responses.DungeonBossAlreadyCleared.Format(dungeon.Name, dungeon.Boss));
                return;
            }

            var addedEvent = History.AddEvent(
                HistoryEventType.BeatBoss,
                true,
                dungeon.Boss.ToString() ?? $"boss of {dungeon.Name}"
            );

            dungeon.Cleared = true;
            Say(Responses.DungeonBossCleared.Format(dungeon.Name, dungeon.Boss));
            IsDirty = true;

            OnDungeonUpdated(new TrackerEventArgs(confidence));
            AddUndo(() =>
            {
                UpdateTrackerProgression = true;
                dungeon.Cleared = false;
                addedEvent.IsUndone = true;
            });
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
        public void MarkBossAsDefeated(BossInfo boss, bool admittedGuilt = true, float? confidence = null)
        {
            if (boss.Defeated)
            {
                Say(x => x.BossAlreadyDefeated, boss.Name);
                return;
            }

            boss.Defeated = true;

            if (!admittedGuilt && boss.WhenTracked != null)
                Say(boss.WhenTracked, boss.Name);
            else
                Say(boss.WhenDefeated ?? Responses.BossDefeated, boss.Name);

            var addedEvent = History.AddEvent(
                HistoryEventType.BeatBoss,
                true,
                boss.Name.ToString() ?? "boss"
            );

            IsDirty = true;
            UpdateTrackerProgression = true;

            OnBossUpdated(new(confidence));
            AddUndo(() =>
            {
                boss.Defeated = false;
                addedEvent.IsUndone = true;
            });
        }

        /// <summary>
        /// Un-marks a boss as defeated.
        /// </summary>
        /// <param name="boss">The boss that should be 'revived'.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void MarkBossAsNotDefeated(BossInfo boss, float? confidence = null)
        {
            if (!boss.Defeated)
            {
                Say(x => x.BossNotYetDefeated, boss.Name);
                return;
            }

            boss.Defeated = false;
            Say(Responses.BossUndefeated, boss.Name);

            IsDirty = true;
            UpdateTrackerProgression = true;

            OnBossUpdated(new(confidence));
            AddUndo(() => boss.Defeated = true);
        }

        /// <summary>
        /// Un-marks a dungeon as cleared and, if possible, untracks the boss
        /// reward.
        /// </summary>
        /// <param name="dungeon">The dungeon that should be un-cleared.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void MarkDungeonAsIncomplete(DungeonInfo dungeon, float? confidence = null)
        {
            if (!dungeon.Cleared)
            {
                Say(Responses.DungeonBossNotYetCleared.Format(dungeon.Name, dungeon.Boss));
                return;
            }

            UpdateTrackerProgression = true;
            dungeon.Cleared = false;
            Say(Responses.DungeonBossUncleared.Format(dungeon.Name, dungeon.Boss));

            // Try to untrack the associated boss reward item
            Action? undoUnclear = null;
            Action? undoUntrackTreasure = null;
            Action? undoUntrack = null;
            if (dungeon.LocationId != null)
            {
                var rewardLocation = World.Locations.Single(x => x.Id == dungeon.LocationId);
                if (rewardLocation.Item != null)
                {
                    var item = ItemService.GetOrDefault(rewardLocation);
                    if (item != null && item.TrackingState > 0)
                    {
                        UntrackItem(item);
                        undoUntrack = _undoHistory.Pop();
                    }

                    if (!rewardLocation.Item.IsDungeonItem)
                    {
                        dungeon.TreasureRemaining++;
                        undoUntrackTreasure = () => dungeon.TreasureRemaining--;
                    }
                }

                if (rewardLocation.Cleared)
                {
                    rewardLocation.Cleared = false;
                    OnLocationCleared(new(rewardLocation, null));
                    undoUnclear = () => rewardLocation.Cleared = true;
                }
            }

            IsDirty = true;

            OnDungeonUpdated(new TrackerEventArgs(confidence));
            AddUndo(() =>
            {
                dungeon.Cleared = false;
                undoUntrack?.Invoke();
                undoUntrackTreasure?.Invoke();
                undoUnclear?.Invoke();
                UpdateTrackerProgression = true;
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
        public void MarkLocation(Location location, ItemData item, float? confidence = null)
        {
            var locationName = GetName(location);
            GiveLocationComment(item, location, isTracking: false, confidence);

            if (item.InternalItemType == ItemType.Nothing)
            {
                Clear(location);
                Say(Responses.LocationMarkedAsBullshit.Format(locationName));
            }
            else if (MarkedLocations.TryGetValue(location.Id, out var oldItem))
            {
                MarkedLocations[location.Id] = item;
                Say(Responses.LocationMarkedAgain.Format(locationName, item.Name, oldItem.Name));
                AddUndo(() => MarkedLocations[location.Id] = oldItem);
            }
            else
            {
                MarkedLocations.Add(location.Id, item);
                Say(Responses.LocationMarked.Format(locationName, item.Name));
                AddUndo(() => MarkedLocations.Remove(location.Id));
            }

            IsDirty = true;

            OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
        }

        /// <summary>
        /// Pegs a Peg World peg.
        /// </summary>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void Peg(float? confidence = null)
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
        public void StartPegWorldMode(float? confidence = null)
        {
            PegWorldMode = true;
            Say(Responses.PegWorldModeOn, wait: true);
            OnPegWorldModeToggled(new TrackerEventArgs(confidence));
            AddUndo(() => PegWorldMode = false);
        }

        /// <summary>
        /// Turns Peg World mode off.
        /// </summary>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void StopPegWorldMode(float? confidence = null)
        {
            PegWorldMode = false;
            Say(Responses.PegWorldModeDone);
            OnPegWorldModeToggled(new TrackerEventArgs(confidence));
            AddUndo(() => PegWorldMode = true);
        }

        /// <summary>
        /// Updates the region that the player is in
        /// </summary>
        /// <param name="region">The region the player is in</param>
        /// <param name="updateMap">Set to true to update the map for the player to match the region</param>
        /// <param name="resetTime">If the time should be reset if this is the first region update</param>
        public void UpdateRegion(Region region, bool updateMap = false, bool resetTime = false)
        {
            UpdateRegion(WorldInfo.Regions.First(x => x.GetRegion(World) == region), updateMap, resetTime);
        }

        /// <summary>
        /// Updates the region that the player is in
        /// </summary>
        /// <param name="region">The region the player is in</param>
        /// <param name="updateMap">Set to true to update the map for the player to match the region</param>
        /// <param name="resetTime">If the time should be reset if this is the first region update</param>
        public void UpdateRegion(RegionInfo region, bool updateMap = false, bool resetTime = false)
        {
            if (region != CurrentRegion)
            {
                if (resetTime && History.GetHistory().Count == 0)
                {
                    ResetTimer(true);
                }

                History.AddEvent(
                    HistoryEventType.EnteredRegion,
                    true,
                    region.Name.ToString() ?? "new region"
                );
            }

            CurrentRegion = region;
            if (updateMap)
            {
                UpdateMap(region.MapName);
            }
        }

        /// <summary>
        /// Updates the map to display for the user
        /// </summary>
        /// <param name="map">The name of the map</param>
        public void UpdateMap(string map)
        {
            CurrentMap = map;
            MapUpdated?.Invoke(this, EventArgs.Empty);
        }

        internal void RestartIdleTimers()
        {
            foreach (var item in _idleTimers)
            {
                var timeout = Parse.AsTimeSpan(item.Key, s_random) ?? Timeout.InfiniteTimeSpan;
                var timer = item.Value;

                timer.Change(timeout, Timeout.InfiniteTimeSpan);
            }
        }

        /// <summary>
        /// Returns the possible names of the specified location.
        /// </summary>
        /// <param name="location">The location whose names to get.</param>
        /// <returns>
        /// A new <see cref="SchrodingersString"/> object representing the
        /// possible names of <paramref name="location"/>.
        /// </returns>
        protected internal virtual SchrodingersString GetName(Location location)
            => WorldInfo.Location(location).Name;

        /// <summary>
        /// Determines whether or not the specified reward is worth getting.
        /// </summary>
        /// <param name="reward">The dungeon reward.</param>
        /// <returns>
        /// <see langword="true"/> if the reward leads to something good;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected internal bool IsWorth(RewardType reward)
        {
            var sahasrahlaItem = ItemService.GetOrDefault(World.LightWorldNorthEast.SahasrahlasHideout.Sahasrahla);
            if (sahasrahlaItem != null && reward == RewardType.PendantGreen)
            {
                _logger.LogDebug("{Reward} leads to {Item}...", reward, sahasrahlaItem);
                if (IsWorth(sahasrahlaItem))
                {
                    _logger.LogDebug("{Reward} leads to {Item}, which is worth it", reward, sahasrahlaItem);
                    return true;
                }
                _logger.LogDebug("{Reward} leads to {Item}, which is junk", reward, sahasrahlaItem);
            }

            var pedItem = ItemService.GetOrDefault(World.LightWorldNorthWest.MasterSwordPedestal);
            if (pedItem != null && (reward is RewardType.PendantGreen or RewardType.PendantRed or RewardType.PendantBlue))
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
        protected internal bool IsWorth(ItemData item)
        {
            var leads = new Dictionary<ItemType, Location[]>()
            {
                [ItemType.Mushroom] = new[] { World.LightWorldNorthEast.MushroomItem },
                [ItemType.Powder] = new[] { World.LightWorldNorthWest.MagicBat },
                [ItemType.Book] = new[]
                {
                    World.LightWorldDeathMountainWest.EtherTablet,
                    World.LightWorldSouth.BombosTablet
                }
            };

            if (leads.TryGetValue(item.InternalItemType, out var leadsToLocation))
            {
                foreach (var location in leadsToLocation)
                {
                    var reward = ItemService.GetOrDefault(location);
                    if (reward != null)
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

            return item.IsGood(World.Config);
        }

        /// <summary>
        /// Adds an action to be invoked to undo the last operation.
        /// </summary>
        /// <param name="undo">
        /// The action to invoke to undo the last operation.
        /// </param>
        protected internal virtual void AddUndo(Action undo) => _undoHistory.Push(undo);

        /// <summary>
        /// Cleans up resources used by this class.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to dispose of managed resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _recognizer.Dispose();
                    (_communicator as IDisposable)?.Dispose();

                    foreach (var timer in _idleTimers.Values)
                        timer.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="ItemTracked"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnItemTracked(ItemTrackedEventArgs e)
            => ItemTracked?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="ToggledPegWorldModeOn"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnPegWorldModeToggled(TrackerEventArgs e)
            => ToggledPegWorldModeOn?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="PegPegged"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnPegPegged(TrackerEventArgs e)
            => PegPegged?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DungeonUpdated"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnDungeonUpdated(TrackerEventArgs e)
            => DungeonUpdated?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="BossUpdated"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnBossUpdated(TrackerEventArgs e)
            => BossUpdated?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="MarkedLocationsUpdated"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnMarkedLocationsUpdated(TrackerEventArgs e)
            => MarkedLocationsUpdated?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="GoModeToggledOn"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnGoModeToggledOn(TrackerEventArgs e)
            => GoModeToggledOn?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="LocationCleared"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnLocationCleared(LocationClearedEventArgs e)
            => LocationCleared?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="ActionUndone"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnActionUndone(TrackerEventArgs e)
            => ActionUndone?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="StateLoaded"/> event.
        /// </summary>
        protected virtual void OnStateLoaded()
            => StateLoaded?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="SpeechRecognized"/> event.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected virtual void OnSpeechRecognized(TrackerEventArgs e)
            => SpeechRecognized?.Invoke(this, e);

        private static bool IsTreasure(ItemData item)
            => !item.InternalItemType.IsInAnyCategory(ItemCategory.BigKey, ItemCategory.SmallKey, ItemCategory.Map, ItemCategory.Compass);

        private static bool IsTreasure(Item? item)
            => item != null && !item.IsDungeonItem;

        private DungeonInfo? GetDungeonFromLocation(Location location)
        {
            if (location.Type == LocationType.NotInDungeon)
                return null;

            return WorldInfo.Dungeons.SingleOrDefault(x => x.Is(location.Region));
        }

        private DungeonInfo? GetDungeonFromArea(IHasLocations area)
        {
            return area switch
            {
                Room room => WorldInfo.Dungeons.SingleOrDefault(x => x.Is(room.Region)),
                Region region => WorldInfo.Dungeons.SingleOrDefault(x => x.Is(region)),
                _ => null
            };
        }

        private Action? TryTrackDungeonTreasure(ItemData item, float? confidence)
        {
            if (confidence < Options.MinimumSassConfidence)
            {
                // Tracker response could give away the location of an item if
                // it is in a dungeon but tracker misheard.
                return null;
            }

            var dungeon = GetDungeonFromItem(item);
            if (dungeon != null && (IsTreasure(item) || World.Config.ZeldaKeysanity))
            {
                if (TrackDungeonTreasure(dungeon, confidence))
                    return _undoHistory.Pop();
            }

            IsDirty = true;

            return null;
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
                    return _undoHistory.Pop();
            }

            IsDirty = true;

            return null;
        }

        private void GetTreasureCounts(IReadOnlyCollection<DungeonInfo> dungeons, World world)
        {
            if (!world.Items.Any())
                return;

            foreach (var dungeon in dungeons)
            {
                var region = world.Regions.SingleOrDefault(x => dungeon.Is(x));
                if (region != null)
                {
                    dungeon.TreasureRemaining = region.Locations.Count(x => (IsTreasure(x.Item) || World.Config.ZeldaKeysanity) && x.Type != LocationType.NotInDungeon);
                    _logger.LogDebug("Found {TreasureRemaining} item(s) in {dungeon}", dungeon.TreasureRemaining, dungeon.Name);
                }
                else
                {
                    _logger.LogWarning("Could not find region for dungeon {dungeon}.", dungeon.Name);
                }
            }
        }

        private void GiveLocationComment(ItemData item, Location location, bool isTracking, float? confidence)
        {
            // Give some sass if the user tracks or marks the wrong item at a
            // location
            if (location.Item != null && !item.Is(location.Item.Type))
            {
                if (confidence == null || confidence < Options.MinimumSassConfidence)
                    return;

                var actualItemName = ItemService.GetName(location.Item.Type);
                if (HintsEnabled) actualItemName = "another item";

                Say(Responses.LocationHasDifferentItem?.Format(item.NameWithArticle, actualItemName));
            }
            else
            {
                if (item.InternalItemType == location.VanillaItem)
                {
                    Say(x => x.TrackedVanillaItem);
                    return;
                }

                var locationInfo = WorldInfo.Location(location);
                var isJunk = item.IsJunk(World.Config);
                if (isJunk)
                {
                    if (!isTracking && locationInfo.WhenMarkingJunk?.Count > 0)
                    {
                        Say(locationInfo.WhenMarkingJunk.Random(s_random));
                    }
                    else if (locationInfo.WhenTrackingJunk?.Count > 0)
                    {
                        Say(locationInfo.WhenTrackingJunk.Random(s_random));
                    }
                }
                else if (!isJunk)
                {
                    if (!isTracking && locationInfo.WhenMarkingProgression?.Count > 0)
                    {
                        Say(locationInfo.WhenMarkingProgression.Random(s_random));
                    }
                    else if (locationInfo.WhenTrackingProgression?.Count > 0)
                    {
                        Say(locationInfo.WhenTrackingProgression.Random(s_random));
                    }
                }
            }
        }

        private DungeonInfo? GetDungeonFromItem(ItemData item, DungeonInfo? dungeon = null)
        {
            var locations = World.Locations
                .Where(x => !x.Cleared && x.Type != LocationType.NotInDungeon && x.ItemIs(item.InternalItemType, World))
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
                if (!locations.Any(x => dungeon.Is(x.Region)))
                {
                    // Be a smart-ass about it
                    Say(Responses.ItemTrackedInIncorrectDungeon?.Format(dungeon.Name, item.NameWithArticle));
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
            Say(Responses.Idle[key]);
        }

        private void Recognizer_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            RestartIdleTimers();
            OnSpeechRecognized(new(e.Result.Confidence, e.Result.Text));
        }

        private void GiveLocationHint(IEnumerable<Location> accessibleBefore)
        {
            if (World == null)
                return;

            var accessibleAfter = GetAccessibleLocations();
            var newlyAccessible = accessibleAfter.Except(accessibleBefore);
            if (newlyAccessible.Any())
            {
                var regions = newlyAccessible.GroupBy(x => x.Region)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key.Name);

                if (newlyAccessible.Contains(World.InnerMaridia.ShaktoolItem))
                    Say(Responses.ShaktoolAvailable);

                if (newlyAccessible.Contains(World.DarkWorldNorthWest.PegWorld))
                    Say(Responses.PegWorldAvailable);
            }
            else if (Responses.TrackedUselessItem != null)
            {
                Say(Responses.TrackedUselessItem);
            }
        }

        private IEnumerable<Location> GetAccessibleLocations()
        {
            if (World == null)
                return Enumerable.Empty<Location>();

            var items = new List<SMZ3.Item>();
            foreach (var item in ItemService.TrackedItems())
            {
                for (var i = 0; i < item.TrackingState; i++)
                    items.Add(new SMZ3.Item(item.InternalItemType));
            }

            var progression = new Progression(items, GetCurrentRewards());
            return World.Locations.Where(x => x.IsAvailable(progression)).ToList();
        }

        private IEnumerable<Reward> GetCurrentRewards()
        {
            var dungeonRewards = WorldInfo.Dungeons
                .Where(d => d.Cleared && d.HasReward && d.Reward != RewardItem.Unknown)
                .Select(d => new Reward(d.Reward.ToRewardType()));
            var bossRewards = WorldInfo.Bosses
                .Where(b => b.Defeated && b.Reward != RewardType.None)
                .Select(b => new Reward(b.Reward));
            return dungeonRewards.Concat(bossRewards);
        }
    }
}
