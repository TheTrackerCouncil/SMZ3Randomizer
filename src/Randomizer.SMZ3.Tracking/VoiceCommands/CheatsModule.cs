using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Module for cheats via the auto tracker
    /// </summary>
    public class CheatsModule : TrackerModule
    {
        private static readonly string s_fillCheatKey = "FillType";
        private static readonly List<string> s_fillHealthChoices = new() { "health", "hp", "energy", "hearts" };
        private static readonly List<string> s_fillMagicChoices = new() { "magic", "mana", "magic meter" };
        private static readonly List<string> s_fillBombsChoices = new() { "bombs", "zelda bombs" };
        private static readonly List<string> s_fillArrowsChoices = new() { "arrows", "sticks" };
        private static readonly List<string> s_fillRupeesChoices = new() { "rupees", "money" };
        private static readonly List<string> s_fillMissilesChoices = new() { "missiles" };
        private static readonly List<string> s_fillSuperMissileChoices = new() { "super missiles", "soup" };
        private static readonly List<string> s_fillPowerBombsChoices = new() { "power bombs", "hamburgers" };

        private bool _cheatsEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTrackerModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">Used to write logging information.</param>
        public CheatsModule(Tracker tracker, IItemService itemService, IWorldService worldService, ILogger<AutoTrackerModule> logger)
            : base(tracker, itemService, worldService, logger)
        {
            if (tracker.World.Config.Race || tracker.World.Config.DisableCheats) return;

            AddCommand("Enable cheats", GetEnableCheatsRule(), (result) =>
            {
                _cheatsEnabled = true;
                Tracker.Say(x => x.Cheats.EnabledCheats);
            });

            AddCommand("Disable cheats", GetDisableHintsRule(), (result) =>
            {
                _cheatsEnabled = false;
                Tracker.Say(x => x.Cheats.DisabledCheats);
            });

            AddCommand("Fill rule", FillRule(), (result) =>
            {
                var fillType = result.Semantics.ContainsKey(s_fillCheatKey) ? (string)result.Semantics[s_fillCheatKey].Value : s_fillHealthChoices.First();
                Fill(fillType);
            });

            AddCommand("Give item", GiveItemRule(), (result) =>
            {
                var item = GetItemFromResult(tracker, result, out var itemName);
                GiveItem(item);
            });

            AddCommand("Kill player", KillPlayerRule(), (result) =>
            {
                if (!PlayerCanCheat()) return;

                if (Tracker.GameService?.TryKillPlayer() == true)
                {
                    Tracker.Say(x => x.Cheats.CheatPerformed);
                }
                else
                {
                    Tracker.Say(x => x.Cheats.CheatFailed);
                }
            });
        }

        private bool PlayerCanCheat()
        {
            if (!_cheatsEnabled)
            {
                Tracker.Say(x => x.Cheats.PromptEnableCheats);
                return false;
            }
            else if (Tracker.AutoTracker == null || !Tracker.AutoTracker.IsConnected || !Tracker.AutoTracker.IsInSMZ3)
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
            if (!PlayerCanCheat() || Tracker.GameService == null) return;

            var successful = false;
            if (s_fillHealthChoices.Contains(fillType))
            {
                successful = Tracker.GameService.TryHealPlayer();
            }
            else if (s_fillMagicChoices.Contains(fillType))
            {
                successful = Tracker.GameService.TryFillMagic();
            }
            else if (s_fillBombsChoices.Contains(fillType))
            {
                successful = Tracker.GameService.TryFillZeldaBombs();
            }
            else if (s_fillArrowsChoices.Contains(fillType))
            {
                successful = Tracker.GameService.TryFillArrows();
            }
            else if (s_fillRupeesChoices.Contains(fillType))
            {
                successful = Tracker.GameService.TryFillRupees();
            }
            else if (s_fillMissilesChoices.Contains(fillType))
            {
                successful = Tracker.GameService.TryFillMissiles();
            }
            else if (s_fillSuperMissileChoices.Contains(fillType))
            {
                successful = Tracker.GameService.TryFillSuperMissiles();
            }
            else if (s_fillPowerBombsChoices.Contains(fillType))
            {
                successful = Tracker.GameService.TryFillPowerBombs();
            }

            if (successful)
            {
                Tracker.Say(x => x.Cheats.CheatPerformed);
            }
            else
            {
                Tracker.Say(x => x.Cheats.CheatFailed);
            }
        }

        private void GiveItem(Item? item)
        {
            if (!PlayerCanCheat()) return;

            if (item == null || item.Type == ItemType.Nothing)
            {
                Tracker.Say(x => x.Cheats.CheatInvalidItem);
            }
            else if (Tracker.GameService?.TryGiveItem(item, null) == true)
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
            fillChoices.Add(s_fillMagicChoices.ToArray());
            fillChoices.Add(s_fillBombsChoices.ToArray());
            fillChoices.Add(s_fillArrowsChoices.ToArray());
            fillChoices.Add(s_fillRupeesChoices.ToArray());
            fillChoices.Add(s_fillMissilesChoices.ToArray());
            fillChoices.Add(s_fillSuperMissileChoices.ToArray());
            fillChoices.Add(s_fillPowerBombsChoices.ToArray());
            var restore = new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please", "would you please")
                .OneOf("restore my", "fill my", "refill my")
                .Append(s_fillCheatKey, fillChoices);

            var heal = new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please", "would you please")
                .OneOf("heal me", "I need healing");

            return GrammarBuilder.Combine(restore, heal);
        }

        private GrammarBuilder GiveItemRule()
        {
            var itemNames = GetItemNames(x => x.Name != "Content");

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("give me", "lend me", "donate")
                .Optional("the", "a", "some")
                .Append(ItemNameKey, itemNames);
        }

        private GrammarBuilder KillPlayerRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("kill me", "give me a tactical reset");
        }
    }

}
