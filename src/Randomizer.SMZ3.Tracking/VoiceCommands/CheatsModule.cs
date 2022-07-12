using System;
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

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Module for cheats via the auto tracker
    /// </summary>
    public class CheatsModule : TrackerModule
    {
        private readonly ILogger<AutoTrackerModule> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTrackerModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to write logging information.</param>
        public CheatsModule(Tracker tracker, ILogger<AutoTrackerModule> logger) : base(tracker, logger)
        {
            _logger = logger;

            AddCommand("Heal player", HealPlayerRule(), (tracker, result) =>
            {
                tracker.GameInteractor?.HealPlayer();
            });

            AddCommand("Give item", GiveItemRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out var itemName);
                tracker.GameInteractor?.GiveItem(item);
            });
        }

        private GrammarBuilder HealPlayerRule()
        {
            var restore = new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please", "would you please")
                .OneOf("restore my", "fill my")
                .OneOf("health", "hp", "energy", "hearts");

            var heal = new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please", "would you please")
                .OneOf("heal me");

            return GrammarBuilder.Combine(restore, heal);
        }

        private GrammarBuilder GiveItemRule()
        {
            var itemNames = GetItemNames(x => x.Name[0] != "Content");

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("give me", "lend me", "donate")
                .Optional("the", "a", "some")
                .Append(ItemNameKey, itemNames);
        }


    }

}
