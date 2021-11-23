using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Represents the currently active tracker configuration.
    /// </summary>
    public class TrackerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerConfig"/> class
        /// with the specified item data.
        /// </summary>
        /// <param name="items">The item data to use.</param>
        /// <param name="pegs">The peg data.</param>
        /// <param name="dungeons">The dungeon data.</param>
        /// <param name="responses">The responses to use.</param>
        /// <param name="requests">The basic requests and responses.</param>
        public TrackerConfig(IReadOnlyCollection<ItemData> items,
            IReadOnlyCollection<Peg> pegs,
            IReadOnlyCollection<DungeonInfo> dungeons,
            ResponseConfig responses,
            IReadOnlyCollection<BasicVoiceRequest> requests)
        {
            Items = items;
            Pegs = pegs;
            Dungeons = dungeons;
            Responses = responses;
            Requests = requests;
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        public IReadOnlyCollection<ItemData> Items { get; init; }

        /// <summary>
        /// Gets the peg world peg configuration.
        /// </summary>
        public IReadOnlyCollection<Peg> Pegs { get; init; }

        /// <summary>
        /// Gets a collection of Zelda dungeons.
        /// </summary>
        public IReadOnlyCollection<DungeonInfo> Dungeons { get; init; }

        /// <summary>
        /// Gets a collection of configured responses.
        /// </summary>
        public ResponseConfig Responses { get; init; }

        /// <summary>
        /// Gets a collection of basic requests and responses.
        /// </summary>
        public IReadOnlyCollection<BasicVoiceRequest> Requests { get; init; }
    }
}
