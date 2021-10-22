using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides Tracker with speech recognition commands.
    /// </summary>
    public abstract class TrackerModule
    {
        /// <summary>
        /// Gets the semantic result key used to identify the name of a dungeon.
        /// </summary>
        protected const string DungeonKey = "DungeonName";

        /// <summary>
        /// Gets the semantic result key used to identify the name of an item.
        /// </summary>
        protected const string ItemNameKey = "ItemName";

        /// <summary>
        /// Gets the semantic result key used to identify the name of a
        /// location.
        /// </summary>
        protected const string LocationKey = "LocationName";

        /// <summary>
        /// Gets the semantic result key used to identify the name of a room.
        /// </summary>
        protected const string RoomKey = "RoomName";

        /// <summary>
        /// Gets the semantic result key used to identify the name of a region.
        /// </summary>
        protected const string RegionKey = "RegionName";

        private readonly Dictionary<string, IEnumerable<string>> _syntax = new();
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerModule"/> class
        /// with the specified tracker.
        /// </summary>
        /// <param name="tracker">The tracker instance to use.</param>
        /// <param name="logger">Used to log information.</param>
        protected TrackerModule(Tracker tracker, ILogger logger)
        {
            Tracker = tracker;
            _logger = logger;
        }

        /// <summary>
        /// Gets a dictionary that contains the rule names and their associated
        /// speech recognition patterns.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<string>> Syntax
            => _syntax.ToImmutableDictionary();

        /// <summary>
        /// Gets the Tracker instance.
        /// </summary>
        protected Tracker Tracker { get; }

        /// <summary>
        /// Gets a list of speech recognition grammars provided by the module.
        /// </summary>
        protected IList<Grammar> Grammars { get; }
            = new List<Grammar>();

        /// <summary>
        /// Loads the voice commands provided by the module into the specified
        /// recognition engine.
        /// </summary>
        /// <param name="engine">
        /// The speech recognition engine to initialize.
        /// </param>
        public void LoadInto(SpeechRecognitionEngine engine)
        {
            foreach (var grammar in Grammars)
                engine.LoadGrammar(grammar);
        }

        /// <summary>
        /// Returns the <see cref="ZeldaDungeon"/> that was detected in a voice
        /// command using <see cref="DungeonKey"/>.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <returns>
        /// A <see cref="ZeldaDungeon"/> from the recognition result.
        /// </returns>
        protected static ZeldaDungeon GetDungeonFromResult(Tracker tracker, RecognitionResult result)
        {
            var dungeonName = (string)result.Semantics[DungeonKey].Value;
            var dungeon = tracker.Dungeons.SingleOrDefault(x => x.Name.Contains(dungeonName, StringComparison.OrdinalIgnoreCase));
            return dungeon ?? throw new Exception($"Could not find recognized dungeon '{dungeonName}'.");
        }

        /// <summary>
        /// Returns the <see cref="ItemData"/> that was detected in a voice
        /// command using <see cref="ItemNameKey"/>.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <param name="itemName">
        /// The actual name of the item that was detected.
        /// </param>
        /// <returns>
        /// An <see cref="ItemData"/> from the recognition result.
        /// </returns>
        protected static ItemData GetItemFromResult(Tracker tracker, RecognitionResult result, out string itemName)
        {
            itemName = (string)result.Semantics[ItemNameKey].Value;
            var itemData = tracker.FindItemByName(itemName);

            return itemData ?? throw new Exception($"Could not find recognized item '{itemName}' (\"{result.Text}\")");
        }

        /// <summary>
        /// Returns the <see cref="Location"/> that was detected in a voice
        /// command using <see cref="LocationKey"/>.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <returns>
        /// A <see cref="Location"/> from the recognition result.
        /// </returns>
        protected static Location GetLocationFromResult(Tracker tracker, RecognitionResult result)
        {
            var id = (int)result.Semantics[LocationKey].Value;
            var location = tracker.World.Locations.SingleOrDefault(x => x.Id == id);
            return location ?? throw new Exception($"Could not find a location with ID {id} (\"{result.Text}\")");
        }

        /// <summary>
        /// Returns the <see cref="Room"/> that was detected in a voice command
        /// using <see cref="RoomKey"/>.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <returns>A <see cref="Room"/> from the recognition result.</returns>
        protected static Room GetRoomFromResult(Tracker tracker, RecognitionResult result)
        {
            var name = (string)result.Semantics[RoomKey].Value;
            var room = tracker.World.Rooms.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return room ?? throw new Exception($"Could not find a room with name '{name}' ('{result.Text}').");
        }

        /// <summary>
        /// Returns the <see cref="Region"/> that was detected in a voice
        /// command using <see cref="RegionKey"/>.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <returns>
        /// A <see cref="Region"/> from the recognition result.
        /// </returns>
        protected static Region GetRegionFromResult(Tracker tracker, RecognitionResult result)
        {
            var name = (string)result.Semantics[RegionKey].Value;
            var region = tracker.World.Regions.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return region ?? throw new Exception($"Could not find a region with name '{name}' ('{result.Text}').");
        }

        /// <summary>
        /// Adds a new voice command that matches the specified phrase.
        /// </summary>
        /// <param name="ruleName">The name of the command.</param>
        /// <param name="phrase">The phrase to recognize.</param>
        /// <param name="executeCommand">
        /// The command to execute when the phrase is recognized.
        /// </param>
        protected void AddCommand(string ruleName, string phrase,
            Action<Tracker, RecognitionResult> executeCommand)
        {
            var builder = new GrammarBuilder()
                .Append(phrase);

            _syntax[ruleName] = new[] { phrase };
            AddCommand(ruleName, builder, executeCommand);
        }

        /// <summary>
        /// Adds a new voice command that matches any of the specified phrases.
        /// </summary>
        /// <param name="ruleName">The name of the command.</param>
        /// <param name="phrases">The phrases to recognize.</param>
        /// <param name="executeCommand">
        /// The command to execute when any of the phrases is recognized.
        /// </param>
        protected void AddCommand(string ruleName, string[] phrases,
            Action<Tracker, RecognitionResult> executeCommand)
        {
            var builder = new GrammarBuilder()
                .OneOf(phrases);

            _syntax[ruleName] = phrases;
            AddCommand(ruleName, builder, executeCommand);
        }

        /// <summary>
        /// Adds a new voice command that matches the specified grammar.
        /// </summary>
        /// <param name="ruleName">The name of the command.</param>
        /// <param name="grammarBuilder">The grammar to recognize.</param>
        /// <param name="executeCommand">
        /// The command to execute when speech matching the grammar is
        /// recognized.
        /// </param>
        protected void AddCommand(string ruleName, GrammarBuilder grammarBuilder,
            Action<Tracker, RecognitionResult> executeCommand)
        {
            _syntax.TryAdd(ruleName, grammarBuilder.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            try
            {
                var grammar = grammarBuilder.Build(ruleName);
                grammar.SpeechRecognized += (sender, e) =>
                {
                    try
                    {
                        if (e.Result.Confidence < Tracker.Options.MinimumConfidence)
                        {
                            _logger.LogWarning("Confidence level too low ({Confidence} < {Threshold}) in voice command: \"{Text}\".",
                                e.Result.Confidence, Tracker.Options.MinimumConfidence, e.Result.Text);
                            Tracker.Say(Tracker.Responses.Misheard);
                            return;
                        }

                        executeCommand(Tracker, e.Result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while executing recognized command '{command}': \"{text}\".", ruleName, e.Result.Text);
                        Tracker.Error();
                    }
                };
                Grammars.Add(grammar);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred while constructing the speech recognition grammar for command '{command}'.", ruleName);
                throw new GrammarException($"An error occurred while constructing the speech recognition grammar for command '{ruleName}'.", ex);
            }
        }

        /// <summary>
        /// Gets the item names for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible item
        /// names.
        /// </returns>
        protected virtual Choices GetItemNames()
        {
            var itemNames = new Choices();
            foreach (var itemData in Tracker.Items)
            {
                foreach (var name in itemData.Name)
                    itemNames.Add(new SemanticResultValue(name.ToString(), name.ToString()));

                if (itemData.Stages != null)
                {
                    foreach (var stageName in itemData.Stages.SelectMany(x => x.Value))
                    {
                        itemNames.Add(new SemanticResultValue(stageName.ToString(), stageName.ToString()));
                    }
                }
            }

            return itemNames;
        }

        /// <summary>
        /// Gets the dungeon names for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible dungeon
        /// names.
        /// </returns>
        protected virtual Choices GetDungeonNames()
        {
            var dungeonNames = new Choices();
            foreach (var dungeon in Tracker.Dungeons)
            {
                foreach (var name in dungeon.Name)
                    dungeonNames.Add(new SemanticResultValue(name.Text, name.Text));
            }

            return dungeonNames;
        }

        /// <summary>
        /// Gets the location names for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible
        /// location names mapped to their IDs.
        /// </returns>
        protected virtual Choices GetLocationNames()
        {
            var locationNames = new Choices();

            foreach (var location in Tracker.World.Locations)
            {
                foreach (var name in Tracker.UniqueLocationNames[location])
                    locationNames.Add(new SemanticResultValue(name.Text, location.Id));
            }

            return locationNames;
        }

        /// <summary>
        /// Gets the room names for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible room
        /// names mapped to the primary room name.
        /// </returns>
        protected virtual Choices GetRoomNames()
        {
            var roomNames = new Choices();

            foreach (var room in Tracker.World.Rooms)
            {
                roomNames.Add(new SemanticResultValue(room.Name, room.Name));
                foreach (var name in room.AlsoKnownAs)
                    roomNames.Add(new SemanticResultValue(name, room.Name));
            }

            return roomNames;
        }

        /// <summary>
        /// Gets the region names for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible region
        /// names mapped to the primary region name.
        /// </returns>
        protected virtual Choices GetRegionNames()
        {
            var regionNames = new Choices();

            foreach (var region in Tracker.World.Regions)
            {
                regionNames.Add(new SemanticResultValue(region.Name, region.Name));
                foreach (var name in region.AlsoKnownAs)
                    regionNames.Add(new SemanticResultValue(name, region.Name));
            }

            return regionNames;
        }
    }
}
