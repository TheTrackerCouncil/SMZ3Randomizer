using System;
using System.Collections.Generic;
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

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
        /// <param name="responses">The responses to use.</param>
        /// <param name="requests">The basic requests and responses.</param>
        public TrackerConfig(IReadOnlyCollection<ItemData> items,
            IReadOnlyCollection<Peg>? pegs,
            ResponseConfig? responses,
            IReadOnlyCollection<BasicVoiceRequest>? requests)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Pegs = pegs ?? new List<Peg>();
            Responses = responses ?? new ResponseConfig();
            Requests = requests ?? new List<BasicVoiceRequest>();
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        public IReadOnlyCollection<ItemData> Items { get; }

        /// <summary>
        /// Gets the peg world peg configuration.
        /// </summary>
        public IReadOnlyCollection<Peg> Pegs { get; }

        /// <summary>
        /// Gets a collection of configured responses.
        /// </summary>
        public ResponseConfig Responses { get; }

        /// <summary>
        /// Gets a collection of basic requests and responses.
        /// </summary>
        public IReadOnlyCollection<BasicVoiceRequest> Requests { get; }
    }
}
