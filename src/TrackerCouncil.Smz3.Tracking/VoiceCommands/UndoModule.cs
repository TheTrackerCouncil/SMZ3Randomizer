using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for undoing things.
/// </summary>
public class UndoModule : TrackerModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UndoModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to log information.</param>
    public UndoModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<UndoModule> logger)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {

    }

    private SpeechRecognitionGrammarBuilder GetUndoRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("undo that", "control Z", "that's not what I said", "take backsies");
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public override void AddCommands()
    {
        AddCommand("Undo last operation", GetUndoRule(), (result) =>
        {
            TrackerBase.Undo(result.Confidence);
        });
    }
}
