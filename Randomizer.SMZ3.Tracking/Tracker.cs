using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Speech.Recognition;
using System.Speech.Synthesis;

using Microsoft.EntityFrameworkCore.Metadata.Conventions;

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
        private readonly Action<string> _log;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="log">Invoked for debug output.</param>
        /// <param name="trackerConfigProvider">
        /// Used to provide the tracking configuration.
        /// </param>
        /// <param name="world">
        /// The generated world to track in.
        /// </param>
        public Tracker(Action<string> log, TrackerConfigProvider trackerConfigProvider, World? world)
        {
            _log = log;
            _tts = new SpeechSynthesizer();
            _tts.SelectVoiceByHints(VoiceGender.Female);

            var config = trackerConfigProvider.GetTrackerConfig();
            Items = config.Items.ToImmutableList();
            World = world;

            var trackItemsCommand = new TrackItemsCommand(this);
            trackItemsCommand.ItemTracked += ItemTracked;

            _recognizer = new SpeechRecognitionEngine();
            _recognizer.LoadGrammar(trackItemsCommand.BuildGrammar());
            _recognizer.SetInputToDefaultAudioDevice();
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        public IReadOnlyCollection<ItemData> Items { get; }

        /// <summary>
        /// Gets the world for the currently tracked playthrough.
        /// </summary>
        public World? World { get; }

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
            return Items.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? Items.SingleOrDefault(x => x.OtherNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                ?? Items.Where(x => x.Stages != null)
                        .SingleOrDefault(x => x.Stages!.SelectMany(stage => stage.Value).Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Starts voice recognition.
        /// </summary>
        public virtual void StartTracking()
        {
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            Say("Is it weekend already?");
        }

        /// <summary>
        /// Stops voice recognition.
        /// </summary>
        public virtual void StopTracking()
        {
            _recognizer.RecognizeAsyncStop();
            Say("Leaving so soon?", wait: true);
        }

        /// <summary>
        /// Speak a sentence using text-to-speech.
        /// </summary>
        /// <param name="text">The phrase to speak.</param>
        /// <param name="wait">
        /// <c>true</c> to wait until the text has been spoken completely or
        /// <c>false</c> to immediately return. The default is <c>false</c>.
        /// </param>
        public virtual void Say(string text, bool wait = false)
        {
            if (wait)
                _tts.Speak(text);
            else
                _tts.SpeakAsync(text);
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
                }

                _disposed = true;
            }
        }

        private void ItemTracked(object? sender, ItemTrackedEventArgs e)
        {
            _log($"Recognized item {e.Item.Name} as {e.TrackedAs} with {e.Confidence:P2} confidence.");

            var accessibleBefore = GetAccessibleLocations();
            var randomName = e.Item.GetRandomName(s_random);

            if (e.Item.HasStages)
            {
                // TODO: Get current stage
                var stage = e.Item.GetStage(e.TrackedAs);
                if (stage != null)
                {
                    // Tracked by specific stage name (e.g. Tempered Sword), set
                    // to that stage specifically
                    var stageName = e.Item.Stages[stage.Value].Random(s_random);
                    e.Item.Track(stage.Value);
                    Say($"Marked {randomName} as {stageName}");
                }
                else
                {
                    // Tracked by regular name, upgrade by one step
                    if (e.Item.Track())
                    {
                        // Say($"Upgraded {randomName} by one step.");
                        var stageName = e.Item.Stages[e.Item.TrackingState].Random(s_random);
                        Say($"Congratulations with your new {stageName}.");
                    }
                    else
                    {
                        Say("I doubt that.");
                    }
                }
            }
            else if (e.Item.Multiple)
            {
                e.Item.Track();
                Say($"Added {randomName}.");
            }
            else
            {
                if (e.Item.Track())
                    Say($"Toggled {e.Item.Name} on.");
                else
                    Say("You already have one.");
            }

            if (World != null)
            {
                var accessibleAfter = GetAccessibleLocations();
                var newlyAccessible = accessibleAfter.Except(accessibleBefore);
                if (newlyAccessible.Any())
                {
                    if (newlyAccessible.Count() == 1)
                        Say($"{newlyAccessible.Single().Name} is now open to you. Have fun.");

                    var regions = newlyAccessible.GroupBy(x => x.Region).OrderByDescending(x => x.Count());
                    if (regions.Count() == 1)
                        Say($"Time to visit {regions.Single().Key.Name} maybe?");
                    if (regions.Count() > 2)
                    {
                        Say($"You now have access to {regions.First().Key.Name} and {regions.Skip(1).First().Key.Name}.");
                        if (regions.Count() == 2)
                            Say("Isn't that exciting?");
                        else
                            Say("Others too but I have better things to do.");
                    }
                }
                else
                {
                    Say("Doesn't get you anywhere, though.");
                }
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
