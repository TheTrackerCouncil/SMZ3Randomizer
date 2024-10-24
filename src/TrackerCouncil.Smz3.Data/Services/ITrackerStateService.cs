using System.Collections.Generic;
using System.Threading.Tasks;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.Services;

public interface ITrackerStateService
{
    public Task CreateStateAsync(IEnumerable<World> world, GeneratedRom generatedRom);

    public TrackerState CreateTrackerState(List<World> worlds);

    public Task SaveStateAsync(IEnumerable<World> worlds, GeneratedRom generatedRom, double secondsElapsed);

    public TrackerState? LoadState(IEnumerable<World> worlds, GeneratedRom generatedRom);
}
