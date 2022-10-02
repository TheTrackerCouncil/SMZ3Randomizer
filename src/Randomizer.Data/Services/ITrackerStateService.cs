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
        public Task CreateStateAsync(World world, GeneratedRom generatedRom);

        public Task SaveStateAsync(World world, GeneratedRom generatedRom, double secondsElapsed);

        public TrackerState? LoadState(World world, GeneratedRom generatedRom);
    }
}
