using Google.Protobuf.Reflection;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

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
        Tracker.ItemTracker.TrackItem(item: item, trackedAs: null, confidence: null, tryClear: true, autoTracked: true, location: location);
        Logger.LogInformation("Auto tracked {ItemName} from {LocationName}", location.Item.Name, location.Name);
    }
}
