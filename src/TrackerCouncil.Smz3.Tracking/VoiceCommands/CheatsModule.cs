using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

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
    /// Initializes a new instance of the <see cref="AutoTrackerVoiceModule"/>
    /// class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    public CheatsModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<CheatsModule> logger)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {

    }

    private bool PlayerCanCheat()
    {
        if (!_cheatsEnabled)
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

    [SupportedOSPlatform("windows")]
    private static GrammarBuilder GetEnableCheatsRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("enable", "turn on")
            .OneOf("cheats", "cheat codes", "sv_cheats");
    }

    [SupportedOSPlatform("windows")]
    private static GrammarBuilder GetDisableHintsRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("disable", "turn off")
            .OneOf("cheats", "cheat codes", "sv_cheats");
    }

    [SupportedOSPlatform("windows")]
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

    [SupportedOSPlatform("windows")]
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

    [SupportedOSPlatform("windows")]
    private GrammarBuilder KillPlayerRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("kill me", "give me a tactical reset");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder SetupCrystalFlashRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("setup crystal flash requirements", "ready a crystal flash");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder ChargeShinesparkRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("charge", "ready", "give me", "enable", "activate", "turn on")
            .Optional("a")
            .Append("shine spark");
    }

    [SupportedOSPlatform("windows")]
    public override void AddCommands()
    {
        if (TrackerBase.World.Config.Race || TrackerBase.World.Config.DisableCheats) return;

        AddCommand("Enable cheats", GetEnableCheatsRule(), (result) =>
        {
            if (!_cheatsEnabled)
            {
                _cheatsEnabled = true;
                TrackerBase.Say(x => x.Cheats.EnabledCheats);
            }
            else
            {
                TrackerBase.Say(x => x.Cheats.AlreadyEnabledCheats);
            }
        });

        AddCommand("Disable cheats", GetDisableHintsRule(), (result) =>
        {
            if (_cheatsEnabled)
            {
                _cheatsEnabled = false;
                TrackerBase.Say(x => x.Cheats.DisabledCheats);
            }
            else
            {
                TrackerBase.Say(x => x.Cheats.AlreadyDisabledCheats);
            }
        });

        AddCommand("Fill rule", FillRule(), (result) =>
        {
            var fillType = result.Semantics.ContainsKey(s_fillCheatKey) ? (string)result.Semantics[s_fillCheatKey].Value : s_fillHealthChoices.First();
            Fill(fillType);
        });

        AddCommand("Give item", GiveItemRule(), (result) =>
        {
            var item = GetItemFromResult(TrackerBase, result, out var itemName);
            _ = GiveItemAsync(item);
        });

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
