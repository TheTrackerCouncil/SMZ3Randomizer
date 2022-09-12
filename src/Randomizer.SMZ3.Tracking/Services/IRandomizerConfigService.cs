using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;

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
