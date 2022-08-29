using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;

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
        /// Gets the semantic result key used to identify the name of a boss.
        /// </summary>
        protected const string BossKey = "BossName";

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

        /// <summary>
        /// Gets the semantic result key used to identify the name of a map.
        /// </summary>
        protected const string MapKey = "Map";

        private readonly Dictionary<string, IEnumerable<string>> _syntax = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerModule"/> class
        /// with the specified tracker.
        /// </summary>
        /// <param name="tracker">The tracker instance to use.</param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="logger">Used to log information.</param>
        protected TrackerModule(Tracker tracker, IItemService itemService, ILogger logger)
        {
            Tracker = tracker;
            ItemService = itemService;
            Logger = logger;
        }

        /// <summary>
        /// Gets a dictionary that contains the rule names and their associated
        /// speech recognition patterns.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<string>> Syntax
            => _syntax.ToImmutableDictionary();

        /// <summary>
        /// Gets a value indicating whether the voice recognition syntax is
        /// secret and should not be displayed to the user.
        /// </summary>
        public virtual bool IsSecret => false;

        /// <summary>
        /// Gets the Tracker instance.
        /// </summary>
        protected Tracker Tracker { get; }

        protected IItemService ItemService { get; }

        /// <summary>
        /// Gets a list of speech recognition grammars provided by the module.
        /// </summary>
        protected IList<Grammar> Grammars { get; }
            = new List<Grammar>();

        /// <summary>
        /// Gets a logger for the current module.
        /// </summary>
        protected ILogger Logger { get; }

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
        /// Returns the <see cref="DungeonInfo"/> that was detected in a voice
        /// command using <see cref="DungeonKey"/>.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <returns>
        /// A <see cref="DungeonInfo"/> from the recognition result.
        /// </returns>
        protected static DungeonInfo GetDungeonFromResult(Tracker tracker, RecognitionResult result)
        {
            var dungeonTypeName = (string)result.Semantics[DungeonKey].Value;
            var dungeon = tracker.WorldInfo.Dungeon(dungeonTypeName);
            return dungeon ?? throw new Exception($"Could not find dungeon {dungeonTypeName} (\"{result.Text}\").");
        }

        /// <summary>
        /// Returns the <see cref="DungeonInfo"/> that was detected in a voice
        /// command using <see cref="BossKey"/>.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <returns>
        /// A <see cref="DungeonInfo"/> from the recognition result.
        /// </returns>
        protected static DungeonInfo? GetBossDungeonFromResult(Tracker tracker, RecognitionResult result)
        {
            var dungeonTypeName = (string)result.Semantics[BossKey].Value;
            return tracker.WorldInfo.Dungeon(dungeonTypeName);
        }

        /// <summary>
        /// Returns the <see cref="BossInfo"/> that was detected in a voice
        /// command using <see cref="BossKey"/>.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <returns>
        /// A <see cref="BossInfo"/> from the recognition result.
        /// </returns>
        protected static BossInfo? GetBossFromResult(Tracker tracker, RecognitionResult result)
        {
            var bossName = (string)result.Semantics[BossKey].Value;
            return tracker.WorldInfo.Bosses.SingleOrDefault(x => x.Name.Contains(bossName, StringComparison.Ordinal));
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
        protected ItemData GetItemFromResult(Tracker tracker, RecognitionResult result, out string itemName)
        {
            itemName = (string)result.Semantics[ItemNameKey].Value;
            var itemData = ItemService.FindOrDefault(itemName);

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
            var location = tracker.WorldInfo.Location(id).GetLocation(tracker.World);
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
            var roomTypeName = (string)result.Semantics[RoomKey].Value;
            var room = tracker.WorldInfo.Room(roomTypeName).GetRoom(tracker.World);
            return room ?? throw new Exception($"Could not find room {roomTypeName} (\"{result.Text}\").");
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
            var regionTypeName = (string)result.Semantics[RegionKey].Value;
            var region = tracker.WorldInfo.Region(regionTypeName).GetRegion(tracker.World);
            return region ?? throw new Exception($"Could not find region {regionTypeName} (\"{result.Text}\").");
        }

        /// <summary>
        /// Returns the region or room that was detected in a voice
        /// command.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="result">The speech recognition result.</param>
        /// <returns>
        /// The recognized area from the recognition result.
        /// </returns>
        protected static IHasLocations GetAreaFromResult(Tracker tracker, RecognitionResult result)
        {
            if (result.Semantics.ContainsKey(RegionKey))
                return GetRegionFromResult(tracker, result);
            else if (result.Semantics.ContainsKey(RoomKey))
                return GetRoomFromResult(tracker, result);
            else
                throw new InvalidOperationException("Could not find a region or room in the recognized voice command.");
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
                        var minimumConfidence = Math.Max(Tracker.Options.MinimumRecognitionConfidence, Tracker.Options.MinimumExecutionConfidence);
                        if (e.Result.Confidence >= minimumConfidence)
                        {
                            Logger.LogInformation("Recognized \"{text}\" with {confidence:P2} confidence.",
                                e.Result.Text, e.Result.Confidence);
                            Tracker.RestartIdleTimers();

                            executeCommand(Tracker, e.Result);
                        }
                        else
                        {
                            Logger.LogWarning("Confidence level too low ({Confidence} < {Threshold}) in voice command: \"{Text}\".",
                                e.Result.Confidence, minimumConfidence, e.Result.Text);

                            if (e.Result.Confidence >= Tracker.Options.MinimumRecognitionConfidence)
                            {
                                // If the confidence level is too low to be
                                // executed, but high enough to be recognized,
                                // let Tracker say something
                                Tracker.Say(Tracker.Responses.Misheard);
                                Tracker.RestartIdleTimers();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "An error occurred while executing recognized command '{command}': \"{text}\".", ruleName, e.Result.Text);
                        Tracker.Error();
                    }
                };
                Grammars.Add(grammar);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "An error occurred while constructing the speech recognition grammar for command '{command}'.", ruleName);
                throw new GrammarException($"An error occurred while constructing the speech recognition grammar for command '{ruleName}'.", ex);
            }
        }

        /// <summary>
        /// Gets the pluralized item names for items that can be tracked more
        /// than once for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible item
        /// names.
        /// </returns>
        protected virtual Choices GetPluralItemNames()
        {
            var itemNames = new Choices();
            foreach (var itemData in ItemService.AllItems().Where(x => x.Multiple && !x.HasStages))
            {
                if (itemData.Plural == null)
                {
                    Logger.LogWarning("{item} is marked as Multiple but does not have plural names", itemData.Name[0]);
                    continue;
                }

                foreach (var name in itemData.Plural)
                    itemNames.Add(new SemanticResultValue(name.ToString(), itemData.Name[0].Text));
            }

            return itemNames;
        }

        /// <summary>
        /// Gets the item names for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible item
        /// names.
        /// </returns>
        protected virtual Choices GetItemNames(Func<ItemData, bool>? where = null)
        {
            if (where == null)
            {
                where = a => 1 == 1;
            }

            var itemNames = new Choices();
            foreach (var itemData in ItemService.AllItems().Where(where))
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
        protected virtual Choices GetDungeonNames(bool includeDungeonsWithoutReward = false)
        {
            var dungeonNames = new Choices();
            foreach (var dungeon in Tracker.WorldInfo.Dungeons)
            {
                if (dungeon.HasReward || includeDungeonsWithoutReward)
                {
                    foreach (var name in dungeon.Name)
                        dungeonNames.Add(new SemanticResultValue(name.Text, dungeon.Type.FullName));
                }
            }

            return dungeonNames;
        }

        /// <summary>
        /// Gets the names of bosses for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible boss
        /// names.
        /// </returns>
        protected virtual Choices GetBossNames()
        {
            var bossNames = new Choices();
            foreach (var dungeon in Tracker.WorldInfo.Dungeons)
            {
                foreach (var name in dungeon.Boss)
                    bossNames.Add(new SemanticResultValue(name.Text, dungeon.Type.FullName));
            }
            foreach (var boss in Tracker.WorldInfo.Bosses)
            {
                foreach (var name in boss.Name)
                    bossNames.Add(new SemanticResultValue(name.Text, name.Text));
            }
            return bossNames;
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

            foreach (var location in Tracker.WorldInfo.Locations)
            {
                foreach (var name in location.Name)
                    locationNames.Add(new SemanticResultValue(name.Text, location.LocationNumber));
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

            foreach (var room in Tracker.WorldInfo.Rooms)
            {
                foreach (var name in room.Name)
                    roomNames.Add(new SemanticResultValue(name.Text, room.Type.FullName));
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
        protected virtual Choices GetRegionNames(bool excludeDungeons = false)
        {
            var regionNames = new Choices();

            foreach (var region in Tracker.WorldInfo.Regions)
            {
                if (excludeDungeons && Tracker.WorldInfo.Dungeon(region.Type.FullName) != null)
                    continue;

                foreach (var name in region.Name)
                    regionNames.Add(new SemanticResultValue(name.Text, region.Type.FullName));
            }

            return regionNames;
        }
    }
}
