using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.GameModes;

public class GameModeWorldService(GameModeFactory gameModeFactory)
{
    public World UpdateWorld(World world, int seed)
    {
        if (world.Config.GameModeOptions.SelectedGameModeType == GameModeType.None)
        {
            return world;
        }

        var gameMode = gameModeFactory.GetGameMode(world.Config.GameModeOptions.SelectedGameModeType);
        gameMode.UpdateWorld(world, seed);
        return world;
    }
}
