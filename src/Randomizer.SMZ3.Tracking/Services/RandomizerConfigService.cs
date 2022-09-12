using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.SMZ3.Contracts;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for retrieving information about the current state of
    /// the world
    /// </summary>
    public class RandomizerConfigService : IRandomizerConfigService
    {
        private readonly IWorldAccessor _world;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="worldAccessor"></param>
        public RandomizerConfigService(IWorldAccessor worldAccessor)
        {
            _world = worldAccessor;
        }

        /// <summary>
        /// Retrieves the config of the current world
        /// </summary>
        public Config Config => _world.World.Config;
    }
}
