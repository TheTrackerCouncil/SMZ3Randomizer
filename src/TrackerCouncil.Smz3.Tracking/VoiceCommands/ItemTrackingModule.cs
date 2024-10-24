using System.Runtime.Versioning;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
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

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetTrackDeathRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .Append("I just died");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetTrackItemRule(bool isMultiworld)
    {
        var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
        var itemNames = GetItemNames(x => x.Name != "Content");
        var locationNames = GetLocationNames();
        var roomNames = GetRoomNames();

        var trackItemNormal = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("track", "add")
            .Append(ItemNameKey, itemNames);

        if (!isMultiworld)
        {
            var trackItemDungeon = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from")
                .Append(DungeonKey, dungeonNames);

            var trackItemLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from", "in the", "from the")
                .Append(LocationKey, locationNames);

            var trackItemRoom = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from", "in the", "from the")
                .Append(RoomKey, roomNames);

            return GrammarBuilder.Combine(
                trackItemNormal, trackItemDungeon, trackItemLocation, trackItemRoom);

        }
        else
        {
            return trackItemNormal;
        }

    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetTrackEverythingRule()
    {
        var roomNames = GetRoomNames();
        var regionNames = GetRegionNames();

        var trackAllInRoom = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("track", "add")
            .OneOf("everything", "available items")
            .OneOf("in", "from", "in the", "from the")
            .Append(RoomKey, roomNames);

        var trackAllInRegion = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("track", "add")
            .OneOf("everything", "available items")
            .OneOf("in", "from")
            .Append(RegionKey, regionNames);

        return GrammarBuilder.Combine(trackAllInRoom, trackAllInRegion);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetTrackEverythingIncludingOutOfLogicRule()
    {
        var roomNames = GetRoomNames();
        var regionNames = GetRegionNames();

        var trackAllInRoom = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("force", "sudo")
            .OneOf("track", "add")
            .OneOf("everything", "all items")
            .OneOf("in", "from", "in the", "from the")
            .Append(RoomKey, roomNames);

        var trackAllInRegion = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("force", "sudo")
            .OneOf("track", "add")
            .OneOf("everything", "all items")
            .OneOf("in", "from")
            .Append(RegionKey, regionNames);

        var cheatedRoom = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("sequence break", "I sequence broke", "I cheated my way to")
            .Append(RoomKey, roomNames);

        var cheatedRegion = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("sequence break", "I sequence broke", "I cheated my way to")
            .Append(RegionKey, regionNames);

        return GrammarBuilder.Combine(trackAllInRoom, trackAllInRegion,
            cheatedRoom, cheatedRegion);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetUntrackItemRule()
    {
        var itemNames = GetItemNames();

        var untrackItem = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("untrack", "remove")
            .Optional("a", "an", "the")
            .Append(ItemNameKey, itemNames);

        var toggleItemOff = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .Append("toggle")
            .Append(ItemNameKey, itemNames)
            .Append("off");

        return GrammarBuilder.Combine(untrackItem, toggleItemOff);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetSetItemCountRule()
    {
        var itemNames = GetPluralItemNames();
        var numbers = new Choices();
        for (var i = 0; i <= 200; i++)
            numbers.Add(new SemanticResultValue(i.ToString(), i));

        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("I have", "I've got", "I possess", "I am in the possession of")
            .Append(ItemCountKey, numbers)
            .Append(ItemNameKey, itemNames);
    }

    [SupportedOSPlatform("windows")]
    public override void AddCommands()
    {
        var isMultiworld = WorldQueryService.World.Config.MultiWorld;

        AddCommand("Track item", GetTrackItemRule(isMultiworld), (result) =>
        {
            var item = GetItemFromResult(TrackerBase, result, out var itemName);

            if (result.Semantics.ContainsKey(DungeonKey))
            {
                var dungeon = GetDungeonFromResult(TrackerBase, result);
                TrackerBase.ItemTracker.TrackItemFrom(item, dungeon,
                    trackedAs: itemName,
                    confidence: result.Confidence);
            }
            else if (result.Semantics.ContainsKey(RoomKey))
            {
                var room = GetRoomFromResult(TrackerBase, result);
                TrackerBase.ItemTracker.TrackItemFrom(item, room,
                    trackedAs: itemName,
                    confidence: result.Confidence);
            }
            else if (result.Semantics.ContainsKey(LocationKey))
            {
                var location = GetLocationFromResult(TrackerBase, result);
                TrackerBase.ItemTracker.TrackItem(item: item,
                    trackedAs: itemName,
                    confidence: result.Confidence,
                    tryClear: true,
                    autoTracked: false,
                    location: location);
            }
            else
            {
                TrackerBase.ItemTracker.TrackItem(item,
                    trackedAs: itemName,
                    confidence: result.Confidence);
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
            TrackerBase.ItemTracker.UntrackItem(item, result.Confidence);
        });

        AddCommand("Set item count", GetSetItemCountRule(), (result) =>
        {
            var item = GetItemFromResult(TrackerBase, result, out _);
            var count = (int)result.Semantics[ItemCountKey].Value;
            TrackerBase.ItemTracker.TrackItemAmount(item, count, result.Confidence);
        });
    }
}
