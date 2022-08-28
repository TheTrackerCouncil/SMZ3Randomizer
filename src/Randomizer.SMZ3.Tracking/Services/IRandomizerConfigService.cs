using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for retrieving the randomizer generation config for the current world
    /// </summary>
    public interface IRandomizerConfigService
    {
        /// <summary>
        /// Retrieves the config
        /// </summary>
        public Config Config { get; }
    }
}
