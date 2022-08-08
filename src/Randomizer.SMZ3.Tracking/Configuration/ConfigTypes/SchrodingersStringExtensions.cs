using System;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
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
            var names = new SchrodingersString();
            names.Add(area.Name);
            foreach (var name in area.AlsoKnownAs)
                names.Add(name);
            return names;
        }
    }
}
