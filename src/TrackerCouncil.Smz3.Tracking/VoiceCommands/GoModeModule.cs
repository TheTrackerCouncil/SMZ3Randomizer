using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for turning on Go Mode.
/// </summary>
public class GoModeModule : TrackerModule
{
    private ResponseConfig _responseConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoModeModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to log information.</param>
    /// <param name="responseConfig"></param>
    public GoModeModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<GoModeModule> logger, ResponseConfig responseConfig)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        _responseConfig = responseConfig;
    }

    private GrammarBuilder GetGoModeRule(List<string> prompts)
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf(prompts.ToArray());
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public override void AddCommands()
    {
        if (_responseConfig.GoModePrompts == null)
        {
            return;
        }

        AddCommand("Toggle Go Mode", GetGoModeRule(_responseConfig.GoModePrompts), (result) =>
        {
            TrackerBase.ModeTracker.ToggleGoMode(result.Confidence);
        });
    }
}
