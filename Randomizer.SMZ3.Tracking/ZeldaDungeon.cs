using System;
using System.Linq;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Represents a dungeon in A Link to the Past.
    /// </summary>
    public class ZeldaDungeon
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZeldaDungeon"/> class.
        /// </summary>
        /// <param name="name">The name of the dungeon.</param>
        /// <param name="abbreviation">
        /// The abbreviation of the dungeon name.
        /// </param>
        /// <param name="boss">The name of the boss.</param>
        public ZeldaDungeon(SchrodingersString name, string abbreviation, SchrodingersString boss)
        {
            Name = name;
            Abbreviation = abbreviation;
            Boss = boss;
        }

        /// <summary>
        /// Gets the possible names of the dungeon.
        /// </summary>
        public SchrodingersString Name { get; }

        /// <summary>
        /// Gets the dungeon name abbreviation.
        /// </summary>
        public string Abbreviation { get; }

        /// <summary>
        /// Gets the possible names of the dungeon boss.
        /// </summary>
        public SchrodingersString Boss { get; init; }

        /// <summary>
        /// Gets or sets the zero-based of the column in which the tracker
        /// should display the dungeon.
        /// </summary>
        public int? Column { get; set; }

        /// <summary>
        /// Gets or sets the zero-based of the row in which the tracker should
        /// display the dungeon.
        /// </summary>
        public int? Row { get; set; }

        /// <summary>
        /// Gets or sets the type of pendant or crystal you are rewarded with
        /// when you beat the dungeon boss.
        /// </summary>
        public RewardItem Reward { get; set; }
            = RewardItem.Unknown;

        /// <summary>
        /// Gets or sets the medallion that is required to enter the dungeon.
        /// </summary>
        public Medallion Requirement { get; set; }
            = Medallion.None;

        /// <summary>
        /// Gets or sets the amount of treasure items (excluding keys, compasses
        /// and maps) that remain in the dungeon.
        /// </summary>
        public int TreasureRemaining { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dungeon has been
        /// cleared.
        /// </summary>
        public bool Cleared { get; set; }

        /// <summary>
        /// Returns a string representation of the dungeon.
        /// </summary>
        /// <returns>A string representing the dungeon.</returns>
        public override string ToString() => Name[0];

        /// <summary>
        /// Determines whether the specified region represents the dungeon.
        /// </summary>
        /// <param name="region">The region to check.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="region"/> matches the dungeon;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Is(Region region)
            => Name.Contains(region.Name, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the region that represents this dungeon in the specified
        /// <see cref="World"/>.
        /// </summary>
        /// <param name="world">The world those regions to find.</param>
        /// <returns>
        /// The <see cref="Region"/> in <paramref name="world"/> that matches
        /// this dungeon.
        /// </returns>
        public Region GetRegion(World world)
            => world.Regions.Single(Is);
    }
}
