using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Enums;
using SnesConnectorLibrary;

namespace Randomizer.SMZ3.Tracking.AutoTracking.AutoTrackerModules;

public abstract class AutoTrackerModule(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger logger)
{
    public AutoTrackerBase AutoTracker { get; set; } = null!;
    protected TrackerBase Tracker => tracker;
    protected ISnesConnectorService SnesConnector => snesConnector;
    protected ILogger Logger => logger;

    public abstract void Initialize();

    protected bool HasStartedGame => AutoTracker.HasStarted;

    protected bool HasValidState => AutoTracker.HasValidState;

    protected bool IsInZelda => AutoTracker.CurrentGame == Game.Zelda;

    protected bool IsInMetroid => AutoTracker.CurrentGame == Game.SM;

    protected bool IsInGame => AutoTracker.CurrentGame is Game.Zelda or Game.SM or Game.Credits;

    protected void TrackLocation(Location location)
    {
        var item = location.Item;
        location.State.Autotracked = true;
        Tracker.TrackItem(item: item, trackedAs: null, confidence: null, tryClear: true, autoTracked: true, location: location);
        Logger.LogInformation("Auto tracked {ItemName} from {LocationName}", location.Item.Name, location.Name);
    }
}
