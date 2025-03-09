using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for tracking goals
/// </summary>
public class GoalModule : TrackerModule
{
    private ResponseConfig _responseConfig;
    private const string ItemCountKey = "ItemCount";

    /// <summary>
    /// Initializes a new instance of the <see cref="GoalModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to log information.</param>
    /// <param name="responseConfig"></param>
    public GoalModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<GoModeModule> logger, ResponseConfig responseConfig)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        _responseConfig = responseConfig;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private GrammarBuilder GetGanonsTowerCrystalCountRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("Ganon's Tower", "G T")
            .OneOf("requires", "needs")
            .Append(ItemCountKey, GetNumberChoices(0, 7))
            .OneOf("crystals", "crystal")
            .Optional("to enter");
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private GrammarBuilder GetGanonsCrystalCountRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("Ganon", "Ganondorf")
            .OneOf("requires", "needs")
            .Append(ItemCountKey, GetNumberChoices(0, 7))
            .OneOf("crystals", "crystal")
            .Optional("to kill", "to defeat");
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private GrammarBuilder GetTourianBossCountRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("tourian", "the statue room", "the metroid statue room", "the golden statue room")
            .OneOf("requires", "needs")
            .Append(ItemCountKey, GetNumberChoices(0, 4))
            .OneOf("bosses", "boss tokens");
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public override void AddCommands()
    {
        if (TrackerBase.World.Config.RomGenerator == RomGenerator.Cas)
        {
            return;
        }

        AddCommand("GT Crystal Count", GetGanonsTowerCrystalCountRule(), (result) =>
        {
            var count = (int)result.Semantics[ItemCountKey].Value;
            TrackerBase.GameStateTracker.UpdateGanonsTowerRequirement(count, false);
        });

        AddCommand("Ganon Crystal Count", GetGanonsCrystalCountRule(), (result) =>
        {
            var count = (int)result.Semantics[ItemCountKey].Value;
            TrackerBase.GameStateTracker.UpdateGanonRequirement(count, false);
        });

        AddCommand("Tourian Boss Count", GetTourianBossCountRule(), (result) =>
        {
            var count = (int)result.Semantics[ItemCountKey].Value;
            TrackerBase.GameStateTracker.UpdateTourianRequirement(count, false);
        });
    }
}
