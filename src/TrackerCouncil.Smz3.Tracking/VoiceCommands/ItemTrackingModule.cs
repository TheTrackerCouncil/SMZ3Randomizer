using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for tracking items.
/// </summary>
public class ItemTrackingModule : TrackerModule
{
    private const string ItemCountKey = "ItemCount";

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemTrackingModule"/>
    /// class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to log information.</param>
    public ItemTrackingModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<ItemTrackingModule> logger)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {

    }

    private SpeechRecognitionGrammarBuilder GetTrackDeathRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Append("I just died");
    }

    private SpeechRecognitionGrammarBuilder GetTrackItemRule(bool isMultiworld)
    {
        var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
        var itemNames = GetItemNames(x => x.Name != "Content");
        var locationNames = GetLocationNames();
        var roomNames = GetRoomNames();

        var trackItemNormal = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional(ForceCommandIdentifiers.Concat(["please", "would you kindly"]).ToArray())
            .OneOf("track", "add")
            .Append(ItemNameKey, itemNames);

        if (!isMultiworld)
        {
            var trackItemDungeon = new SpeechRecognitionGrammarBuilder()
                .Append("Hey tracker,")
                .Optional(ForceCommandIdentifiers.Concat(["please", "would you kindly"]).ToArray())
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from")
                .Append(DungeonKey, dungeonNames);

            var trackItemLocation = new SpeechRecognitionGrammarBuilder()
                .Append("Hey tracker,")
                .Optional(ForceCommandIdentifiers.Concat(["please", "would you kindly"]).ToArray())
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from", "in the", "from the")
                .Append(LocationKey, locationNames);

            var trackItemRoom = new SpeechRecognitionGrammarBuilder()
                .Append("Hey tracker,")
                .Optional(ForceCommandIdentifiers.Concat(["please", "would you kindly"]).ToArray())
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from", "in the", "from the")
                .Append(RoomKey, roomNames);

            return SpeechRecognitionGrammarBuilder.Combine(
                trackItemNormal, trackItemDungeon, trackItemLocation, trackItemRoom);

        }
        else
        {
            return trackItemNormal;
        }

    }

    private SpeechRecognitionGrammarBuilder GetTrackEverythingRule()
    {
        var roomNames = GetRoomNames();
        var regionNames = GetRegionNames();

        var trackAllInRoom = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("track", "add")
            .OneOf("everything", "available items")
            .OneOf("in", "from", "in the", "from the")
            .Append(RoomKey, roomNames);

        var trackAllInRegion = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("track", "add")
            .OneOf("everything", "available items")
            .OneOf("in", "from")
            .Append(RegionKey, regionNames);

        return SpeechRecognitionGrammarBuilder.Combine(trackAllInRoom, trackAllInRegion);
    }

    private SpeechRecognitionGrammarBuilder GetTrackEverythingIncludingOutOfLogicRule()
    {
        var roomNames = GetRoomNames();
        var regionNames = GetRegionNames();

        var trackAllInRoom = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("force", "sudo")
            .OneOf("track", "add")
            .OneOf("everything", "all items")
            .OneOf("in", "from", "in the", "from the")
            .Append(RoomKey, roomNames);

        var trackAllInRegion = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("force", "sudo")
            .OneOf("track", "add")
            .OneOf("everything", "all items")
            .OneOf("in", "from")
            .Append(RegionKey, regionNames);

        var cheatedRoom = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("sequence break", "I sequence broke", "I cheated my way to")
            .Append(RoomKey, roomNames);

        var cheatedRegion = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("sequence break", "I sequence broke", "I cheated my way to")
            .Append(RegionKey, regionNames);

        return SpeechRecognitionGrammarBuilder.Combine(trackAllInRoom, trackAllInRegion,
            cheatedRoom, cheatedRegion);
    }

    private SpeechRecognitionGrammarBuilder GetUntrackItemRule()
    {
        var itemNames = GetItemNames();

        var untrackItem = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional(ForceCommandIdentifiers.Concat(["please", "would you kindly"]).ToArray())
            .OneOf("untrack", "remove")
            .Optional("a", "an", "the")
            .Append(ItemNameKey, itemNames);

        var toggleItemOff = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional(ForceCommandIdentifiers.Concat(["please", "would you kindly"]).ToArray())
            .Append("toggle")
            .Append(ItemNameKey, itemNames)
            .Append("off");

        return SpeechRecognitionGrammarBuilder.Combine(untrackItem, toggleItemOff);
    }

    private SpeechRecognitionGrammarBuilder GetSetItemCountRule()
    {
        var itemNames = GetPluralItemNames();
        var numbers = new List<GrammarKeyValueChoice>();
        for (var i = 0; i <= 200; i++)
            numbers.Add(new GrammarKeyValueChoice(i.ToString(), i.ToString()));

        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional(ForceCommandIdentifiers)
            .OneOf("I have", "I've got", "I possess", "I am in the possession of", "track")
            .Append(ItemCountKey, numbers)
            .Append(ItemNameKey, itemNames);
    }

    public override void AddCommands()
    {
        var isMultiworld = WorldQueryService.World.Config.MultiWorld;

        AddCommand("Track item", GetTrackItemRule(isMultiworld), (result) =>
        {
            var item = GetItemFromResult(TrackerBase, result, out var itemName);

            var force = result.Text.ContainsAny(ForceCommandIdentifiers);

            if (result.Semantics.ContainsKey(DungeonKey))
            {
                var dungeon = GetDungeonFromResult(TrackerBase, result);
                TrackerBase.ItemTracker.TrackItemFrom(item, dungeon,
                    trackedAs: itemName,
                    confidence: result.Confidence,
                    force: force);
            }
            else if (result.Semantics.ContainsKey(RoomKey))
            {
                var room = GetRoomFromResult(TrackerBase, result);
                TrackerBase.ItemTracker.TrackItemFrom(item, room,
                    trackedAs: itemName,
                    confidence: result.Confidence,
                    force: force);
            }
            else if (result.Semantics.ContainsKey(LocationKey))
            {
                var location = GetLocationFromResult(TrackerBase, result);
                TrackerBase.ItemTracker.TrackItem(item: item,
                    trackedAs: itemName,
                    confidence: result.Confidence,
                    tryClear: true,
                    autoTracked: false,
                    location: location,
                    force: force);
            }
            else
            {
                TrackerBase.ItemTracker.TrackItem(item,
                    trackedAs: itemName,
                    confidence: result.Confidence,
                    force: force);
            }
        });

        AddCommand("Track death", GetTrackDeathRule(), (result) =>
        {
            var death = WorldQueryService.FirstOrDefault("Death");
            if (death == null)
            {
                Logger.LogError("Tried to track death, but could not find an item named 'Death'.");
                TrackerBase.Say(x => x.Error);
                return;
            }

            TrackerBase.ItemTracker.TrackItem(death, confidence: result.Confidence, tryClear: false);
        });

        if (!isMultiworld)
        {
            AddCommand("Track available items in an area", GetTrackEverythingRule(), (result) =>
            {
                if (result.Semantics.ContainsKey(RoomKey))
                {
                    var room = GetRoomFromResult(TrackerBase, result);
                    TrackerBase.LocationTracker.ClearArea(room,
                        trackItems: true,
                        includeUnavailable: false,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(RegionKey))
                {
                    var region = GetRegionFromResult(TrackerBase, result);
                    TrackerBase.LocationTracker.ClearArea(region,
                        trackItems: true,
                        includeUnavailable: false,
                        confidence: result.Confidence);
                }
            });

            AddCommand("Track all items in an area (including out-of-logic)", GetTrackEverythingIncludingOutOfLogicRule(), (result) =>
            {
                if (result.Semantics.ContainsKey(RoomKey))
                {
                    var room = GetRoomFromResult(TrackerBase, result);
                    TrackerBase.LocationTracker.ClearArea(room,
                        trackItems: true,
                        includeUnavailable: true,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(RegionKey))
                {
                    var region = GetRegionFromResult(TrackerBase, result);
                    TrackerBase.LocationTracker.ClearArea(region,
                        trackItems: true,
                        includeUnavailable: true,
                        confidence: result.Confidence);
                }
            });
        }

        AddCommand("Untrack an item", GetUntrackItemRule(), (result) =>
        {
            var item = GetItemFromResult(TrackerBase, result, out _);
            var force = result.Text.ContainsAny(ForceCommandIdentifiers);
            TrackerBase.ItemTracker.UntrackItem(item, result.Confidence, force: force);
        });

        AddCommand("Set item count", GetSetItemCountRule(), (result) =>
        {
            var item = GetItemFromResult(TrackerBase, result, out _);
            var count = int.Parse(result.Semantics[ItemCountKey].Value);
            var force = result.Text.ContainsAny(ForceCommandIdentifiers);
            TrackerBase.ItemTracker.TrackItemAmount(item, count, result.Confidence, force: force);
        });
    }
}
