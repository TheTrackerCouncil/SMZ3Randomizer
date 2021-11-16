using System;
using System.Linq;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Represents extra information about a room in SMZ3.
    /// </summary>
    public class RoomInfo : IPointOfInterest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomInfo"/> class with
        /// the specified info.
        /// </summary>
        /// <param name="typeName">The fully qualified name of the room.</param>
        /// <param name="name">The possible names for the room.</param>
        public RoomInfo(string typeName, SchrodingersString name)
        {
            TypeName = typeName;
            Name = name;
        }

        /// <summary>
        /// Gets the fully qualified name of the room, e.g.
        /// <c>Randomizer.SMZ3.Regions.Zelda.LightWorld.LightWorldNorthWest.LightWorldNorthWest</c>.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the possible names for the room.
        /// </summary>
        public SchrodingersString Name { get; }

        /// <summary>
        /// Gets the x-coordinate of the room on the map, if it should be
        /// displayed.
        /// </summary>
        public int? X { get; init; }

        /// <summary>
        /// Gets the y-coordinate of the room on the map, if it should be
        /// displayed.
        /// </summary>
        public int? Y { get; init; }

        /// <summary>
        /// Gets the scale of the room, if it should be displayed on a map.
        /// </summary>
        public double? Scale { get; init; }

        /// <summary>
        /// Returns the <see cref="Room"/> that matches the room info in the
        /// specified world.
        /// </summary>
        /// <param name="world">The world to find the room in.</param>
        /// <returns>
        /// A matching <see cref="Room"/> for the current room info.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// There is no matching room in <paramref name="world"/>. -or- There is
        /// more than one matching room in <paramref name="world"/>.
        /// </exception>
        public Room GetRegion(World world)
            => world.Rooms.Single(x => x.GetType().FullName == TypeName);
    }
}
