using System;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using PySpeechServiceClient.Grammar;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Module for creating the auto tracker and interacting with the auto tracker
/// </summary>
public class AutoTrackerVoiceModule : TrackerModule, IDisposable
{
    private readonly AutoTrackerBase _autoTrackerBase;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoTrackerVoiceModule"/>
    /// class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    /// <param name="autoTrackerBase">The auto tracker to associate with this module</param>
    public AutoTrackerVoiceModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<AutoTrackerVoiceModule> logger, AutoTrackerBase autoTrackerBase)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        TrackerBase.AutoTracker = autoTrackerBase;
        _autoTrackerBase = autoTrackerBase;
    }

    private SpeechRecognitionGrammarBuilder GetLookAtGameRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please", "would you please")
            .OneOf("look at this", "look here", "record this", "log this", "take a look at this", "get a load of this")
            .Optional("shit", "crap");
    }

    private void LookAtGame()
    {
        if (_autoTrackerBase.LatestViewAction == null || _autoTrackerBase.LatestViewAction.Invoke() == false)
        {
            TrackerBase.Say(x => x.AutoTracker.LookedAtNothing);
        }
    }

    /// <summary>
    /// Called when the module is destroyed
    /// </summary>
    public void Dispose()
    {
        _autoTrackerBase.Dispose();
    }

    public override void AddCommands()
    {
        AddCommand("Look at this", GetLookAtGameRule(), (result) =>
        {
            LookAtGame();
        });
    }
}
