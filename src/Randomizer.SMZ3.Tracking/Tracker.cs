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
using Randomizer.Shared.Models;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Tracking.Configuration;
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

        private readonly SpeechSynthesizer _tts;
        private readonly SpeechRecognitionEngine _recognizer;
        private readonly TrackerModuleFactory _moduleFactory;
        private readonly IChatClient _chatClient;
        private readonly ILogger<Tracker> _logger;
        private readonly Dictionary<string, Timer> _idleTimers;
        private readonly Stack<Action> _undoHistory = new();
        private readonly RandomizerContext _dbContext;

        private DateTime _startTime = DateTime.MinValue;
        private bool _disposed;
        private string? _mood;
        private string? _lastSpokenText;
        private Dictionary<string, Progression> _progression = new();
        private bool _alternateTracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="config">The tracking configuration.</param>
        /// <param name="locationConfig">The location configuration.</param>
        /// <param name="worldAccessor">
        /// Used to get the world to track in.
        /// </param>
        /// <param name="moduleFactory">
        /// Used to provide the tracking speech recognition syntax.
        /// </param>
        /// <param name="chatClient"></param>
        /// <param name="logger">Used to write logging information.</param>
        /// <param name="options">Provides Tracker preferences.</param>
        /// <param name="dbContext">The database context</param>
        public Tracker(TrackerConfig config,
            LocationConfig locationConfig,
            IWorldAccessor worldAccessor,
            TrackerModuleFactory moduleFactory,
            IChatClient chatClient,
            ILogger<Tracker> logger,
            TrackerOptions options,
            RandomizerContext dbContext)
        {
            _moduleFactory = moduleFactory;
            _chatClient = chatClient;
            _logger = logger;
            Options = options;
            _dbContext = dbContext;

            // Initialize the tracker configuration
            Items = config.Items;
            Pegs = config.Pegs;
            Responses = config.Responses;
            Requests = config.Requests;
            World = worldAccessor.GetWorld();
            WorldInfo = locationConfig;
            GetTreasureCounts(WorldInfo.Dungeons, World);
            UpdateTrackerProgression = true;

            // Initalize the timers used to trigger idle responses
            _idleTimers = Responses.Idle.ToDictionary(
                x => x.Key,
                x => new Timer(IdleTimerElapsed, x.Key, Timeout.Infinite, Timeout.Infinite));

            // Initialize the text-to-speech
            if (s_random.NextDouble() <= 0.01)
                _alternateTracker = true;

            _tts = new SpeechSynthesizer();
            _tts.SelectVoiceByHints(_alternateTracker ? VoiceGender.Male : VoiceGender.Female);

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
        public LocationConfig WorldInfo { get; }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        public IReadOnlyCollection<ItemData> Items { get; }

        /// <summary>
        /// Get a collection of pegs in Peg World mode.
        /// </summary>
        public IReadOnlyCollection<Peg> Pegs { get; }

        /// <summary>
        /// Gets the world for the currently tracked playthrough.
        /// </summary>
        public World World { get; internal set; }

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
        public TrackerOptions Options { get; }

        /// <summary>
        /// The generated rom
        /// </summary>
        public GeneratedRom Rom { get; private set; }

        /// <summary>
        /// The region the player is currently in
        /// </summary>
        public RegionInfo CurrentRegion { get; private set; }

        /// <summary>
        /// The map to display for the player
        /// </summary>
        public string CurrentMap { get; private set; }

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
        public AutoTrackerModule AutoTracker { get; set; }

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
            state.Apply(this);
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
                state.Apply(this);
                OnStateLoaded();
                
                return true;
            }
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
            var state = TrackerState.TakeSnapshot(this);
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
            var state = TrackerState.TakeSnapshot(this);
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
        public bool TrackDungeonTreasure(DungeonInfo dungeon, float? confidence = null, int amount = 1)
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
                if (confidence != null || dungeon.TreasureRemaining >= 1)
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
            else if (confidence != null && Responses.DungeonTreasureTracked.TryGetValue(-1, out var response))
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
                Say(Responses.DungeonRewardMarked.Format(dungeon.Name, dungeon.Reward.GetName()));
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
                Say(Responses.RemainingDungeonsMarked.Format(reward.GetName()));

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
                    && medallionRegion.Medallion != medallion.Value.ToItemType())
                {
                    Say(Responses.DungeonRequirementMismatch?.Format(
                        medallionRegion.Medallion.ToString(),
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
        /// Returns the item that has the specified name.
        /// </summary>
        /// <param name="name">The name of the item to find.</param>
        /// <returns>
        /// The <see cref="ItemData"/> with the specified name, or <c>null</c>
        /// if no items have a matching name.
        /// </returns>
        /// <remarks>
        /// Names are case insensitive, and items can be found either by their
        /// default name, additional names, or stage names.
        /// </remarks>
        public ItemData? FindItemByName(string name)
        {
            return Items.SingleOrDefault(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                ?? Items.SingleOrDefault(x => x.GetStage(name) != null);
        }

        /// <summary>
        /// Returns the first item with the specified item type.
        /// </summary>
        /// <param name="type">The type of the item to find.</param>
        /// <returns>
        /// The item data for the item type, or <c>null</c> if no item data is
        /// present for the specified type.
        /// </returns>
        public ItemData? FindItemByType(ItemType type)
        {
            return Items.FirstOrDefault(x => x.InternalItemType == type);
        }

        /// <summary>
        /// Returns the first item that matches the item at the specified
        /// location.
        /// </summary>
        /// <param name="location">The location with the item to find.</param>
        /// <returns>
        /// The item data for the item at the location, or <c>null</c> if no
        /// item data is present for the item at the location.
        /// </returns>
        public ItemData? FindItem(Location location)
        {
            return Items.FirstOrDefault(x => x.InternalItemType == location.Item.Type);
        }

        /// <summary>
        /// Gets the currently available items.
        /// </summary>
        /// <returns>
        /// A new <see cref="Progression"/> object representing the currently
        /// available items.
        /// </returns>
        /// <remarks>
        /// Keycards and dungeon items such as keys are assumed to be owned,
        /// unless playing on a keysanity world, in which case keys and keycards
        /// must be tracked manually.
        /// </remarks>
        public Progression GetProgression()
            => GetProgression(assumeKeys: !World.Config.Keysanity);

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

            if (!World.Config.Keysanity || assumeKeys)
            {
                progression.AddRange(Item.CreateKeycards(World));
                if (assumeKeys)
                    progression.AddRange(Item.CreateDungeonPool(World));
            }

            foreach (var item in Items)
            {
                if (item.TrackingState > 0)
                    progression.AddRange(Enumerable.Repeat(item.InternalItemType, item.TrackingState));
            }

            _progression[mapKey] = progression;
            return progression;
        }

        /// <summary>
        /// Starts voice recognition.
        /// </summary>
        public virtual void StartTracking()
        {
            // Load the modules for voice recognition
            StartTimer();
            Syntax = _moduleFactory.LoadAll(this, _recognizer);
            EnableVoiceRecognition();
            Say(_alternateTracker ? Responses.StartingTrackingAlternate : Responses.StartedTracking);
            RestartIdleTimers();
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
                    _chatClient.Connect(userName, oauthToken, channel ?? userName, id);
                }
                catch (AggregateException e)
                {
                    _logger.LogError("Error in connection to Twitch chat", e);
                    Say(x => x.Chat.WhenDisconnected);
                }
            }
        }

        /// <summary>
        /// Sets the start time of the timer
        /// </summary>
        public virtual void StartTimer()
        {
            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Resets the timer to 0
        /// </summary>
        public virtual void ResetTimer()
        {
            SavedElapsedTime = TimeSpan.Zero;
            StartTimer();
        }

        /// <summary>
        /// Pauses the timer, saving the elapsed time
        /// </summary>
        public virtual void PauseTimer()
        {
            SavedElapsedTime = TotalElapsedTime;
            _startTime = DateTime.MinValue;
        }

        /// <summary>
        /// Stops voice recognition.
        /// </summary>
        public virtual void StopTracking()
        {
            DisableVoiceRecognition();
            _tts.SpeakAsyncCancelAll();
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
            var prompt = ParseText(formattedText);
            if (wait)
                _tts.Speak(prompt);
            else
                _tts.SpeakAsync(prompt);
            _lastSpokenText = text;
            return true;

            static Prompt ParseText(string text)
            {
                // If text does not contain any XML elements, just interpret it
                // as text
                if (!text.Contains("<") && !text.Contains("/>"))
                    return new Prompt(text);

                var prompt = new PromptBuilder();
                prompt.AppendSsmlMarkup(text);
                return new Prompt(prompt);
            }
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
            if (_lastSpokenText == null)
            {
                Say("I haven't said anything yet.");
                return;
            }

            _tts.Speak("I said");
            _tts.Rate -= RepeatRateModifier;
            Say(_lastSpokenText, wait: true);
            _tts.Rate += RepeatRateModifier;
        }

        /// <summary>
        /// Makes Tracker stop talking.
        /// </summary>
        public virtual void ShutUp()
        {
            _tts.SpeakAsyncCancelAll();
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
        /// <returns>
        /// <see langword="true"/> if the item was actually tracked; <see
        /// langword="false"/> if the item could not be tracked, e.g. when
        /// tracking Bow twice.
        /// </returns>
        public bool TrackItem(ItemData item, string? trackedAs = null, float? confidence = null, bool tryClear = true, bool autoTracked = false)
        {
            var didTrack = false;
            var accessibleBefore = GetAccessibleLocations();
            var itemName = item.Name;
            var originalTrackingState = item.TrackingState;
            UpdateTrackerProgression = true;
            var stateItem = !autoTracked || !item.InternalItemType.IsInAnyCategory(ItemCategory.BigKey, ItemCategory.SmallKey) || World.Config.Keysanity;

            if (item.HasStages)
            {
                if (trackedAs != null && item.GetStage(trackedAs) != null)
                {
                    var stage = item.GetStage(trackedAs)!;

                    // Tracked by specific stage name (e.g. Tempered Sword), set
                    // to that stage specifically
                    var stageName = item.Stages[stage.Value].ToString();

                    didTrack = item.Track(stage.Value);
                    if (stateItem)
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
                    if (stateItem)
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
                    if (stateItem)
                        Say(response.Format(item.Counter));
                }
                else if (item.Counter == 1)
                {
                    if (stateItem)
                        Say(Responses.TrackedItem.Format(itemName, item.NameWithArticle));
                }
                else if (item.Counter > 1)
                {
                    if (stateItem)
                        Say(Responses.TrackedItemMultiple.Format(item.Plural ?? $"{itemName}s", item.Counter));
                }
                else
                {
                    _logger.LogWarning("Encountered multiple item with counter 0: {item} has counter {counter}", item, item.Counter);
                    if (stateItem)
                        Say(Responses.TrackedItem.Format(itemName, item.NameWithArticle));
                }
            }
            else
            {
                didTrack = item.Track();
                if (stateItem)
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

            var location = World.Locations.TrySingle(x => x.Cleared == false && x.Item.Type == item.InternalItemType);
            if (location != null)
            {
                if (tryClear)
                {
                    // If this item was in a dungeon, track treasure count
                    undoTrackDungeonTreasure = TryTrackDungeonTreasure(item, confidence);

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

                var items = GetProgression();
                if (!location.IsAvailable(items) && (confidence >= Options.MinimumSassConfidence || autoTracked))
                {
                    var locationInfo = WorldInfo.Location(location);
                    var missingItems = Logic.GetMissingRequiredItems(location, items)
                        .OrderBy(x => x.Length)
                        .FirstOrDefault();
                    if (missingItems == null)
                    {
                        Say(x => x.TrackedOutOfLogicItemTooManyMissing, item.Name, locationInfo.Name ?? location.Name);
                    }
                    else
                    {
                        var missingItemNames = NaturalLanguage.Join(missingItems.Select(GetName));
                        Say(x => x.TrackedOutOfLogicItem, item.Name, locationInfo?.Name ?? location.Name, missingItemNames);
                    }
                }
            }

            IsDirty = true;

            if (!autoTracked)
            {
                AddUndo(() =>
                {
                    undoTrack();
                    undoClear?.Invoke();
                    undoTrackDungeonTreasure?.Invoke();
                    UpdateTrackerProgression = true;
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
        /// Tracks the specified item and clears the specified location.
        /// </summary>
        /// <param name="item">The item data to track.</param>
        /// <param name="trackedAs">
        /// The text that was tracked, when triggered by voice command.
        /// </param>
        /// <param name="location">The location the item was found in.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        /// <param name="autoTracked">If this was tracked by the auto tracker</param>
        public void TrackItem(ItemData item, Location location, string? trackedAs = null, float? confidence = null, bool autoTracked = false)
        {
            GiveLocationComment(item, location, isTracking: true, confidence);
            TrackItem(item, trackedAs, confidence, tryClear: false, autoTracked: autoTracked);
            Clear(location, null, autoTracked);

            UpdateTrackerProgression = true;
            IsDirty = true;

            if (!autoTracked)
            {
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
                Say(Responses.TrackedItemMultiple.Format(item.Plural ?? $"{item.Name}s", item.Counter));
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
                .WhereUnless(includeUnavailable, x => x.IsAvailable(GetProgression()))
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
                        var item = Items.SingleOrDefault(x => x.InternalItemType == onlyLocation.Item.Type);
                        if (item == null)
                        {
                            // Probably just the compass or something. Clear the
                            // location still, even if we can't track the item.
                            Clear(onlyLocation, confidence);
                        }
                        else
                        {
                            TrackItem(item, onlyLocation, confidence: confidence);
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
                            if (IsTreasure(location.Item))
                                treasureTracked++;
                            location.Cleared = true;
                            OnLocationCleared(new(location, confidence));
                            continue;
                        }

                        var itemType = location.Item?.Type;
                        var item = Items.SingleOrDefault(x => x.InternalItemType == itemType);
                        if (item == null || !item.Track())
                            _logger.LogWarning("Failed to track {itemType} in {area}.", itemType, area.Name); // Probably the compass or something, who cares
                        else
                            itemsTracked.Add(item);
                        if (IsTreasure(location.Item))
                            treasureTracked++;

                        location.Cleared = true;
                    }

                    if (trackItems)
                    {
                        var itemNames = confidence >= Options.MinimumSassConfidence
                            ? NaturalLanguage.Join(itemsTracked, World.Config)
                            : $"{itemsCleared} items";
                        Say(x => x.TrackedMultipleItems, itemsCleared, area.GetName(), itemNames);

                        var someOutOfLogicLocation = locations.Where(x => !x.IsAvailable(GetProgression())).Random(s_random);
                        if (someOutOfLogicLocation != null)
                        {
                            var someOutOfLogicItem = FindItemByType(someOutOfLogicLocation.Item.Type);
                            var missingItems = Logic.GetMissingRequiredItems(someOutOfLogicLocation, GetProgression())
                                .OrderBy(x => x.Length)
                                .FirstOrDefault();
                            if (missingItems != null)
                            {
                                var missingItemNames = NaturalLanguage.Join(missingItems.Select(GetName));
                                Say(x => x.TrackedOutOfLogicItem, someOutOfLogicItem?.Name, GetName(someOutOfLogicLocation), missingItemNames);
                            }
                            else
                            {
                                Say(x => x.TrackedOutOfLogicItemTooManyMissing, someOutOfLogicItem?.Name, GetName(someOutOfLogicLocation));
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
                        var item = Items.SingleOrDefault(x => x.InternalItemType == location.Item.Type);
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

            var progress = GetProgression();
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
            if (inaccessibleLocations.Count > 0)
            {
                var anyMissedLocation = inaccessibleLocations.Random(s_random);
                var locationInfo = WorldInfo.Location(anyMissedLocation);
                var missingItemCombinations = Logic.GetMissingRequiredItems(anyMissedLocation, progress);
                if (missingItemCombinations.Any())
                {
                    var missingItems = missingItemCombinations.Random(s_random)
                            .Select(FindItemByType)
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

            Action? undoTrackTreasure = null;
            var dungeon = GetDungeonFromLocation(location);
            if (dungeon != null && IsTreasure(location.Item))
            {
                TrackDungeonTreasure(dungeon, confidence);

                if (!autoTracked)
                {
                    undoTrackTreasure = _undoHistory.Pop();
                }
            }

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

            dungeon.Cleared = true;
            Say(Responses.DungeonBossCleared.Format(dungeon.Name, dungeon.Boss));

            // Try to track the associated boss reward item
            Action? undoTrack = null;
            if (dungeon.LocationId != null)
            {
                var rewardLocation = World.Locations.Single(x => x.Id == dungeon.LocationId);
                if (!rewardLocation.Cleared)
                {
                    if (rewardLocation.Item != null)
                    {
                        var item = Items.FirstOrDefault(x => x.InternalItemType == rewardLocation.Item.Type);
                        if (item != null)
                        {
                            TrackItem(item, rewardLocation);
                            undoTrack = _undoHistory.Pop();
                        }
                    }

                    if (undoTrack == null)
                    {
                        // Couldn't track an item, so just clear the location
                        Clear(rewardLocation);
                        undoTrack = _undoHistory.Pop();
                    }
                }
            }

            IsDirty = true;

            OnDungeonUpdated(new TrackerEventArgs(confidence));
            AddUndo(() =>
            {
                UpdateTrackerProgression = true;
                dungeon.Cleared = false;
                undoTrack?.Invoke();
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

            OnBossUpdated(new(confidence));
            AddUndo(() => boss.Defeated = false);
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
                    var item = Items.FirstOrDefault(x => x.InternalItemType == rewardLocation.Item.Type);
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
        /// <param name="peg">The peg to peg.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void Peg(Peg peg, float? confidence = null)
        {
            if (!PegWorldMode)
                return;

            peg.Pegged = true;

            if (Pegs.Any(x => !x.Pegged))
                Say(Responses.PegWorldModePegged);
            else
                Say(Responses.PegWorldModeDone);
            OnPegPegged(new TrackerEventArgs(confidence));
            AddUndo(() => peg.Pegged = false);

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
        public void UpdateRegion(Region region, bool updateMap = false)
        {
            UpdateRegion(WorldInfo.Regions.First(x => x.GetRegion(World) == region), updateMap);
        }

        /// <summary>
        /// Updates the region that the player is in
        /// </summary>
        /// <param name="region">The region the player is in</param>
        /// <param name="updateMap">Set to true to update the map for the player to match the region</param>
        public void UpdateRegion(RegionInfo region, bool updateMap = false)
        {
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
        /// Returns a name for the specified item.
        /// </summary>
        /// <param name="item">The type of item whose name to get.</param>
        /// <returns>
        /// One of the possible names for the <paramref name="item"/>.
        /// </returns>
        protected internal virtual string GetName(ItemType item)
            => Items.SingleOrDefault(x => x.InternalItemType == item)?.NameWithArticle.ToString() ?? item.GetDescription();

        /// <summary>
        /// Determines whether or not the specified reward is worth getting.
        /// </summary>
        /// <param name="reward">The dungeon reward.</param>
        /// <returns>
        /// <see langword="true"/> if the reward leads to something good;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected internal bool IsWorth(Reward reward)
        {
            var sahasrahlaItem = FindItemByType(World.LightWorldNorthEast.SahasrahlasHideout.Sahasrahla.Item.Type);
            if (sahasrahlaItem != null && reward == Reward.PendantGreen)
            {
                _logger.LogDebug("{Reward} leads to {Item}...", reward, sahasrahlaItem);
                if (IsWorth(sahasrahlaItem))
                {
                    _logger.LogDebug("{Reward} leads to {Item}, which is worth it", reward, sahasrahlaItem);
                    return true;
                }
                _logger.LogDebug("{Reward} leads to {Item}, which is junk", reward, sahasrahlaItem);
            }

            var pedItem = FindItemByType(World.LightWorldNorthWest.MasterSwordPedestal.Item.Type);
            if (pedItem != null && (reward == Reward.PendantGreen || reward == Reward.PendantNonGreen))
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
                    var reward = FindItem(location);
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
                    _tts.Dispose();

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
            if (dungeon != null && IsTreasure(item))
            {
                if (TrackDungeonTreasure(dungeon, confidence))
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
                    dungeon.TreasureRemaining = region.Locations.Count(x => !x.Item.IsDungeonItem && x.Type != LocationType.NotInDungeon);
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

                var actualItemName = Items.FirstOrDefault(x => x.InternalItemType == location.Item.Type)?.NameWithArticle
                        ?? location.Item.Name;
                Say(Responses.LocationHasDifferentItem?.Format(item.NameWithArticle, actualItemName));
            }
            else
            {
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
            foreach (var item in Items)
            {
                for (var i = 0; i < item.TrackingState; i++)
                    items.Add(new SMZ3.Item(item.InternalItemType));
            }

            var progression = new Progression(items);
            return World.Locations.Where(x => x.IsAvailable(progression)).ToList();
        }
    }
}
