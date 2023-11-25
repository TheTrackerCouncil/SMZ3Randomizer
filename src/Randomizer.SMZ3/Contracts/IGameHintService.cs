using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;

namespace Randomizer.SMZ3.Contracts
{
    /// <summary>
    /// Service for generating hints for the player
    /// </summary>
    public interface IGameHintService
    {
        /// <summary>
        /// Retrieves the hints to display in game for the player
        /// </summary>
        /// <param name="hintPlayerWorld">The player that will be receiving the hints</param>
        /// <param name="allWorlds">All worlds that are a part of the seed</param>
        /// <param name="playthrough">The initial playthrough with all of the spheres</param>
        /// <param name="seed">Seed number for shuffling and randomization</param>
        /// <returns>A collection of strings to use for the in game hints</returns>
        void GetInGameHints(World hintPlayerWorld, List<World> allWorlds, Playthrough playthrough, int seed);

        /// <summary>
        /// Retrieves the usefulness of a particular location in a seed
        /// </summary>
        /// <param name="location">The location to check</param>
        /// <param name="allWorlds">All worlds that are a part of the seed</param>
        /// <param name="playthrough">The initial playthrough with all of the spheres</param>
        /// <returns>How useful the location is</returns>
        LocationUsefulness GetLocationUsefulness(Location location, List<World> allWorlds,
            Playthrough playthrough);

        /// <summary>
        /// Retrieves the text that could be displayed on one of the hint tiles in a player's
        /// world
        /// </summary>
        /// <param name="tile">The hint tile to get the text for</param>
        /// <param name="hintPlayerWorld">The world which the hint tile is contained in</param>
        /// <param name="worlds">All of the worlds in the game</param>
        /// <returns>The text that could be displayed on the hint tile</returns>
        string? GetHintTileText(PlayerHintTile tile, World hintPlayerWorld, List<World> worlds);
    }
}
