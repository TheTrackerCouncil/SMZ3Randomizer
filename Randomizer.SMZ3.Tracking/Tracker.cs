using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;

using BunLabs;

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Tracking.Vocabulary;
using Randomizer.SMZ3.Tracking.VoiceCommands;

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
        private Dictionary<string, Timer> _idleTimers;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="trackerConfigProvider">
        /// Used to provide the tracking configuration.
        /// </param>
        /// <param name="world">The generated world to track in.</param>
        public Tracker(TrackerConfigProvider trackerConfigProvider, World? world)
        {
            var config = trackerConfigProvider.GetTrackerConfig();
            Items = config.Items.ToImmutableList();
            Pegs = config.Pegs.ToImmutableList();
            Dungeons = config.Dungeons.ToImmutableList();
            Responses = config.Responses;
            World = world ?? new World(new Config(), "Player", 0, "");

            _idleTimers = Responses.Idle.ToDictionary(
                x => x.Key,
                x => new Timer(IdleTimerElapsed, x.Key, Timeout.Infinite, Timeout.Infinite));

            _tts = new SpeechSynthesizer();
            _tts.SelectVoiceByHints(VoiceGender.Female);

            _recognizer = new SpeechRecognitionEngine();
            _recognizer.SpeechRecognized += SpeechRecognized;
            _recognizer.SetInputToDefaultAudioDevice();

            var moduleFactory = new TrackerModuleFactory();
            moduleFactory.LoadAll(this, _recognizer);
        }

        public event EventHandler<ItemTrackedEventArgs>? ItemTracked;

        public event EventHandler<TrackerEventArgs>? ToggledPegWorldModeOn;

        public event EventHandler<TrackerEventArgs>? PegPegged;

        public event EventHandler<TrackerEventArgs>? DungeonUpdated;

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
        public IReadOnlyCollection<ZeldaDungeon> Dungeons { get; init; }

        /// <summary>
        /// Gets the configured responses.
        /// </summary>
        public ResponseConfig Responses { get; }

        /// <summary>
        /// Gets the world for the currently tracked playthrough.
        /// </summary>
        public World World { get; }

        public void ClearDungeon(ZeldaDungeon dungeon, float confidence = 1.0f)
        {
            dungeon.Cleared = true;
            Say(Responses.DungeonCleared.Format(dungeon.Name, dungeon.Boss));
            OnDungeonUpdated(new TrackerEventArgs(confidence));
        }

        public void SetDungeonReward(ZeldaDungeon dungeon, RewardItem? reward = null, float confidence = 1.0f)
        {
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
        }

        public void SetDungeonRequirement(ZeldaDungeon dungeon, Medallion? medallion = null, float confidence = 1.0f)
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
        /// Starts voice recognition.
        /// </summary>
        public virtual void StartTracking()
        {
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            Say(Responses.StartedTracking);
            RestartIdleTimers();
        }

        /// <summary>
        /// Stops voice recognition.
        /// </summary>
        public virtual void StopTracking()
        {
            _recognizer.RecognizeAsyncStop();
            Say(Responses.StoppedTracking, wait: true);

            foreach (var timer in _idleTimers.Values)
                timer.Change(Timeout.Infinite, Timeout.Infinite);
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
        /// <param name="confidence">
        /// The confidence when triggered by voice command.
        /// </param>
        public void TrackItem(ItemData item, string? trackedAs = null, float confidence = 1.0f)
        {
            var accessibleBefore = GetAccessibleLocations();
            var itemName = item.Name;

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
                Say(Responses.TrackedItemMultiple.Format(itemName));
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

            OnItemTracked(new ItemTrackedEventArgs(item, trackedAs, confidence));
            GiveLocationHint(accessibleBefore);
            RestartIdleTimers();
        }

        /// <summary>
        /// Pegs a Peg World peg.
        /// </summary>
        /// <param name="peg">The peg to peg.</param>
        /// <param name="confidence">
        /// The confidence when triggered by voice command.
        /// </param>
        public void Peg(Peg peg, float confidence = 1.0f)
        {
            peg.Pegged = true;

            if (Pegs.Any(x => !x.Pegged))
                Say(Responses.PegWorldModePegged);
            else
                Say(Responses.PegWorldModeDone);
            OnPegPegged(new TrackerEventArgs(confidence));

            RestartIdleTimers();
        }

        public void StartPegWorldMode(float confidence = 1.0f)
        {
            Say(Responses.PegWorldModeOn, wait: true);
            OnPegWorldModeToggled(new TrackerEventArgs(confidence));
        }

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

        protected virtual void OnItemTracked(ItemTrackedEventArgs e)
            => ItemTracked?.Invoke(this, e);

        protected virtual void OnPegWorldModeToggled(TrackerEventArgs e)
            => ToggledPegWorldModeOn?.Invoke(this, e);

        protected virtual void OnPegPegged(TrackerEventArgs e)
            => PegPegged?.Invoke(this, e);

        protected virtual void OnDungeonUpdated(TrackerEventArgs e)
            => DungeonUpdated?.Invoke(this, e);

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
            else
            {
                var rnd = s_random.Next();
                if (rnd % 20 == 0)
                    Say("Doesn't get you anywhere though.");
            }
        }

        private IEnumerable<Location> GetAccessibleLocations()
        {
            if (World == null)
                return Enumerable.Empty<Location>();

            var items = new List<Item>();
            foreach (var item in Items)
            {
                for (var i = 0; i < item.TrackingState; i++)
                    items.Add(new Item(item.InternalItemType));
            }

            var progression = new Progression(items);
            return World.Locations.Where(x => x.IsAvailable(progression)).ToList();
        }
    }
}
