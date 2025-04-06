using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides Tracker with speech recognition commands.
/// </summary>
public abstract class TrackerModule
{
    /// <summary>
    /// Phrases for forcing a commmand when using auto tracking
    /// </summary>
    protected static string[] ForceCommandIdentifiers = ["would you please", "please, I'm begging you"];

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
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to log information.</param>
    protected TrackerModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger logger)
    {
        TrackerBase = tracker;
        PlayerProgressionService = playerProgressionService;
        WorldQueryService = worldQueryService;
        Logger = logger;
    }

    /// <summary>
    /// Add voice commands for the tracker module
    /// </summary>
    public abstract void AddCommands();

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
    protected TrackerBase TrackerBase { get; }

    /// <summary>
    /// Service for getting item data
    /// </summary>
    protected IPlayerProgressionService PlayerProgressionService { get; }

    /// <summary>
    /// Service for getting world data
    /// </summary>
    protected IWorldQueryService WorldQueryService { get; }

    /// <summary>
    /// Gets a list of speech recognition grammars provided by the module.
    /// </summary>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public IList<SpeechRecognitionGrammar> Grammars { get; }
        = new List<SpeechRecognitionGrammar>();

    /// <summary>
    /// Gets a logger for the current module.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Loads the voice commands provided by the module into the specified
    /// recognition engine.
    /// </summary>
    /// <param name="grammar">
    /// The speech recognition grammar to initialize
    /// </param>
    public void LoadInto(List<SpeechRecognitionGrammar> grammar)
    {
        grammar.AddRange(Grammars);
    }

    /// <summary>
    /// Returns the <see cref="IHasTreasure"/> that was detected in a voice
    /// command using <see cref="DungeonKey"/>.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="result">The speech recognition result.</param>
    /// <returns>
    /// A <see cref="IHasTreasure"/> from the recognition result.
    /// </returns>
    protected static IHasTreasure GetDungeonFromResult(TrackerBase tracker, SpeechRecognitionResult result)
    {
        var name = (string)result.Semantics[DungeonKey].Value;
        var dungeon = tracker.World.TreasureRegions.FirstOrDefault(x => x.Name == name);
        return dungeon ?? throw new Exception($"Could not find dungeon {name} (\"{result.Text}\").");
    }

    /// <summary>
    /// Returns the <see cref="IHasTreasure"/> that was detected in a voice
    /// command using <see cref="BossKey"/>.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="result">The speech recognition result.</param>
    /// <returns>
    /// A <see cref="IHasTreasure"/> from the recognition result.
    /// </returns>
    protected static IHasTreasure? GetBossDungeonFromResult(TrackerBase tracker, SpeechRecognitionResult result)
    {
        var name = (string)result.Semantics[BossKey].Value;
        return tracker.World.TreasureRegions.FirstOrDefault(x => x.Name == name);
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
    protected static Boss? GetBossFromResult(TrackerBase tracker, SpeechRecognitionResult result)
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
    protected Item GetItemFromResult(TrackerBase tracker, SpeechRecognitionResult result, out string itemName)
    {
        itemName = (string)result.Semantics[ItemNameKey].Value;
        var item = WorldQueryService.FirstOrDefault(itemName);

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
    protected static Location GetLocationFromResult(TrackerBase tracker, SpeechRecognitionResult result)
    {
        var id = (LocationId)int.Parse(result.Semantics[LocationKey].Value);
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
    protected static Room GetRoomFromResult(TrackerBase tracker, SpeechRecognitionResult result)
    {
        var roomTypeName = result.Semantics[RoomKey].Value;
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
    protected static Region GetRegionFromResult(TrackerBase tracker, SpeechRecognitionResult result)
    {
        var regionTypeName = result.Semantics[RegionKey].Value;
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
    protected static IHasLocations GetAreaFromResult(TrackerBase tracker, SpeechRecognitionResult result)
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
        Action<SpeechRecognitionResult> executeCommand)
    {
        var builder = new SpeechRecognitionGrammarBuilder()
            .Append(phrase);

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
        Action<SpeechRecognitionResult> executeCommand)
    {
        var builder = new SpeechRecognitionGrammarBuilder()
            .OneOf(phrases);

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
    protected void AddCommand(string ruleName, SpeechRecognitionGrammarBuilder grammarBuilder,
        Action<SpeechRecognitionResult> executeCommand)
    {
        try
        {
            var grammar = grammarBuilder.BuildGrammar(ruleName);
            _syntax.TryAdd(ruleName, grammar.HelpText);
            grammar.SpeechRecognized += (_, e) =>
            {
                try
                {
                    var minimumConfidence = Math.Max(TrackerBase.Options.MinimumRecognitionConfidence, TrackerBase.Options.MinimumExecutionConfidence);
                    if (e.Result.Confidence >= minimumConfidence)
                    {
                        Logger.LogInformation("Recognized \"{text}\" with {confidence:P2} confidence.",
                            e.Result.Text, e.Result.Confidence);
                        TrackerBase.RestartIdleTimers();

                        executeCommand(e.Result);
                    }
                    else
                    {
                        Logger.LogWarning("Confidence level too low ({Confidence} < {Threshold}) in voice command: \"{Text}\".",
                            e.Result.Confidence, minimumConfidence, e.Result.Text);

                        if (e.Result.Confidence >= TrackerBase.Options.MinimumRecognitionConfidence)
                        {
                            // If the confidence level is too low to be
                            // executed, but high enough to be recognized,
                            // let Tracker say something
                            TrackerBase.Say(x => x.Misheard);
                            TrackerBase.RestartIdleTimers();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An error occurred while executing recognized command '{command}': \"{text}\".", ruleName, e.Result.Text);
                    TrackerBase.Error();
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
    protected virtual List<GrammarKeyValueChoice> GetPluralItemNames()
    {
        var itemNames = new List<GrammarKeyValueChoice>();
        foreach (var item in WorldQueryService.LocalPlayersItems().Where(x => x.Metadata is { Multiple: true, HasStages: false }))
        {
            if (item.Metadata.Plural == null)
            {
                Logger.LogWarning("{item} is marked as Multiple but does not have plural names", item.Name);
                continue;
            }

            foreach (var name in item.Metadata.Plural)
                itemNames.Add(new GrammarKeyValueChoice(name.ToString(), item.Name));
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
    protected virtual List<GrammarKeyValueChoice> GetItemNames(Func<Item, bool>? where = null)
    {
        where ??= _ => true;

        var addedNames = new HashSet<string>();
        var itemNames = new List<GrammarKeyValueChoice>();
        foreach (var item in WorldQueryService.LocalPlayersItems().Where(where))
        {
            if (item.Metadata.Name != null)
            {
                foreach (var name in item.Metadata.Name)
                {
                    if (!addedNames.Contains(name.ToString()))
                    {
                        itemNames.Add(new GrammarKeyValueChoice(name.ToString(), item.Name));
                        addedNames.Add(name.ToString());
                    }
                }
            }
            else
            {
                if (!addedNames.Contains(item.Metadata.Item))
                {
                    itemNames.Add(new GrammarKeyValueChoice(item.Metadata.Item, item.Name));
                    addedNames.Add(item.Metadata.Item);
                }
            }

            if (item.Metadata.Stages != null)
            {
                foreach (var stageName in item.Metadata.Stages.SelectMany(x => x.Value))
                {
                    if (!addedNames.Contains(stageName.ToString()))
                    {
                        itemNames.Add(new GrammarKeyValueChoice(stageName.ToString(), item.Name));
                        addedNames.Add(stageName.ToString());
                    }
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
    protected virtual List<GrammarKeyValueChoice> GetDungeonNames(bool includeDungeonsWithoutReward = false)
    {
        var dungeonNames = new List<GrammarKeyValueChoice>();
        foreach (var dungeon in TrackerBase.World.TreasureRegions)
        {
            var rewardRegion = dungeon as IHasReward;

            if (rewardRegion == null && !includeDungeonsWithoutReward) continue;

            if (dungeon.Metadata.Name != null)
            {
                foreach (var name in dungeon.Metadata.Name)
                    dungeonNames.Add(new GrammarKeyValueChoice(name.Text, dungeon.Name));
            }
            else
            {
                dungeonNames.Add(new GrammarKeyValueChoice(dungeon.Name, dungeon.Name));
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
    protected virtual List<GrammarKeyValueChoice> GetBossNames()
    {
        var bossNames = new List<GrammarKeyValueChoice>();
        foreach (var boss in TrackerBase.World.AllBosses)
        {
            if (boss.Metadata.Name != null)
            {
                foreach (var name in boss.Metadata.Name)
                    bossNames.Add(new GrammarKeyValueChoice(name.Text, boss.Name));
            }
            else
            {
                bossNames.Add(new GrammarKeyValueChoice(boss.Name, boss.Name));
            }
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
    protected virtual List<GrammarKeyValueChoice> GetLocationNames()
    {
        var locationNames = new List<GrammarKeyValueChoice>();

        foreach (var location in TrackerBase.World.Locations)
        {
            if (location.Metadata.Name != null)
            {
                foreach (var name in location.Metadata.Name)
                    locationNames.Add(new GrammarKeyValueChoice(name.Text, ((int)location.Id).ToString()));
            }
            else
            {
                locationNames.Add(new GrammarKeyValueChoice(location.Name, ((int)location.Id).ToString()));
            }
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
    protected virtual List<GrammarKeyValueChoice> GetRoomNames()
    {
        var roomNames = new List<GrammarKeyValueChoice>();

        foreach (var room in TrackerBase.World.Rooms)
        {
            var roomName = room.GetType().FullName;
            if (roomName == null || room.Metadata.Name == null) continue;
            foreach (var name in room.Metadata.Name)
                roomNames.Add(new GrammarKeyValueChoice(name.Text, roomName));
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
    protected virtual List<GrammarKeyValueChoice> GetRegionNames(bool excludeDungeons = false)
    {
        var regionNames = new List<GrammarKeyValueChoice>();

        foreach (var region in TrackerBase.World.Regions)
        {
            var regionName = region.GetType().FullName;
            if (excludeDungeons && region is IHasTreasure || regionName == null || region.Metadata.Name == null)
                continue;

            foreach (var name in region.Metadata.Name)
                regionNames.Add(new GrammarKeyValueChoice(name.Text, regionName));
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
    protected virtual List<GrammarKeyValueChoice> GetMedallionNames()
    {
        var medallions = new List<GrammarKeyValueChoice>();

        var medallionTypes = new List<ItemType>(Enum.GetValues<ItemType>());
        foreach (var medallion in medallionTypes.Where(x => x == ItemType.Nothing || x.IsInCategory(ItemCategory.Medallion)))
        {
            var item = WorldQueryService.FirstOrDefault(medallion);
            if (item?.Metadata.Name != null)
            {
                foreach (var name in item.Metadata.Name)
                    medallions.Add(new GrammarKeyValueChoice(medallion.ToString(), item.Name));
            }
        }

        return medallions;
    }

    /// <summary>
    /// Gets speech recognition choices ranging from min to max
    /// </summary>
    /// <param name="min">Minimum number choice</param>
    /// <param name="max">Maximum number choice</param>
    /// <returns>The choices for the user to choose from in speech recognition</returns>
    protected virtual List<GrammarKeyValueChoice> GetNumberChoices(int min, int max)
    {
        var numbers = new List<GrammarKeyValueChoice>();
        for (var i = min; i <= max; i++)
            numbers.Add(new GrammarKeyValueChoice(i.ToString(), i));
        return numbers;
    }

    /// <summary>
    /// Gets speech recognition choices ranging from 0 to the provided number
    /// </summary>
    /// <param name="max">Maximum number choice</param>
    /// <returns>The choices for the user to choose from in speech recognition</returns>
    protected virtual List<GrammarKeyValueChoice> GetNumberChoices(int max)

    {
        return GetNumberChoices(0, max);
    }

}
