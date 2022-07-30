﻿using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions
{
    /// <summary>
    /// Defines a region that offers a reward for completing it, e.g. a Zelda
    /// dungeon or a Super Metroid boss.
    /// </summary>
    public interface IHasReward
    {
        /// <summary>
        /// Gets or sets the reward for completing the region, e.g. pendant or
        /// crystal.
        /// </summary>
        ItemType Reward { get; set; }

        Item RewardItem { get; set; }

        /// <summary>
        /// Determines whether the reward for the region can be obtained.
        /// </summary>
        /// <param name="items">The items currently available.</param>
        /// <returns>
        /// <see langword="true"/> if the region can be completed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool CanComplete(Progression items);
    }
}
