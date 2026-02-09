using TrackerCouncil.Smz3.Data.WorldData;

namespace TrackerCouncil.Smz3.Abstractions;

public abstract class GameModeBase
{
    public virtual void UpdateWorld(World world, int seed)
    {
    }
}
