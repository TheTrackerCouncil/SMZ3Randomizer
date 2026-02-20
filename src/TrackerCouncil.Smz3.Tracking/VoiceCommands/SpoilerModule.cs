using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands that reveal about items and locations.
/// </summary>
public class SpoilerModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, ILogger<SpoilerModule> logger, IWorldQueryService worldQueryService) : TrackerModule(tracker, playerProgressionService, worldQueryService, logger), IOptionalModule
{
    private SpeechRecognitionGrammarBuilder GetItemSpoilerRule()
    {
        var items = GetItemNames();

        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("where is", "where's", "where are", "where can I find",
                "where the fuck is", "where the hell is",
                "where the heck is")
            .Optional("the", "a", "an")
            .Append(ItemNameKey, items);
    }

    private SpeechRecognitionGrammarBuilder GetLocationSpoilerRule()
    {
        var locations = GetLocationNames();

        var whatsAtRule = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("what's", "what is")
            .OneOf("at", "in")
            .Optional("the")
            .Append(LocationKey, locations);

        var whatDoesLocationHaveRule = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Append("what does")
            .Optional("the")
            .Append(LocationKey, locations)
            .Append("have")
            .Optional("for me");

        return SpeechRecognitionGrammarBuilder.Combine(whatsAtRule, whatDoesLocationHaveRule);
    }

    private SpeechRecognitionGrammarBuilder GetEnableSpoilersRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("enable", "turn on")
            .Append("spoilers");
    }

    private SpeechRecognitionGrammarBuilder GetDisableSpoilersRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("disable", "turn off")
            .Append("spoilers");
    }

    private SpeechRecognitionGrammarBuilder GetEnableHintsRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("enable", "turn on")
            .Append("hints");
    }

    private SpeechRecognitionGrammarBuilder GetDisableHintsRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("disable", "turn off")
            .Append("hints");
    }

    private SpeechRecognitionGrammarBuilder GetProgressionHintRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("give me a hint",
                "give me a suggestion",
                "can you give me a hint?",
                "do you have any suggestions?",
                "where should I go?",
                "what should I do?");
    }

    private SpeechRecognitionGrammarBuilder GetLocationUsefulnessHintRule()
    {
        var regionNames = GetRegionNames();
        var regionGrammar = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("is there anything useful",
                "is there anything I need",
                "is there anything good",
                "is there anything",
                "what's left",
                "what's")
            .OneOf("in", "at")
            .Append(RegionKey, regionNames);

        var roomNames = GetRoomNames();
        var roomGrammar = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("is there anything useful",
                "is there anything I need",
                "is there anything good",
                "is there anything",
                "what's left",
                "what's")
            .OneOf("in", "at")
            .Append(RoomKey, roomNames);

        return SpeechRecognitionGrammarBuilder.Combine(regionGrammar, roomGrammar);
    }

    public override void AddCommands()
    {
        if (TrackerBase.World.Config.Race) return;

        if (!TrackerBase.World.Config.DisableTrackerHints)
        {
            AddCommand("Enable hints", GetEnableHintsRule(), (_) =>
            {
                TrackerBase.SpoilerService.ToggleHints(true);
            });
            AddCommand("Disable hints", GetDisableHintsRule(), (_) =>
            {
                TrackerBase.SpoilerService.ToggleHints(false);
            });
            AddCommand("Give progression hint", GetProgressionHintRule(), (_) =>
            {
                TrackerBase.SpoilerService.GiveProgressionHint();
            });

            AddCommand("Give area hint", GetLocationUsefulnessHintRule(), (result) =>
            {
                var area = GetAreaFromResult(TrackerBase, result);
                TrackerBase.SpoilerService.GiveAreaHint(area);
            });
        }

        if (!TrackerBase.World.Config.DisableTrackerSpoilers)
        {
            AddCommand("Enable spoilers", GetEnableSpoilersRule(), (_) =>
            {
                TrackerBase.SpoilerService.ToggleSpoilers(true);
            });
            AddCommand("Disable spoilers", GetDisableSpoilersRule(), (_) =>
            {
                TrackerBase.SpoilerService.ToggleSpoilers(false);
            });

            AddCommand("Reveal item location", GetItemSpoilerRule(), (result) =>
            {
                var item = GetItemFromResult(TrackerBase, result, out _);
                TrackerBase.SpoilerService.RevealItemLocation(item);
            });

            AddCommand("Reveal location item", GetLocationSpoilerRule(), (result) =>
            {
                var location = GetLocationFromResult(TrackerBase, result);
                TrackerBase.SpoilerService.RevealLocationItem(location);
            });
        }
    }
}
