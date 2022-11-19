using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client.GameServices;

public class MultiworldGameService : MultiplayerGameTypeService
{
    private Smz3Randomizer _randomizer;
    private ITrackerStateService _trackerStateService;

    public MultiworldGameService(Smz3Randomizer randomizer, ITrackerStateService trackerStateService)
    {
        _randomizer = randomizer;
        _trackerStateService = trackerStateService;
    }

    public override List<MultiplayerPlayerState> CreateWorld(List<MultiplayerPlayerState> players)
    {
        var configs = GetPlayerConfigs(players).ToList();
        var generationConfigs = new List<Config>();
        for (var i = 0; i < configs.Count; i++)
        {
            configs[i].Id = i;
            configs[i].GameMode = GameMode.Multiworld;
            generationConfigs.Add(configs[i].SeedOnly());
        }

        var seedData = _randomizer.GenerateSeed(generationConfigs, null);
        var worlds = seedData.Worlds.Select(x => x.World);

        var state = _trackerStateService.CreateTrackerState(worlds);

        foreach (var player in players)
        {
            var config = configs.Single(x => x.PlayerGuid == player.Guid);
            config.Seed = seedData.Seed;
            var world = worlds.Single(x => x.Guid == player.Guid);
            player.Locations = state.LocationStates.Where(x => x.WorldId == world.Id).ToList();
            player.Items = state.ItemStates.Where(x => x.WorldId == world.Id).ToList();
            player.Bosses = state.BossStates.Where(x => x.WorldId == world.Id).ToList();
            player.Dungeons = state.DungeonStates.Where(x => x.WorldId == world.Id).ToList();
            player.Config = Config.ToConfigString(config);
        }

        return players;
    }
}
