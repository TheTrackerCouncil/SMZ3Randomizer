using System;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.Data.Options;
using SnesConnectorLibrary;

namespace Randomizer.SMZ3.Tracking.VoiceCommands;

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
    /// <param name="itemService">Service to get item information</param>
    /// <param name="worldService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    /// <param name="autoTrackerBase">The auto tracker to associate with this module</param>
    public AutoTrackerVoiceModule(TrackerBase tracker, IItemService itemService, IWorldService worldService, ILogger<AutoTrackerVoiceModule> logger, AutoTrackerBase autoTrackerBase)
        : base(tracker, itemService, worldService, logger)
    {
        TrackerBase.AutoTracker = autoTrackerBase;
        _autoTrackerBase = autoTrackerBase;
    }

    private GrammarBuilder GetLookAtGameRule()
    {
        return new GrammarBuilder()
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
        _autoTrackerBase.SetConnector(new SnesConnectorSettings());
    }

    [SupportedOSPlatform("windows")]
    public override void AddCommands()
    {
        AddCommand("Look at this", GetLookAtGameRule(), (result) =>
        {
            LookAtGame();
        });
    }
}
