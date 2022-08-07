using System;
using System.Collections.Generic;
using System.Threading;

namespace Randomizer.SMZ3.Contracts
{
    /// <summary>
    /// Defines an algorithm for filling items into one or more SMZ3 worlds.
    /// </summary>
    public interface IFiller
    {
        /// <summary>
        /// Specifies the random number generator to use when filling worlds.
        /// </summary>
        /// <param name="random">
        /// The pseudo-random number generator to use.
        /// </param>
        void SetRandom(Random random);

        /// <summary>
        /// Distributes items across locations in the specified worlds.
        /// </summary>
        /// <param name="worlds">The world or worlds to initialize.</param>
        /// <param name="config">The configuration to use.</param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        void Fill(List<World> worlds, Config config, CancellationToken cancellationToken);
    }
}
