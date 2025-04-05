using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
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
    /// <param name="metadataService"></param>
    public HintTileModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<HintTileModule> logger, IGameHintService gameHintService, IMetadataService metadataService) : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        _gameHintService = gameHintService;
        _hintTileConfig = metadataService.HintTiles;
    }

    /// <summary>
    /// Adds the voice commands
    /// </summary>
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

    private List<GrammarKeyValueChoice> GetHintTileNames()
    {
        var hintTileNames = new List<GrammarKeyValueChoice>();

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
                hintTileNames.Add(new GrammarKeyValueChoice(name.Text, hintTile.HintTileKey));
        }

        return hintTileNames;
    }

    private SpeechRecognitionGrammarBuilder GetHintTileRules()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Append("what does the")
            .Append(_hintTileKey, GetHintTileNames())
            .OneOf("hint tile", "telepathic tile")
            .OneOf("say", "state");
    }

    private (HintTile HintTile, Shared.Models.PlayerHintTile PlayerHintTile) GetHintTileFromResult(SpeechRecognitionResult result)
    {
        var key = (string)result.Semantics[_hintTileKey].Value;
        var hintTile = _hintTileConfig.HintTiles?.FirstOrDefault(x => x.HintTileKey == key) ??
                       throw new Exception($"Could not find hint tile {key}");
        var playerHintTile = WorldQueryService.World.HintTiles.FirstOrDefault(x => x.HintTileCode == key) ??
                             throw new Exception($"Could not find player hint tile {key}");
        return (hintTile, playerHintTile);
    }
}
