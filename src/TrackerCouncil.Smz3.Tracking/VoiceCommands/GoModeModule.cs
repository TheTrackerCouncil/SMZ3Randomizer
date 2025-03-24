using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using PySpeechServiceClient.Grammar;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for turning on Go Mode.
/// </summary>
public class GoModeModule : TrackerModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoModeModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to log information.</param>
    public GoModeModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<GoModeModule> logger)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
    }

    private SpeechRecognitionGrammarBuilder GetGoModeRule(List<string> prompts)
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf(prompts.ToArray());
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public override void AddCommands()
    {
        if (TrackerBase.Responses.GoModePrompts == null)
        {
            return;
        }

        AddCommand("Toggle Go Mode", GetGoModeRule(TrackerBase.Responses.GoModePrompts), (result) =>
        {
            TrackerBase.ModeTracker.ToggleGoMode(result.Confidence);
        });
    }
}
