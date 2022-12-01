using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client.GameServices;

public class MultiworldGameService : MultiplayerGameTypeService, IDisposable
{
    private ITrackerStateService _trackerStateService;

    public MultiworldGameService(Smz3Randomizer randomizer, Smz3MultiplayerRomGenerator multiplayerRomGenerator,  MultiplayerClientService client, ITrackerStateService trackerStateService) : base(randomizer, multiplayerRomGenerator, client)
    {
        _trackerStateService = trackerStateService;
    }

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

    public override SeedData? RegenerateSeed(string seed, List<MultiplayerPlayerGenerationData> playerGenerationData,
        List<MultiplayerPlayerState> players, MultiplayerPlayerState localPlayer,
        out string error)
    {
        var generationConfigs = new List<Config>();
        foreach (var player in players)
        {
            var config = Config.FromConfigString(player.Config!).First();
            config.IsLocalConfig = player == localPlayer;
            config.Seed = seed;
            config.MultiplayerPlayerGenerationData = playerGenerationData.Single(x => x.WorldId == config.Id);
            generationConfigs.Add(config);
        }

        return RegenerateSeedInternal(generationConfigs, seed, out error);
    }

    public void Dispose()
    {
        EnableSync = false;
        GC.SuppressFinalize(this);
    }

    public override void OnTrackingStarted()
    {
        EnableSync = true;
        Task.Run(SyncPlayerState);
    }

    private bool EnableSync { get; set; }

    /// <summary>
    /// Send the local player's full state in case anything was missed
    /// </summary>
    private async Task SyncPlayerState()
    {
        while (EnableSync)
        {
            if (Client.LocalPlayer != null && TrackerState != null)
            {
                UpdatePlayerState(Client.LocalPlayer, TrackerState);
                await Client.UpdatePlayerState(Client.LocalPlayer);
            }

            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }
}
