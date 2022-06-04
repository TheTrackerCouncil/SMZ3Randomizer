﻿using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Provides the phrases for chat integration.
    /// </summary>
    public class AutoTrackerConfig
    {
        /// <summary>
        /// Gets the phrases to respond with when connected to to emulator.
        /// </summary>
        public SchrodingersString WhenConnected { get; init; }
            = new("Auto tracker connected");

        /// <summary>
        /// Gets the phrases to respond with when the game is started.
        /// </summary>
        public SchrodingersString GameStarted { get; init; }
            = new("Incoming PB for seed {0}");

        /// <summary>
        /// Gets the phrases to respond with when nearing KAD
        /// </summary>
        public SchrodingersString NearKraidsAwfulSon { get; init; }
            = new("Oh no. Disaster imminent.");

        /// <summary>
        /// Gets the phrases to respond with when nearing shaktool
        /// </summary>
        public SchrodingersString NearShaktool { get; init; }
            = new("Finally, the moment we've all been waiting for since the start of this run. We get to see Shaktool.");

        /// <summary>
        /// Gets the phrases to respond with when nearing crocomire
        /// </summary>
        public SchrodingersString NearCrocomire { get; init; }
            = new("You currently have {0} out of {1} super missiles. Do you think you have enough?");

        /// <summary>
        /// Gets the phrases to respond with when falling down the pit from moldorm
        /// </summary>
        public SchrodingersString FallFromMoldorm { get; init; }
            = new("Ha ha. Moldorm strikes again. Content increased by one step.");

        /// <summary>
        /// Gets the phrases to respond with when falling down the pit from moldorm
        /// </summary>
        public SchrodingersString FallFromGanon { get; init; }
            = new("Oops. Don't worry, I'm sure no one saw that.");

        /// <summary>
        /// Gets the phrases to respond with when performing Hera Pot tech
        /// </summary>
        public SchrodingersString HeraPot { get; init; }
            = new("Good job on the tech. Was it a first try?");

        /// <summary>
        /// Gets the phrases to respond with when performing Ice Breaker tech
        /// </summary>
        public SchrodingersString IceBreaker { get; init; }
            = new("Good job on the tech. Was it a first try?");

        /// <summary>
        /// Gets the phrases to respond with when performing Diver Down tech
        /// </summary>
        public SchrodingersString DiverDown { get; init; }
            = new("Good job on the tech. Was it a first try?");

        /// <summary>
        /// Gets the phrases to respond with when entering Hyrule Castle tower
        /// </summary>
        public SchrodingersString EnterHyruleCastleTower { get; init; }
            = new("Ouch. So it's come to this, then?");

        /// <summary>
        /// Gets the phrases to respond with when entering a pendant dungeon
        /// </summary>
        public SchrodingersString EnterPendantDungeon { get; init; }
            = new("Ouch. So it's come to this, then?");

    }
}
