using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared.Enums;

namespace Randomizer.Abstractions;

public abstract class GameModeBase
{
    public abstract GameModeType GameModeType { get; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public virtual void ModifyWorldConfig(Config config)
    {
        Console.WriteLine($"Modify world config for{Name}");
    }

    public virtual void ModifyWorldItemPools(WorldItemPools itemPool)
    {
        Console.WriteLine($"Modify item pool for {Name}");
    }

    public virtual void PreWorldGeneration()
    {

    }

    public virtual ICollection<RomPatch> GetPatches(World world)
    {
        return new List<RomPatch>();
    }

    public virtual void ItemTracked(Item item, Location? location, TrackerBase? tracker)
    {

    }

    public virtual void DungeonCleared(IDungeon dungeon, TrackerBase? tracker)
    {

    }

    public virtual void BossDefeated(Boss boss, TrackerBase? tracker)
    {

    }

    public virtual void ZeldaStateChanged(AutoTrackerZeldaState currentState, AutoTrackerZeldaState? previousState, TrackerBase? tracker)
    {

    }

    public virtual void MetroidStateChanged(AutoTrackerMetroidState currentState, AutoTrackerMetroidState? previousState, TrackerBase? tracker)
    {

    }
}
