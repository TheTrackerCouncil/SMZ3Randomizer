using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client.GameServices;

public class MultiworldGameService : MultiplayerGameTypeService
{
    private Smz3Randomizer _randomizer;

    public MultiworldGameService(Smz3Randomizer randomizer)
    {
        _randomizer = randomizer;
    }

    public override List<MultiplayerPlayerState> CreateWorld(List<MultiplayerPlayerState> players)
    {
        var configs = GetPlayerConfigs(players).ToList();
        for (var i = 0; i < configs.Count; i++)
        {
            configs[i].Id = i;
        }

        var seedData = _randomizer.GenerateSeed(configs, null);
        var worlds = seedData.Worlds.Select(x => x.World);
        return players;
    }
}
