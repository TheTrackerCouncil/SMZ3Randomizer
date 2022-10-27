using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Generation
{
    /// <summary>
    /// Class to create worlds for a specified generated rom
    /// </summary>
    public class Smz3GeneratedRomLoader
    {
        private readonly IWorldAccessor _worldAccessor;

        public Smz3GeneratedRomLoader(IWorldAccessor worldAccessor)
        {
            _worldAccessor = worldAccessor;
        }

        /// <summary>
        /// Creates a series of worls and sets up the WorldAccessor for
        /// the given GeneratedRom
        /// </summary>
        /// <param name="rom"></param>
        public void LoadGeneratedRom(GeneratedRom rom)
        {
            var trackerState = rom.TrackerState;

            var configs = Config.FromConfigString(rom.Settings);
            var worlds = new List<World>();

            foreach (var config in configs)
            {
                worlds.Add(new World(config, config.PlayerName, config.Id, config.PlayerGuid, config.Id == trackerState.LocalWorldId));
            }

            _worldAccessor.Worlds = worlds;
            _worldAccessor.World = worlds.First(x => x.IsLocalWorld);
        }
    }
}
