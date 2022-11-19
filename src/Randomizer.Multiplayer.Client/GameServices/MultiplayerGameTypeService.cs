using Randomizer.Data.Options;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client.GameServices;

public abstract class MultiplayerGameTypeService
{
    public abstract List<MultiplayerPlayerState> CreateWorld(List<MultiplayerPlayerState> players);

    protected IEnumerable<Config> GetPlayerConfigs(List<MultiplayerPlayerState> players)
        => players.Select(x => Config.FromConfigString(x.Config!).First());
}
