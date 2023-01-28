using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// This class represents a config for displaying a map of the tracker state
    /// </summary>
    public class TrackerMapConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMapConfig"/> class
        /// with data to create a map to represent the state of the tracker
        /// </summary>
        /// <param name="maps">List of all of the possible maps that are selectable by the player</param>
        /// <param name="regions">List of all of the different regions and their locations that can be a part of the maps</param>
        public TrackerMapConfig(IReadOnlyCollection<TrackerMap> maps, IReadOnlyCollection<TrackerMapRegion> regions)
        {
            Maps = maps;
            Regions = regions;
        }

        /// <summary>
        /// Collection of all of the maps that the user can select and the
        /// regions that appear on those maps
        /// </summary>
        public IReadOnlyCollection<TrackerMap> Maps { get; init; }

        /// <summary>
        /// Collection of all regions that can be referenced in a map and the locations
        /// that make up that region
        /// </summary>
        public IReadOnlyCollection<TrackerMapRegion> Regions { get; init; }
    }
}
