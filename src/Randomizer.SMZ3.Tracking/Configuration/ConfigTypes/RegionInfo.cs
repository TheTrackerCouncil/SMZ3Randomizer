using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    /// <summary>
    /// Represents extra information about a region in SMZ3.
    /// </summary>
    public class RegionInfo : IMergeable<RegionInfo>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RegionInfo() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionInfo"/> class
        /// with the specified info.
        /// </summary>
        /// <param name="name">The possible names for the region.</param>
        /// <param name="mapName">The map name to display for the region.</param>
        public RegionInfo(SchrodingersString name, string mapName)
        {
            Name = name;
            MapName = mapName;
        }

        /// <summary>
        /// Unique key to connect the Region with other configs
        /// </summary>
        [MergeKey]
        public string Region { get; set; }

        /// <summary>
        /// Gets Randomizer.SMZ3 type for the region
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets the possible names for the region.
        /// </summary>
        public SchrodingersString Name { get; set; }

        /// <summary>
        /// Gets the possible hints for the region, if any are defined.
        /// </summary>
        public SchrodingersString? Hints { get; set; }

        /// <summary>
        /// The name of the map to display for this region
        /// </summary>
        public string MapName { get; init; }

        /// <summary>
        /// Returns the <see cref="Region"/> that matches the region info in the
        /// specified world.
        /// </summary>
        /// <param name="world">The world to find the region in.</param>
        /// <returns>
        /// A matching <see cref="Region"/> for the current region info.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// There is no matching region in <paramref name="world"/>. -or- There
        /// is more than one matching region in <paramref name="world"/>.
        /// </exception>
        public Region GetRegion(World world)
            => world.Regions.Single(x => x.GetType() == Type);

        /// <summary>
        /// Returns a string representation of the region.
        /// </summary>
        /// <returns>A string representation of this region.</returns>
        public override string? ToString() => Name[0];

        /// <summary>
        /// Text for Tracker to say when dying in a room or screen in the region
        /// </summary>
        public Dictionary<int, SchrodingersString>? WhenDiedInRoom { get; init; }

        /// <summary>
        /// Gets the phrases to reply with when the location is out of logic
        /// </summary>
        public SchrodingersString? OutOfLogic { get; set; }
    }
}
