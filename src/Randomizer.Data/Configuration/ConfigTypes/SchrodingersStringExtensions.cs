using System;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.Data.Configuration.ConfigTypes
{
    /// <summary>
    /// Provides functionality for the <see cref="SchrodingersString"/> class.
    /// </summary>
    public static class SchrodingersStringExtensions
    {
        /// <summary>
        /// Gets the possible names of the area.
        /// </summary>
        /// <param name="area">The area whose names to get.</param>
        /// <returns>
        /// A new <see cref="SchrodingersString"/> representing the possible
        /// names of the <paramref name="area"/>.
        /// </returns>
        public static SchrodingersString GetName(this IHasLocations area)
        {
            if (area is IDungeon dungeon)
            {
                return dungeon.DungeonMetadata.Name;
            }
            else if (area is Region region)
            {
                return region.Metadata.Name;
            }
            else if (area is Room room)
            {
                return room.Metadata.Name;
            }
            else
            {
                var names = new SchrodingersString();
                names.Add(area.Name);
                foreach (var name in area.AlsoKnownAs)
                    names.Add(name);
                return names;
            }
        }
    }
}
