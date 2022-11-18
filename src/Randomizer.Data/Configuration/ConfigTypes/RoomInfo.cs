using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Randomizer.Data.WorldData;

namespace Randomizer.Data.Configuration.ConfigTypes
{
    /// <summary>
    /// Represents extra information about a room in SMZ3.
    /// </summary>
    public class RoomInfo : IMergeable<RoomInfo>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RoomInfo() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomInfo"/> class with
        /// the specified info.
        /// </summary>
        /// <param name="name">The possible names for the room.</param>
        public RoomInfo(SchrodingersString name)
        {
            Name = name;
        }

        /// <summary>
        /// Unique key to connect the RoomInfo with other configs
        /// </summary>
        [MergeKey]
        public string Room { get; init; } = "";

        /// <summary>
        /// The Randomzier.SMZ3 type matching this region
        /// </summary>

        public Type? Type { get; init; }

        /// <summary>
        /// Gets the possible names for the room.
        /// </summary>
        public SchrodingersString Name { get; set; } = new();

        /// <summary>
        /// Gets the possible hints for the room, if any are defined.
        /// </summary>
        public SchrodingersString? Hints { get; set; }

        /// <summary>
        /// Gets the phrases to reply with when the room is out of logic
        /// </summary>
        public SchrodingersString? OutOfLogic { get; set; }

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
            => world.Rooms.Single(x => x.GetType() == Type);

        /// <summary>
        /// Determines whether the room is accessible with the specified set of
        /// items.
        /// </summary>
        /// <param name="world">
        /// The instance of the world that contains the room.
        /// </param>
        /// <param name="progression">The available items.</param>
        /// <returns>
        /// <c>true</c> if the room is accessible; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAccessible(World world, Progression progression)
        {
            var room = GetRoom(world);
            return room.Locations.Any(x => x.IsAvailable(progression));
        }

        /// <summary>
        /// Returns a string representation of the room.
        /// </summary>
        /// <returns>A string representation of this room.</returns>
        public override string? ToString() => Name[0];
    }
}
