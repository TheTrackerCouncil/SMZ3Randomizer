using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Speech.Recognition;
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
        private static readonly string s_fillCheatKey = "FillType";
        private static readonly List<string> s_fillHealthChoices = new() { "health", "hp", "energy", "hearts" };

        private readonly ILogger<AutoTrackerModule> _logger;
        private bool _cheatsEnabled = false;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTrackerModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to write logging information.</param>
        public CheatsModule(Tracker tracker, ILogger<AutoTrackerModule> logger) : base(tracker, logger)
        {
            _logger = logger;

            AddCommand("Enable cheats", GetEnableCheatsRule(), (tracker, result) =>
            {
                _cheatsEnabled = true;
                Tracker.Say(x => x.Cheats.EnabledCheats);
            });

            AddCommand("Disable cheats", GetDisableHintsRule(), (tracker, result) =>
            {
                _cheatsEnabled = false;
                Tracker.Say(x => x.Cheats.DisabledCheats);
            });

            AddCommand("Fill rule", FillRule(), (tracker, result) =>
            {
                var fillType = result.Semantics.ContainsKey(s_fillCheatKey) ? (string)result.Semantics[s_fillCheatKey].Value : s_fillHealthChoices.First();
                Fill(fillType);
            });

            AddCommand("Give item", GiveItemRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out var itemName);
                GiveItem(item);
            });
        }

        private bool PlayerCanCheat()
        {
            if (!_cheatsEnabled)
            {
                Tracker.Say(x => x.Cheats.PromptEnableCheats);
                return false;
            }
            else if (Tracker.AutoTracker == null || !Tracker.AutoTracker.IsConnected)
            {
                Tracker.Say(x => x.Cheats.PromptEnableAutoTracker);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Fills different types of pools for the player (health, bombs, missiles, etc.)
        /// </summary>
        /// <param name="fillType">What should be filled</param>
        private void Fill(string fillType)
        {
            if (!PlayerCanCheat()) return;

            if (s_fillHealthChoices.Contains(fillType) && Tracker.GameService?.TryHealPlayer() == true)
            {
                Tracker.Say(x => x.Cheats.CheatPerformed);
            }
            else
            {
                Tracker.Say(x => x.Cheats.CheatFailed);
            }
        }

        private void GiveItem(ItemData? item)
        {
            if (!PlayerCanCheat()) return;

            if (item != null && Tracker.GameService?.TryGiveItem(item) == true)
            {
                Tracker.Say(x => x.Cheats.CheatPerformed);
            }
            else
            {
                Tracker.Say(x => x.Cheats.CheatFailed);
            }
        }

        private static GrammarBuilder GetEnableCheatsRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("enable", "turn on")
                .OneOf("cheats", "cheat codes", "sv_cheats");
        }

        private static GrammarBuilder GetDisableHintsRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("disable", "turn off")
                .OneOf("cheats", "cheat codes", "sv_cheats");
        }

        private static GrammarBuilder FillRule()
        {
            var fillChoices = new Choices();
            fillChoices.Add(s_fillHealthChoices.ToArray());
            var restore = new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please", "would you please")
                .OneOf("restore my", "fill my")
                .Append(s_fillCheatKey, fillChoices);

            var heal = new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please", "would you please")
                .OneOf("heal me", "I need healing");

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
