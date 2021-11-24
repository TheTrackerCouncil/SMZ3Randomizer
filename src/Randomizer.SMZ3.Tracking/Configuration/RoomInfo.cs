using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        /// Gets the possible hints for the room, if any are defined.
        /// </summary>
        public SchrodingersString? Hints { get; init; }

        /// <summary>
        /// Returns the locations in the room.
        /// </summary>
        /// <param name="world">
        /// The instance of the world whose locations to return.
        /// </param>
        /// <returns>
        /// A collection of locations in this room from the specified world.
        /// </returns>
        public IReadOnlyCollection<Location> GetLocations(World world)
        {
            var room = GetRoom(world);
            return room.Locations.ToImmutableList();
        }

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
        public Room GetRoom(World world)
            => world.Rooms.Single(x => x.GetType().FullName == TypeName);

        /// <summary>
        /// Determines whether the room is accessible with the
        /// specified set of items.
        /// </summary>
        /// <param name="world">
        /// The instance of the world that contains the room.
        /// </param>
        /// <param name="progression">The available items.</param>
        /// <returns>
        /// <c>true</c> if the room is accessible; otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool IsAccessible(World world, Progression progression)
        {
            var room = GetRoom(world);
            return room.Locations.Any(x => x.IsAvailable(progression));
        }
    }
}
