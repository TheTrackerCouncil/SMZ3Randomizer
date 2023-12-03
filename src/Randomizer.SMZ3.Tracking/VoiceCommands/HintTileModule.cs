using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands;

/// <summary>
/// Voice module for hint tiles
/// </summary>
public class HintTileModule : TrackerModule
{
    private readonly IGameHintService _gameHintService;
    private readonly HintTileConfig _hintTileConfig;
    private readonly string _hintTileKey = "HintTileKey";

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tracker"></param>
    /// <param name="itemService"></param>
    /// <param name="worldService"></param>
    /// <param name="logger"></param>
    /// <param name="gameHintService"></param>
    /// <param name="configs"></param>
    public HintTileModule(TrackerBase tracker, IItemService itemService, IWorldService worldService, ILogger<HintTileModule> logger, IGameHintService gameHintService, Configs configs) : base(tracker, itemService, worldService, logger)
    {
        _gameHintService = gameHintService;
        _hintTileConfig = configs.HintTileConfig;
    }

    /// <summary>
    /// Adds the voice commands
    /// </summary>
    [SupportedOSPlatform("windows")]
    public override void AddCommands()
    {
        AddCommand("Hint Tile", GetHintTileRules(), (result) =>
        {
            if (WorldService.World.HintTiles.Any())
            {
                var hintTile = GetHintTileFromResult(result);
                var text = _gameHintService.GetHintTileText(hintTile.PlayerHintTile, WorldService.World, WorldService.Worlds);
                TrackerBase.Say(_hintTileConfig.RequestedHintTile, text);
                TrackerBase.UpdateHintTile(hintTile.PlayerHintTile);
            }
            else
            {
                TrackerBase.Say(_hintTileConfig.NoHintTiles);
            }

        });

        AddCommand("Clear Hint Tile", GetClearHintTileRules(), (result) =>
        {
            var hintTile = TrackerBase.LastViewedHintTile;
            if (hintTile?.State == null)
            {
                TrackerBase.Say(_hintTileConfig.NoPreviousHintTile);
            }
            else if (hintTile.State.HintState != HintState.Cleared && hintTile.Locations?.Count() > 0)
            {
                var locations = hintTile.Locations.Select(x => WorldService.World.FindLocation(x))
                    .Where(x => x.State is { Cleared: false, Autotracked: false }).ToList();
                if (locations.Any())
                {
                    TrackerBase.Clear(locations, result.Confidence);
                    hintTile.State.HintState = HintState.Cleared;
                    TrackerBase.UpdateHintTile(hintTile);
                }
                else
                {
                    TrackerBase.Say(_hintTileConfig.ClearHintTileFailed);
                }
            }
            else
            {
                TrackerBase.Say(_hintTileConfig.ClearHintTileFailed);
            }
        });
    }

    [SupportedOSPlatform("windows")]
    private Choices GetHintTileNames()
    {
        var hintTileNames = new Choices();

        foreach (var hintTile in _hintTileConfig.HintTiles)
        {
            foreach (var name in hintTile.Name)
                hintTileNames.Add(new SemanticResultValue(name.Text, hintTile.HintTileKey));
        }

        return hintTileNames;
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetHintTileRules()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .Append("what does the")
            .Append(_hintTileKey, GetHintTileNames())
            .OneOf("hint tile", "telepathic tile")
            .OneOf("say", "state");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetClearHintTileRules()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("clear that", "ignore that", "I don't care about", "I don't give a shit about",
                "I don't give a fuck about")
            .OneOf("hint tile", "telepathic tile");
    }

    [SupportedOSPlatform("windows")]
    private (HintTile HintTile, PlayerHintTile PlayerHintTile) GetHintTileFromResult(RecognitionResult result)
    {
        var key = (string)result.Semantics[_hintTileKey].Value;
        var hintTile = _hintTileConfig.HintTiles.FirstOrDefault(x => x.HintTileKey == key) ??
                       throw new Exception($"Could not find hint tile {key}");
        var playerHintTile = WorldService.World.HintTiles.FirstOrDefault(x => x.HintTileCode == key) ??
                             throw new Exception($"Could not find player hint tile {key}");
        return (hintTile, playerHintTile);
    }
}
