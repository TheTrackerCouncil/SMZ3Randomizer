using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for marking and clearing locations.
/// </summary>
public class LocationTrackingModule : TrackerModule
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="LocationTrackingModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="itemService">Service to get item information</param>
    /// <param name="worldService">Service to get world information</param>
    /// <param name="logger">Used to log information.</param>
    public LocationTrackingModule(TrackerBase tracker, IItemService itemService, IWorldService worldService, ILogger<LocationTrackingModule> logger)
        : base(tracker, itemService, worldService, logger)
    {

    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetMarkItemAtLocationRule()
    {
        var itemNames = GetItemNames();
        var locationNames = GetLocationNames();

        var itemIsAtLocation = new GrammarBuilder()
            .Append("Hey tracker,")
            .Append(ItemNameKey, itemNames)
            .OneOf("is at", "are at")
            .Append(LocationKey, locationNames);

        var theItemIsAtLocation = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("a", "an", "the")
            .Append(ItemNameKey, itemNames)
            .OneOf("is at", "are at")
            .Append(LocationKey, locationNames);

        var thereIsItemAtLocation = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("there are", "there is a", "there is an")
            .Append(ItemNameKey, itemNames)
            .Append("at")
            .Append(LocationKey, locationNames);

        var locationHasItem = new GrammarBuilder()
            .Append("Hey tracker,")
            .Append(LocationKey, locationNames)
            .OneOf("has", "has a", "has an", "has the")
            .Append(ItemNameKey, itemNames);

        var markAtLocation = new GrammarBuilder()
            .Append("Hey tracker,")
            .Append("mark")
            .Append(ItemNameKey, itemNames)
            .Append("at")
            .Append(LocationKey, locationNames);

        return GrammarBuilder.Combine(
            itemIsAtLocation, theItemIsAtLocation, thereIsItemAtLocation,
            locationHasItem, markAtLocation);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetClearViewedObjectRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("clear", "I don't care about", "I don't give a shit about", "I don't give a fuck about")
            .OneOf("that", "those")
            .Optional("last", "recent")
            .OneOf("marked location", "marked locations", "hint tile", "telepathic tile");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetClearLocationRule()
    {
        var locationNames = GetLocationNames();

        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("clear", "ignore")
            .Append(LocationKey, locationNames);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetClearAreaRule()
    {
        var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
        var roomNames = GetRoomNames();
        var regionNames = GetRegionNames(excludeDungeons: true);

        var clearDungeon = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("clear", "please clear")
            .Append(DungeonKey, dungeonNames);

        var clearRoom = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("clear", "please clear")
            .Append(RoomKey, roomNames);

        var clearRegion = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("clear", "please clear")
            .Append(RegionKey, regionNames);

        return GrammarBuilder.Combine(clearDungeon, clearRoom, clearRegion);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetClearAreaIncludingOutOfLogicRule()
    {
        var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
        var roomNames = GetRoomNames();
        var regionNames = GetRegionNames(excludeDungeons: true);

        var clearDungeon = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("force", "sudo")
            .OneOf("clear", "please clear")
            .Append(DungeonKey, dungeonNames);

        var clearRoom = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("force", "sudo")
            .OneOf("clear", "please clear")
            .Append(RoomKey, roomNames);

        var clearRegion = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("force", "sudo")
            .OneOf("clear", "please clear")
            .Append(RegionKey, regionNames);

        return GrammarBuilder.Combine(clearDungeon, clearRoom, clearRegion);
    }

    [SupportedOSPlatform("windows")]
    public override void AddCommands()
    {
        AddCommand("Mark item at specific location", GetMarkItemAtLocationRule(), (result) =>
        {
            var item = GetItemFromResult(TrackerBase, result, out _);
            var location = GetLocationFromResult(TrackerBase, result);
            TrackerBase.LocationTracker.MarkLocation(location, item, result.Confidence);
        });

        AddCommand("Clear specific item location", GetClearLocationRule(), (result) =>
        {
            var location = GetLocationFromResult(TrackerBase, result);
            TrackerBase.LocationTracker.Clear(location, result.Confidence);
        });

        AddCommand("Clear available items in an area", GetClearAreaRule(), (result) =>
        {
            if (result.Semantics.ContainsKey(RoomKey))
            {
                var room = GetRoomFromResult(TrackerBase, result);
                TrackerBase.LocationTracker.ClearArea(room,
                    trackItems: false,
                    includeUnavailable: false,
                    confidence: result.Confidence);
            }
            else if (result.Semantics.ContainsKey(DungeonKey))
            {
                var dungeon = GetDungeonFromResult(TrackerBase, result);
                TrackerBase.TreasureTracker.ClearDungeon(dungeon, result.Confidence);
            }
            else if (result.Semantics.ContainsKey(RegionKey))
            {
                var region = GetRegionFromResult(TrackerBase, result);
                TrackerBase.LocationTracker.ClearArea(region,
                    trackItems:false,
                    includeUnavailable: false,
                    confidence: result.Confidence);
            }
        });

        AddCommand("Clear all items in an area (including out-of-logic)", GetClearAreaIncludingOutOfLogicRule(), (result) =>
        {
            if (result.Semantics.ContainsKey(RoomKey))
            {
                var room = GetRoomFromResult(TrackerBase, result);
                TrackerBase.LocationTracker.ClearArea(room,
                    trackItems: false,
                    includeUnavailable: true,
                    confidence: result.Confidence);
            }
            else if (result.Semantics.ContainsKey(DungeonKey))
            {
                var dungeon = GetDungeonFromResult(TrackerBase, result);
                TrackerBase.TreasureTracker.ClearDungeon(dungeon, result.Confidence);
            }
            else if (result.Semantics.ContainsKey(RegionKey))
            {
                var region = GetRegionFromResult(TrackerBase, result);
                TrackerBase.LocationTracker.ClearArea(region,
                    trackItems:false,
                    includeUnavailable: true,
                    confidence: result.Confidence);
            }
        });

        AddCommand("Clear recent marked locations", GetClearViewedObjectRule(), (result) =>
        {
            TrackerBase.GameStateTracker.ClearLastViewedObject(result.Confidence);
        });
    }
}
