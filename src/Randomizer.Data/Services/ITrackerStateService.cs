using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Services
{
    public interface ITrackerStateService
    {
        public Task CreateStateAsync(IEnumerable<World> world, GeneratedRom generatedRom);

        public TrackerState CreateTrackerState(IEnumerable<World> worlds);

        public Task SaveStateAsync(IEnumerable<World> worlds, GeneratedRom generatedRom, double secondsElapsed);

        public TrackerState? LoadState(IEnumerable<World> worlds, GeneratedRom generatedRom);
    }
}
