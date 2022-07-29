﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Tracking.AutoTracking;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Module for creating the auto tracker and interacting with the auto tracker
    /// </summary>
    public class AutoTrackerModule : TrackerModule, IDisposable
    {
        private readonly ILogger<AutoTrackerModule> _logger;
        private AutoTracker _autoTracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTrackerModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to write logging information.</param>
        /// <param name="autoTracker">The auto tracker to associate with this module</param>
        public AutoTrackerModule(Tracker tracker, IItemService itemService, ILogger<AutoTrackerModule> logger, AutoTracker autoTracker)
            : base(tracker, itemService, logger)
        {
            _logger = logger;
            autoTracker.Tracker = tracker;
            Tracker.AutoTracker = autoTracker;
            _autoTracker = autoTracker;

            AddCommand("Look at this", GetLookAtGameRule(), (tracker, result) =>
            {
                LookAtGame();
            });
        }

        private GrammarBuilder GetLookAtGameRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please", "would you please")
                .OneOf("look at this", "look here", "record this", "log this", "take a look at this")
                .Optional("shit", "crap");
        }

        private void LookAtGame()
        {
            if (_autoTracker.LatestViewAction == null || _autoTracker.LatestViewAction.Invoke() == false)
            {
                Tracker.Say(x => x.AutoTracker.LookedAtNothing);
            }
        }

        /// <summary>
        /// Called when the module is destroyed
        /// </summary>
        public void Dispose() {
            _autoTracker.SetConnector(EmulatorConnectorType.None);
        }
    }

}
