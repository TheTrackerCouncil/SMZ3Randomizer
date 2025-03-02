using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PySpeechServiceClient.Grammar;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Module for cheats via the auto tracker
/// </summary>
public class CheatsModule(
    TrackerBase tracker,
    IPlayerProgressionService playerProgressionService,
    IWorldQueryService worldQueryService,
    ILogger<CheatsModule> logger) : TrackerModule(tracker,
    playerProgressionService,
    worldQueryService,
    logger)
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

    private bool PlayerCanCheat()
    {
        if (!TrackerBase.ModeTracker.CheatsEnabled)
        {
            TrackerBase.Say(x => x.Cheats.PromptEnableCheats);
            return false;
        }
        else if (TrackerBase.AutoTracker == null || !TrackerBase.AutoTracker.IsConnected || !TrackerBase.AutoTracker.IsInSMZ3)
        {
            TrackerBase.Say(x => x.Cheats.PromptEnableAutoTracker);
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
        if (!PlayerCanCheat() || TrackerBase.GameService == null) return;

        var successful = false;
        if (s_fillHealthChoices.Contains(fillType))
        {
            successful = TrackerBase.GameService.TryHealPlayer();
        }
        else if (s_fillMagicChoices.Contains(fillType))
        {
            successful = TrackerBase.GameService.TryFillMagic();
        }
        else if (s_fillBombsChoices.Contains(fillType))
        {
            successful = TrackerBase.GameService.TryFillZeldaBombs();
        }
        else if (s_fillArrowsChoices.Contains(fillType))
        {
            successful = TrackerBase.GameService.TryFillArrows();
        }
        else if (s_fillRupeesChoices.Contains(fillType))
        {
            successful = TrackerBase.GameService.TryFillRupees();
        }
        else if (s_fillMissilesChoices.Contains(fillType))
        {
            successful = TrackerBase.GameService.TryFillMissiles();
        }
        else if (s_fillSuperMissileChoices.Contains(fillType))
        {
            successful = TrackerBase.GameService.TryFillSuperMissiles();
        }
        else if (s_fillPowerBombsChoices.Contains(fillType))
        {
            successful = TrackerBase.GameService.TryFillPowerBombs();
        }

        if (successful)
        {
            TrackerBase.Say(x => x.Cheats.CheatPerformed);
        }
        else
        {
            TrackerBase.Say(x => x.Cheats.CheatFailed);
        }
    }

    private async Task GiveItemAsync(Item? item)
    {
        if (!PlayerCanCheat()) return;

        if (item == null || item.Type == ItemType.Nothing)
        {
            TrackerBase.Say(x => x.Cheats.CheatInvalidItem);
        }
        else if (TrackerBase.GameService != null)
        {
            var successful = await TrackerBase.GameService.TryGiveItemAsync(item, null);

            if (successful)
            {
                TrackerBase.Say(x => x.Cheats.CheatPerformed);
            }
            else
            {
                TrackerBase.Say(x => x.Cheats.CheatFailed);
            }
        }
    }

    private static SpeechRecognitionGrammarBuilder GetEnableCheatsRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("enable", "turn on")
            .OneOf("cheats", "cheat codes", "sv_cheats");
    }

    private static SpeechRecognitionGrammarBuilder GetDisableHintsRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("disable", "turn off")
            .OneOf("cheats", "cheat codes", "sv_cheats");
    }

    private static SpeechRecognitionGrammarBuilder FillRule()
    {
        var fillChoices = new List<GrammarKeyValueChoice>();
        fillChoices.AddRange(s_fillHealthChoices.Select(x => new GrammarKeyValueChoice(x)));
        fillChoices.AddRange(s_fillMagicChoices.Select(x => new GrammarKeyValueChoice(x)));
        fillChoices.AddRange(s_fillBombsChoices.Select(x => new GrammarKeyValueChoice(x)));
        fillChoices.AddRange(s_fillArrowsChoices.Select(x => new GrammarKeyValueChoice(x)));
        fillChoices.AddRange(s_fillRupeesChoices.Select(x => new GrammarKeyValueChoice(x)));
        fillChoices.AddRange(s_fillMissilesChoices.Select(x => new GrammarKeyValueChoice(x)));
        fillChoices.AddRange(s_fillSuperMissileChoices.Select(x => new GrammarKeyValueChoice(x)));
        fillChoices.AddRange(s_fillPowerBombsChoices.Select(x => new GrammarKeyValueChoice(x)));
        var restore = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please", "would you please")
            .OneOf("restore my", "fill my", "refill my")
            .Append(s_fillCheatKey, fillChoices);

        var heal = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please", "would you please")
            .OneOf("heal me", "I need healing");

        return SpeechRecognitionGrammarBuilder.Combine(restore, heal);
    }

    private SpeechRecognitionGrammarBuilder GiveItemRule()
    {
        var itemNames = GetItemNames(x => x.Name != "Content");

        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("give me", "lend me", "donate")
            .Optional("the", "a", "some")
            .Append(ItemNameKey, itemNames);
    }

    private SpeechRecognitionGrammarBuilder KillPlayerRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("kill me", "give me a tactical reset");
    }

    private SpeechRecognitionGrammarBuilder SetupCrystalFlashRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("setup crystal flash requirements", "ready a crystal flash");
    }

    private SpeechRecognitionGrammarBuilder ChargeShinesparkRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("charge", "ready", "give me", "enable", "activate", "turn on")
            .Optional("a")
            .Append("shine spark");
    }

    public override void AddCommands()
    {
        if (TrackerBase.World.Config.Race || TrackerBase.World.Config.DisableCheats) return;

        AddCommand("Enable cheats", GetEnableCheatsRule(), (result) =>
        {
            TrackerBase.ModeTracker.EnableCheats();
        });

        AddCommand("Disable cheats", GetDisableHintsRule(), (result) =>
        {
            TrackerBase.ModeTracker.DisableCheats();
        });

        AddCommand("Fill rule", FillRule(), (result) =>
        {
            var fillType = result.Semantics.ContainsKey(s_fillCheatKey) ? (string)result.Semantics[s_fillCheatKey].Value : s_fillHealthChoices.First();
            Fill(fillType);
        });

        if (TrackerBase.World.Config.RomGenerator == RomGenerator.Cas)
        {
            AddCommand("Give item", GiveItemRule(), (result) =>
            {
                var item = GetItemFromResult(TrackerBase, result, out var itemName);
                _ = GiveItemAsync(item);
            });
        }

        AddCommand("Kill player", KillPlayerRule(), (result) =>
        {
            if (!PlayerCanCheat()) return;

            if (TrackerBase.GameService?.TryKillPlayer() == true)
            {
                TrackerBase.Say(x => x.Cheats.KilledPlayer ?? x.Cheats.CheatPerformed);
            }
            else
            {
                TrackerBase.Say(x => x.Cheats.CheatFailed);
            }
        });

        AddCommand("Setup Crystal Flash", SetupCrystalFlashRule(), (result) =>
        {
            if (!PlayerCanCheat()) return;

            if (TrackerBase.GameService?.TrySetupCrystalFlash() == true)
            {
                TrackerBase.Say(x => x.Cheats.CheatPerformed);
            }
            else
            {
                TrackerBase.Say(x => x.Cheats.CheatFailed);
            }
        });

        AddCommand("Charge Shinespark", ChargeShinesparkRule(), (result) =>
        {
            if (!PlayerCanCheat()) return;

            if (TrackerBase.GameService?.TryChargeShinespark() == true)
            {
                TrackerBase.Say(x => x.Cheats.CheatPerformed);
            }
            else
            {
                TrackerBase.Say(x => x.Cheats.CheatFailed);
            }
        });
    }
}
