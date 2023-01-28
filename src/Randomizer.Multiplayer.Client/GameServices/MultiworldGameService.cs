using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client.GameServices;


/// <summary>
/// Service for handling multiworld games
/// </summary>
public class MultiworldGameService : MultiplayerGameTypeService
{
    private ITrackerStateService _trackerStateService;
    private ILogger<MultiworldGameService> _logger;

    public MultiworldGameService(Smz3Randomizer randomizer, Smz3MultiplayerRomGenerator multiplayerRomGenerator,  MultiplayerClientService client, ITrackerStateService trackerStateService, ILogger<MultiworldGameService> logger) : base(randomizer, multiplayerRomGenerator, client)
    {
        _trackerStateService = trackerStateService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a multiworld seed
    /// </summary>
    /// <param name="seed">Seed number</param>
    /// <param name="players">The states for each of the players</param>
    /// <param name="localPlayer">The state for the local player</param>
    /// <param name="error">Error from generating the seed</param>
    /// <returns>The seed data object with all of the world and patch details</returns>
    public override SeedData? GenerateSeed(string seed, List<MultiplayerPlayerState> players,
        MultiplayerPlayerState localPlayer, out string error)
    {

        var generationConfigs = new List<Config>();
        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            player.WorldId = i;
            var config = Config.FromConfigString(player.Config!).First();
            config.Id = i;
            config.GameMode = GameMode.Multiworld;
            config.IsLocalConfig = player == localPlayer;
            player.Config = Config.ToConfigString(config);
            generationConfigs.Add(config);
        }

        return GenerateSeedInternal(generationConfigs, seed, out error);
    }

    /// <summary>
    /// Regenerates a multiworld seed based on the generation data sent from the host
    /// </summary>
    /// <param name="seed">Seed number</param>
    /// <param name="playerGenerationData">List of all of the data needed to generate each of the worlds locally</param>
    /// <param name="players">List of player states</param>
    /// <param name="localPlayer">The locla player's state</param>
    /// <param name="error">Error from regenerating the seed</param>
    /// <returns>The seed data object with all of the world and patch details</returns>
    public override SeedData? RegenerateSeed(string seed, List<MultiplayerPlayerGenerationData> playerGenerationData,
        List<MultiplayerPlayerState> players, MultiplayerPlayerState localPlayer,
        out string error)
    {
        var generationConfigs = new List<Config>();
        foreach (var player in players)
        {
            var config = Config.FromConfigString(player.Config!).First();
            config.Id = player.WorldId!.Value;
            config.GameMode = GameMode.Multiworld;
            config.IsLocalConfig = player == localPlayer;
            config.Seed = seed;
            config.MultiplayerPlayerGenerationData = playerGenerationData.Single(x => x.WorldId == config.Id);
            generationConfigs.Add(config);
        }

        return RegenerateSeedInternal(generationConfigs, seed, out error);
    }

    public override void OnTrackingStarted() {  }

}
