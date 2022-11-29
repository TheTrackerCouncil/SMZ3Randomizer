﻿using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client.GameServices;

public class MultiworldGameService : MultiplayerGameTypeService
{
    private ITrackerStateService _trackerStateService;

    public MultiworldGameService(Smz3Randomizer randomizer, MultiplayerClientService client, ITrackerStateService trackerStateService) : base(randomizer, client)
    {
        _trackerStateService = trackerStateService;
    }

    public override SeedData? GenerateSeed(List<MultiplayerPlayerState> players, out string error)
    {
        var generationConfigs = new List<Config>();
        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            player.WorldId = i;
            var config = Config.FromConfigString(player.Config!).First();
            config.Id = i;
            config.GameMode = GameMode.Multiworld;
            player.Config = Config.ToConfigString(config);
            generationConfigs.Add(config.SeedOnly());
        }

        return GenerateSeedInternal(generationConfigs, null, out error);
    }

    public override SeedData? RegenerateSeed(List<MultiplayerPlayerState> players, MultiplayerPlayerState localPlayer,
        string seed, out string error)
    {
        var generationConfigs = new List<Config>();
        foreach (var player in players)
        {
            var config = Config.FromConfigString(player.Config!).First();
            config.IsLocalConfig = player == localPlayer;
            config.Seed = seed;
            generationConfigs.Add(config);
        }

        return GenerateSeedInternal(generationConfigs, seed, out error);
    }
}