using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

using BunLabs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using Parse = BunLabs.Parse;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Tracks items and locations in a playthrough by listening for voice
    /// commands and responding with text-to-speech.
    /// </summary>
    public class Tracker : IDisposable
    {
        private static readonly Random s_random = new();

        private readonly SpeechSynthesizer _tts;
        private readonly SpeechRecognitionEngine _recognizer;
        private readonly TrackerModuleFactory _moduleFactory;
        private readonly ILogger<Tracker> _logger;
        private readonly Dictionary<string, Timer> _idleTimers;
        private readonly Stack<Action> _undoHistory = new();
        private readonly RandomizerContext _dbContext;
        private DateTime _startTime = DateTime.MinValue;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="config">The tracking configuration.</param>
        /// <param name="worldAccessor">
        /// Used to get the world to track in.
        /// </param>
        /// <param name="moduleFactory">
        /// Used to provide the tracking speech recognition syntax.
        /// </param>
        /// <param name="logger">Used to write logging information.</param>
        /// <param name="options">Provides Tracker preferences.</param>
        public Tracker(TrackerConfig config,
            IWorldAccessor worldAccessor,
            TrackerModuleFactory moduleFactory,
            ILogger<Tracker> logger,
            TrackerOptions options,
            RandomizerContext dbContext)
        {
            _moduleFactory = moduleFactory;
            _logger = logger;
            Options = options;
            _dbContext = dbContext;

            // Initialize the tracker configuration
            Items = config.Items.Select(x => x.Clone()).ToImmutableList();
            Pegs = config.Pegs.Select(x => x.Clone()).ToImmutableList();
            Dungeons = config.Dungeons.Select(x => x.Clone()).ToImmutableList();
            Responses = config.Responses;
            World = worldAccessor.GetWorld();
            GetTreasureCounts(Dungeons, World);
            UniqueLocationNames = World.Locations.ToDictionary(x => x.Id, x => GetUniqueNames(x));

            // Initalize the timers used to trigger idle responses
            _idleTimers = Responses.Idle.ToDictionary(
                x => x.Key,
                x => new Timer(IdleTimerElapsed, x.Key, Timeout.Infinite, Timeout.Infinite));

            // Initialize the text-to-speech
            _tts = new SpeechSynthesizer();
            _tts.SelectVoiceByHints(VoiceGender.Female);

            // Initialize the speech recognition engine
            _recognizer = new SpeechRecognitionEngine();
            _recognizer.SpeechRecognized += SpeechRecognized;
            InitializeMicrophone();
        }

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
        /// Gets a collection of trackable items.
        /// </summary>
        public IReadOnlyCollection<ItemData> Items { get; }

        /// <summary>
        /// Get a collection of pegs in Peg World mode.
        /// </summary>
        public IReadOnlyCollection<Peg> Pegs { get; }

        /// <summary>
        /// Gets a collection of Zelda dungeons.
        /// </summary>
        public IReadOnlyCollection<ZeldaDungeon> Dungeons { get; }

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
        /// Gets a dictionary that contains unique location names for each
        /// location.
        /// </summary>
        protected internal IReadOnlyDictionary<int, SchrodingersString> UniqueLocationNames { get; }

        /// <summary>
        /// Initializes the microphone from the default audio device
        /// </summary>
        /// <returns>True if the microphone is initialized, false otherwise</returns>
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
        public bool TrackDungeonTreasure(ZeldaDungeon dungeon, float? confidence = null, int amount = 1)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount of items must be greater than zero.");
            if (amount > dungeon.TreasureRemaining)
                _logger.LogWarning("Trying to track {amount} treasures in a dungeon with only {left} treasures left.", amount, dungeon.TreasureRemaining);

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
                    // If we don't have a response for the exact amount and we have
                    // multiple left, get the one for 2 (considered generic)
                    else if (dungeon.TreasureRemaining >= 2 && Responses.DungeonTreasureTracked.TryGetValue(2, out response))
                        Say(response.Format(dungeon.Name, dungeon.TreasureRemaining));
                }

                OnDungeonUpdated(new TrackerEventArgs(confidence));
                AddUndo(() => dungeon.TreasureRemaining++);
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
        public void SetDungeonReward(ZeldaDungeon dungeon, RewardItem? reward = null, float? confidence = null)
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
            var unmarkedDungeons = Dungeons
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
        public void SetDungeonRequirement(ZeldaDungeon dungeon, Medallion? medallion = null, float? confidence = null)
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
        public Progression GetProgression(bool assumeKeys = false)
        {
            var progression = new Progression();

            if (!World.Config.Keysanity || assumeKeys)
            {
                progression.Add(SMZ3.Item.CreateKeycards(World));
                if (assumeKeys)
                    progression.Add(SMZ3.Item.CreateDungeonPool(World));
            }

            foreach (var item in Items)
            {
                if (item.TrackingState > 0)
                    progression.Add(Enumerable.Repeat(new SMZ3.Item(item.InternalItemType), item.TrackingState));
            }
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
            Say(Responses.StartedTracking);
            RestartIdleTimers();
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
        /// The previous saved elapsed time
        /// </summary>
        public TimeSpan SavedElapsedTime { get; set; }

        /// <summary>
        /// The total elapsed time including the previously saved time
        /// </summary>
        public TimeSpan TotalElapsedTime => SavedElapsedTime + (DateTime.Now - (_startTime == DateTime.MinValue ? DateTime.Now : _startTime));

        /// <summary>
        /// Stops voice recognition.
        /// </summary>
        public virtual void StopTracking()
        {
            DisableVoiceRecognition();
            _tts.SpeakAsyncCancelAll();
            Say(Responses.StoppedTracking, wait: true);

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
            }
        }

        /// <summary>
        /// Speak a sentence using text-to-speech.
        /// </summary>
        /// <param name="text">The phrase to speak.</param>
        /// <param name="wait">
        /// <c>true</c> to wait until the text has been spoken completely or
        /// <c>false</c> to immediately return. The default is <c>false</c>.
        /// </param>
        public virtual void Say(string? text, bool wait = false)
        {
            if (text == null)
                return;

            var prompt = ParseText(text);
            if (wait)
                _tts.Speak(prompt);
            else
                _tts.SpeakAsync(prompt);

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
        /// <c>true</c> to attempt to clear a location for the tracked item;
        /// <c>false</c> if that is done by the caller.
        /// </param>
        public void TrackItem(ItemData item, string? trackedAs = null, float? confidence = null, bool tryClear = true)
        {
            var accessibleBefore = GetAccessibleLocations();
            var itemName = item.Name;
            var originalTrackingState = item.TrackingState;

            if (item.HasStages)
            {
                if (trackedAs != null && item.GetStage(trackedAs) != null)
                {
                    var stage = item.GetStage(trackedAs)!;

                    // Tracked by specific stage name (e.g. Tempered Sword), set
                    // to that stage specifically
                    var stageName = item.Stages[stage.Value].ToString();
                    if (item.Track(stage.Value))
                        Say(Responses.TrackedItemByStage.Format(itemName, stageName));
                    else
                        Say(Responses.TrackedOlderProgressiveItem?.Format(itemName, item.Stages[item.TrackingState].ToString()));
                }
                else
                {
                    // Tracked by regular name, upgrade by one step
                    if (item.Track())
                    {
                        var stageName = item.Stages[item.TrackingState].ToString();
                        Say(Responses.TrackedProgressiveItem.Format(itemName, stageName));
                    }
                    else
                    {
                        Say(Responses.TrackedTooManyOfAnItem);
                    }
                }
            }
            else if (item.Multiple)
            {
                item.Track();
                if (item.Counter == 1)
                    Say(Responses.TrackedItem.Format(itemName));
                else if (item.Counter > 1)
                    Say(Responses.TrackedItemMultiple.Format(item.Plural ?? $"{itemName}s", item.Counter));
                else
                {
                    _logger.LogWarning("Encountered multiple item with counter 0: {item} has counter {counter}", item, item.Counter);
                    Say(Responses.TrackedItem.Format(itemName));
                }
            }
            else
            {
                if (item.Track())
                {
                    if (Responses.TrackedSpecificItem.TryGetValue(item.Name[0], out var responses)
                        && responses.Count > 0)
                    {
                        Say(responses);
                    }
                    else
                    {
                        Say(Responses.TrackedItem.Format(itemName));
                    }
                }
                else
                {
                    Say(Responses.TrackedAlreadyTrackedItem?.Format(itemName));
                }
            }

            Action undoTrack = () => item.TrackingState = originalTrackingState;
            OnItemTracked(new ItemTrackedEventArgs(trackedAs, confidence));

            // Check if we can clear a location
            Action? undoClear = null;
            Action? undoTrackDungeonTreasure = null;
            if (tryClear)
            {
                var location = World.Locations.TrySingle(x => x.Cleared == false && x.ItemIs(item.InternalItemType, World));
                if (location != null)
                {
                    location.Cleared = true;
                    OnLocationCleared(new(location, confidence));

                    undoClear = () => location.Cleared = false;
                    if (MarkedLocations.ContainsKey(location.Id))
                    {
                        MarkedLocations.Remove(location.Id);
                        OnMarkedLocationsUpdated(new TrackerEventArgs(confidence));
                    }

                    // If this item was in a dungeon, also track treasure count
                    undoTrackDungeonTreasure = TryTrackDungeonTreasure(item, confidence);
                }
            }

            IsDirty = true;

            AddUndo(() =>
            {
                undoTrack();
                undoClear?.Invoke();
                undoTrackDungeonTreasure?.Invoke();
            });
            GiveLocationHint(accessibleBefore);
            RestartIdleTimers();
        }

        /// <summary>
        /// Removes an item from the tracker.
        /// </summary>
        /// <param name="item">The item to untrack.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void UntrackItem(ItemData item, float? confidence = null)
        {
            var originalTrackingState = item.TrackingState;

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
            AddUndo(() => item.TrackingState = originalTrackingState);
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
        public void TrackItem(ItemData item, ZeldaDungeon dungeon, string? trackedAs = null, float? confidence = null)
        {
            TrackItem(item, trackedAs, confidence, tryClear: false);
            var undoTrack = _undoHistory.Pop();

            // Check if we can remove something from the remaining treasures in
            // a dungeon
            Action? undoTrackTreasure = null;
            dungeon = GetDungeonFromItem(item, dungeon)!;
            if (TrackDungeonTreasure(dungeon, confidence))
                undoTrackTreasure = _undoHistory.Pop();

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
                });
            }
            else
            {
                AddUndo(() =>
                {
                    undoTrack();
                    undoTrackTreasure?.Invoke();
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
        public void TrackItem(ItemData item, Location location, string? trackedAs = null, float? confidence = null)
        {
            SassIfItemIsWrong(item, location, confidence);
            TrackItem(item, trackedAs, confidence, tryClear: false);
            Clear(location);

            IsDirty = true;

            var undoClear = _undoHistory.Pop();
            var undoTrack = _undoHistory.Pop();
            AddUndo(() =>
            {
                undoClear();
                undoTrack();
            });
        }

        /// <summary>
        /// Sets the item count for the specified item.
        /// </summary>
        /// <param name="item">The item to track.</param>
        /// <param name="count">The amount of the item that is in the player's inventory now.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void SetItemCount(ItemData item, int count, float confidence)
        {
            var newItemCount = count;
            if (item.CounterMultiplier > 1
                && count % item.CounterMultiplier == 0)
            {
                newItemCount = count / item.CounterMultiplier.Value;
            }

            var oldItemCount = item.TrackingState;
            item.TrackingState = newItemCount;
            if (newItemCount > oldItemCount)
            {
                Say(Responses.TrackedItemMultiple.Format(item.Plural ?? $"{item.Name}s", item.Counter));
            }
            else
            {
                Say(Responses.UntrackedItemMultiple.Format(item.Plural ?? $"{item.Name}s", item.Plural ?? $"{item.Name}s"));
            }

            IsDirty = true;

            AddUndo(() => item.TrackingState = oldItemCount);
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
        /// <param name="assumeKeys">Set to true to ignore keys when clearing the location.</param>
        public void ClearArea(IHasLocations area, bool trackItems, bool includeUnavailable = false, float? confidence = null, bool assumeKeys = false)
        {
            var dungeon = Dungeons.SingleOrDefault(x => area is Region region && x.Is(region));
            if (dungeon != null)
            {
                assumeKeys = true; // Always assume keys when clearing the dungeon itself
                dungeon.Cleared = true;
                OnDungeonUpdated(new(confidence));
            }

            var locations = area.Locations
                .Where(x => !x.Cleared)
                .WhereUnless(includeUnavailable, x => x.IsAvailable(GetProgression(assumeKeys)))
                .ToImmutableList();

            if (locations.Count == 0)
            {
                Say(Responses.TrackedNothing.Format(area.Name));
                return;
            }

            // If there is only one (available) item here, just call the regular
            // TrackItem instead
            var onlyLocation = locations.TrySingle();
            if (onlyLocation != null)
            {
                if (!trackItems)
                {
                    Clear(onlyLocation, confidence);
                    return;
                }

                var item = Items.SingleOrDefault(x => x.InternalItemType == onlyLocation.Item.Type);
                if (item == null)
                {
                    // Probably just the compass or something. Clear the
                    // location still, even if we can't track the item.
                    Clear(onlyLocation, confidence);
                    return;
                }

                TrackItem(item, onlyLocation, confidence: confidence);
                return;
            }

            // Otherwise, start counting
            var itemsTracked = 0;
            var treasureTracked = 0;
            foreach (var location in locations)
            {
                if (!trackItems)
                {
                    itemsTracked++;
                    if (location.Item != null && !location.Item.IsDungeonItem)
                        treasureTracked++;
                    location.Cleared = true;
                    OnLocationCleared(new(location, confidence));
                    continue;
                }

                var itemType = location.Item?.Type;
                var item = Items.SingleOrDefault(x => x.InternalItemType == itemType);
                if (item == null || !item.Track())
                    _logger.LogWarning("Failed to track {itemType} in {area}.", itemType, area.Name); // Probably the compass or something, who cares
                itemsTracked++;
                if (location.Item != null && !location.Item.IsDungeonItem)
                    treasureTracked++;

                location.Cleared = true;
            }

            // TODO: Include the most noteworthy item (by item value, once added
            // to data), e.g. "Tracked 5 items in Mini Moldorm Cave, including
            // the Morph Ball"
            var responses = trackItems ? Responses.TrackedMultipleItems : Responses.ClearedMultipleItems;
            Say(responses.Format(itemsTracked, area.GetName()));

            Action? undoTrackDungeon = null;
            if (dungeon != null && treasureTracked > 0)
            {
                TrackDungeonTreasure(dungeon, amount: treasureTracked);
                undoTrackDungeon = _undoHistory.Pop();
            }

            IsDirty = true;

            AddUndo(() =>
            {
                if (dungeon != null)
                    dungeon.Cleared = false;

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
                undoTrackDungeon?.Invoke();
            });
            OnItemTracked(new ItemTrackedEventArgs(null, confidence));
        }

        /// <summary>
        /// Clears an item from the specified location.
        /// </summary>
        /// <param name="location">The location to clear.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void Clear(Location location, float? confidence = null)
        {
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
            var dungeon = Dungeons.SingleOrDefault(x => x.Is(location.Region));
            if (dungeon != null && location.Item?.IsDungeonItem == false)
            {
                TrackDungeonTreasure(dungeon, confidence);
                undoTrackTreasure = _undoHistory.Pop();
            }

            Action? undoStopPegWorldMode = null;
            if (location == World.DarkWorldNorthWest.PegWorld)
            {
                StopPegWorldMode();
                undoStopPegWorldMode = _undoHistory.Pop();
            }

            IsDirty = true;

            AddUndo(() =>
            {
                location.Cleared = false;
                undoTrackTreasure?.Invoke();
                undoStopPegWorldMode?.Invoke();
            });
            OnLocationCleared(new(location, confidence));
        }

        /// <summary>
        /// Marks a dungeon as cleared and, if possible, tracks the boss reward.
        /// </summary>
        /// <param name="dungeon">The dungeon that was cleared.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void MarkDungeonAsCleared(ZeldaDungeon dungeon, float? confidence = null)
        {
            if (dungeon.Cleared)
            {
                Say(Responses.DungeonAlreadyCleared.Format(dungeon.Name, dungeon.Boss));
                return;
            }

            dungeon.Cleared = true;
            Say(Responses.DungeonCleared.Format(dungeon.Name, dungeon.Boss));

            // Try to track the associated boss reward item
            Action? undoTrack = null;
            if (dungeon.LocationId != null)
            {
                var rewardLocation = World.Locations.Single(x => x.Id == dungeon.LocationId);
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

            IsDirty = true;

            OnDungeonUpdated(new TrackerEventArgs(confidence));
            AddUndo(() =>
            {
                dungeon.Cleared = false;
                undoTrack?.Invoke();
            });
        }

        /// <summary>
        /// Un-marks a dungeon as cleared and, if possible, untracks the boss reward.
        /// </summary>
        /// <param name="dungeon">The dungeon that should be un-cleared.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public void MarkDungeonAsIncomplete(ZeldaDungeon dungeon, float? confidence = null)
        {
            if (!dungeon.Cleared)
            {
                Say(Responses.DungeonNotYetCleared.Format(dungeon.Name, dungeon.Boss));
                return;
            }

            dungeon.Cleared = false;
            Say(Responses.DungeonUncleared.Format(dungeon.Name, dungeon.Boss));

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
            SassIfItemIsWrong(item, location, confidence);

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
        /// Returns the possible names of the specified location.
        /// </summary>
        /// <param name="location">The location whose names to get.</param>
        /// <returns>
        /// A new <see cref="SchrodingersString"/> object representing the
        /// possible names of <paramref name="location"/>.
        /// </returns>
        protected internal virtual SchrodingersString GetName(Location location)
            => UniqueLocationNames[location.Id];

        /// <summary>
        /// Adds an action to be invoked to undo the last operation.
        /// </summary>
        /// <param name="undo">
        /// The action to invoke to undo the last operation.
        /// </param>
        protected virtual void AddUndo(Action undo) => _undoHistory.Push(undo);

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

        private Action? TryTrackDungeonTreasure(ItemData item, float? confidence)
        {
            if (confidence < Options.MinimumSassConfidence)
            {
                // Tracker response could give away the location of an item if
                // it is in a dungeon but tracker misheard.
                return null;
            }

            var dungeon = GetDungeonFromItem(item);
            if (dungeon != null && !item.InternalItemType.IsInAnyCategory(ItemCategory.BigKey, ItemCategory.SmallKey, ItemCategory.Map, ItemCategory.Compass))
            {
                if (TrackDungeonTreasure(dungeon, confidence))
                    return _undoHistory.Pop();
            }

            IsDirty = true;

            return null;
        }

        private void GetTreasureCounts(IReadOnlyCollection<ZeldaDungeon> dungeons, World world)
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

        private void SassIfItemIsWrong(ItemData item, Location location, float? confidence)
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
        }

        private ZeldaDungeon? GetDungeonFromItem(ItemData item, ZeldaDungeon? dungeon = null)
        {
            var locations = World.Locations
                .Where(x => !x.Cleared && x.ItemIs(item.InternalItemType, World))
                .ToImmutableList();

            if (locations.Count == 1 && dungeon == null)
            {
                // User didn't have a guess and there's only one location that
                // has the tracker item
                return Dungeons.SingleOrDefault(x => x.Is(locations[0].Region));
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

        private void RestartIdleTimers()
        {
            foreach (var item in _idleTimers)
            {
                var timeout = Parse.AsTimeSpan(item.Key, s_random) ?? Timeout.InfiniteTimeSpan;
                var timer = item.Value;

                timer.Change(timeout, Timeout.InfiniteTimeSpan);
            }
        }

        private void IdleTimerElapsed(object? state)
        {
            var key = (string)state!;
            Say(Responses.Idle[key]);
        }

        private void SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            RestartIdleTimers();
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

        private SchrodingersString GetUniqueNames(Location location)
        {
            var allLocationNames = World.Locations.Select(x => x.Name)
                .Concat(World.Locations.SelectMany(x => x.AlternateNames));

            return new SchrodingersString(location.AlternateNames.Concat(new[] { location.Name })
                .SelectMany(x => MakeUnique(x, location)));

            IEnumerable<SchrodingersString.Possibility> MakeUnique(string name, Location location)
            {
                // Only at the location name if it is unique
                var isUnique = Occurences(name) < 2;
                if (isUnique)
                {
                    yield return new(name);
                }

                // Add the room/region name variants. If the name is unique, add
                // them with a zero weight for consistent tracking, without
                // having tracker say verbose names.
                if (location.Room != null)
                {
                    foreach (var roomName in location.Room.AlsoKnownAs.Concat(new[] { location.Room.Name }))
                    {
                        yield return new($"{roomName} {name}", isUnique ? 0 : 1);
                    }
                }
                else
                {
                    foreach (var regionName in location.Region.AlsoKnownAs.Concat(new[] { location.Region.Name }))
                    {
                        yield return new($"{regionName} {name}", isUnique ? 0 : 1);
                    }
                }
            }

            int Occurences(string name)
                => allLocationNames!.Count(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsDirty { get; set; }

    }


}
