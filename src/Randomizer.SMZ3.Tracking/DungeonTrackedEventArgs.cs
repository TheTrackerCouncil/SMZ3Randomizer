using System;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides data for events that occur when tracking a dungeon.
    /// </summary>
    public class DungeonTrackedEventArgs : TrackerEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="DungeonTrackedEventArgs"/> class.
        /// </summary>
        /// <param name="dungeon">The dungeon that was tracked.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        /// <param name="autoTracked">If the location was automatically tracked</param>
        public DungeonTrackedEventArgs(IDungeon? dungeon, float? confidence, bool autoTracked)
            : base(confidence, autoTracked)
        {
            Dungeon = dungeon;
        }

        /// <summary>
        /// Gets the boss that was tracked.
        /// </summary>
        public IDungeon? Dungeon { get; }
    }
}
