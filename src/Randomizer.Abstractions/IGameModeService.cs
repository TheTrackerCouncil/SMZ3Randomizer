using Microsoft.Extensions.DependencyInjection;
using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.Abstractions;

public interface IGameModeService
{
    public Dictionary<string, GameModeBase> GameModeTypes { get; }

    public void SetTracker(TrackerBase tracker, GameModeConfigs gameModeConfigs);

    public void ModifyConfig(Config config);

    public void ModifyWorldItemPools(World world);

    public IEnumerable<RomPatch> GetPatches(World world);

    public void ItemTracked(Item item, Location? location);

    public void DungeonCleared(IDungeon dungeon);

    public void BossDefeated(Boss boss);

    public void ZeldaStateChanged(AutoTrackerZeldaState currentState, AutoTrackerZeldaState? previousState);

    public void MetroidStateChanged(AutoTrackerMetroidState currentState, AutoTrackerMetroidState? previousState);
}
