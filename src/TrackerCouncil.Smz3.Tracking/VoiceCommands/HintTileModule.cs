using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

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
    /// <param name="playerProgressionService"></param>
    /// <param name="worldQueryService"></param>
    /// <param name="logger"></param>
    /// <param name="gameHintService"></param>
    /// <param name="configs"></param>
    public HintTileModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<HintTileModule> logger, IGameHintService gameHintService, Configs configs) : base(tracker, playerProgressionService, worldQueryService, logger)
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
            if (WorldQueryService.World.HintTiles.Any())
            {
                var hintTile = GetHintTileFromResult(result);
                var text = _gameHintService.GetHintTileText(hintTile.PlayerHintTile, WorldQueryService.World, WorldQueryService.Worlds);
                TrackerBase.Say(response: _hintTileConfig.RequestedHintTile, args: [text]);
                TrackerBase.GameStateTracker.UpdateHintTile(hintTile.PlayerHintTile);
            }
            else
            {
                TrackerBase.Say(response: _hintTileConfig.NoHintTiles);
            }

        });
    }

    [SupportedOSPlatform("windows")]
    private Choices GetHintTileNames()
    {
        var hintTileNames = new Choices();

        if (_hintTileConfig.HintTiles == null)
        {
            return hintTileNames;
        }

        foreach (var hintTile in _hintTileConfig.HintTiles)
        {
            if (hintTile.Name == null)
            {
                continue;
            }
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
    private (HintTile HintTile, Shared.Models.PlayerHintTile PlayerHintTile) GetHintTileFromResult(RecognitionResult result)
    {
        var key = (string)result.Semantics[_hintTileKey].Value;
        var hintTile = _hintTileConfig.HintTiles?.FirstOrDefault(x => x.HintTileKey == key) ??
                       throw new Exception($"Could not find hint tile {key}");
        var playerHintTile = WorldQueryService.World.HintTiles.FirstOrDefault(x => x.HintTileCode == key) ??
                             throw new Exception($"Could not find player hint tile {key}");
        return (hintTile, playerHintTile);
    }
}
