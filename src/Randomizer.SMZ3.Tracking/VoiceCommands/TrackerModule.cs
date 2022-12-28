using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared.Enums;
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
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">Used to log information.</param>
        protected TrackerModule(Tracker tracker, IItemService itemService, IWorldService worldService, ILogger logger)
        {
            Tracker = tracker;
            ItemService = itemService;
            WorldService = worldService;
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

        /// <summary>
        /// Service for getting item data
        /// </summary>
        protected IItemService ItemService { get; }

        /// <summary>
        /// Service for getting world data
        /// </summary>
        protected IWorldService WorldService { get; }

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
        protected static IDungeon GetDungeonFromResult(Tracker tracker, RecognitionResult result)
        {
            var name = (string)result.Semantics[DungeonKey].Value;
            var dungeon = tracker.World.Dungeons.FirstOrDefault(x => x.DungeonName == name);
            return dungeon ?? throw new Exception($"Could not find dungeon {name} (\"{result.Text}\").");
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
        protected static IDungeon? GetBossDungeonFromResult(Tracker tracker, RecognitionResult result)
        {
            var name = (string)result.Semantics[BossKey].Value;
            return tracker.World.Dungeons.FirstOrDefault(x => x.DungeonName == name);
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
        protected static Boss? GetBossFromResult(Tracker tracker, RecognitionResult result)
        {
            var bossName = (string)result.Semantics[BossKey].Value;
            return tracker.World.AllBosses.SingleOrDefault(x => x.Name.Contains(bossName, StringComparison.Ordinal));
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
        protected Item GetItemFromResult(Tracker tracker, RecognitionResult result, out string itemName)
        {
            itemName = (string)result.Semantics[ItemNameKey].Value;
            var item = ItemService.FirstOrDefault(itemName);

            return item ?? throw new Exception($"Could not find recognized item '{itemName}' (\"{result.Text}\")");
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
            var location = tracker.World.Locations.First(x => x.Id == id);
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
            var room = tracker.World.Rooms.First(x => x.GetType().FullName == roomTypeName);
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
            var region = tracker.World.Regions.First(x => x.GetType().FullName == regionTypeName);
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
            Action<RecognitionResult> executeCommand)
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
            Action<RecognitionResult> executeCommand)
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
            Action<RecognitionResult> executeCommand)
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

                            executeCommand(e.Result);
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
            foreach (var item in ItemService.LocalPlayersItems().Where(x => x.Metadata is { Multiple: true, HasStages: false }))
            {
                if (item.Metadata.Plural == null)
                {
                    Logger.LogWarning("{item} is marked as Multiple but does not have plural names", item.Name);
                    continue;
                }

                foreach (var name in item.Metadata.Plural)
                    itemNames.Add(new SemanticResultValue(name.ToString(), item.Name));
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
        protected virtual Choices GetItemNames(Func<Item, bool>? where = null)
        {
            where ??= a => true;

            var itemNames = new Choices();
            foreach (var item in ItemService.LocalPlayersItems().Where(where))
            {
                foreach (var name in item.Metadata.Name)
                {
                    itemNames.Add(new SemanticResultValue(name.ToString(), item.Name));
                }

                if (item.Metadata.Stages != null)
                {
                    foreach (var stageName in item.Metadata.Stages.SelectMany(x => x.Value))
                    {
                        itemNames.Add(new SemanticResultValue(stageName.ToString(), item.Name));
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
            foreach (var dungeon in Tracker.World.Dungeons)
            {
                if ((dungeon.DungeonState.HasReward || includeDungeonsWithoutReward))
                {
                    foreach (var name in dungeon.DungeonMetadata.Name)
                        dungeonNames.Add(new SemanticResultValue(name.Text, dungeon.DungeonName));
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
            foreach (var dungeon in Tracker.World.Dungeons)
            {
                foreach (var name in dungeon.DungeonMetadata.Boss)
                    bossNames.Add(new SemanticResultValue(name.Text, dungeon.DungeonName));
            }
            foreach (var boss in Tracker.World.AllBosses)
            {
                foreach (var name in boss.Metadata.Name)
                    bossNames.Add(new SemanticResultValue(name.Text, boss.Name));
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

            foreach (var location in Tracker.World.Locations)
            {
                foreach (var name in location.Metadata.Name)
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
                var roomName = room.GetType().FullName;
                if (roomName == null) continue;
                foreach (var name in room.Metadata.Name)
                    roomNames.Add(new SemanticResultValue(name.Text, roomName));
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

            foreach (var region in Tracker.World.Regions)
            {
                var regionName = region.GetType().FullName;
                if (excludeDungeons && region is IDungeon || regionName == null)
                    continue;

                foreach (var name in region.Metadata.Name)
                    regionNames.Add(new SemanticResultValue(name.Text, regionName));
            }

            return regionNames;
        }

        /// <summary>
        /// Get the medallion names for speech recognition.
        /// </summary>
        /// <returns>
        /// A new <see cref="Choices"/> object representing all possible medallion
        /// names mapped to the primary item name.
        /// </returns>
        protected virtual Choices GetMedallionNames()
        {
            var medallions = new Choices();

            var medallionTypes = new List<ItemType>(Enum.GetValues<ItemType>());
            foreach (var medallion in medallionTypes.Where(x => x == ItemType.Nothing || x.IsInCategory(ItemCategory.Medallion)))
            {
                var item = ItemService.FirstOrDefault(medallion);
                if (item?.Metadata != null)
                {
                    foreach (var name in item.Metadata.Name)
                        medallions.Add(new SemanticResultValue(medallion.ToString(), item.Name));
                }
            }

            return medallions;
        }
    }
}
