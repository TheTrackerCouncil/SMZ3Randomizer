using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.Abstractions;
using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.GameModes;

public class GameModeService : IGameModeService
{
    private readonly List<GameModeBase> _allGameModes;
    private List<GameModeBase> _trackerGameModes = new();
    private Dictionary<string, GameModeBase> _gameModeTypes;
    private TrackerBase? _tracker;

    public GameModeService(IServiceProvider serviceProvider)
    {
        _allGameModes = serviceProvider.GetServices<GameModeBase>().ToList();
        _gameModeTypes = _allGameModes.ToDictionary(x => x.Name, x => x);
    }

    public Dictionary<string, GameModeBase> GameModeTypes => _gameModeTypes;

    public void SetTracker(TrackerBase tracker, GameModeConfigs gameModeConfigs)
    {
        _trackerGameModes = GetConfigGameModes(gameModeConfigs).ToList();
        _tracker = tracker;
    }

    public void ModifyConfig(Config config)
    {
        foreach (var gameMode in GetConfigGameModes(config))
        {
            gameMode.ModifyWorldConfig(config);
        }
    }

    public void ModifyWorldItemPools(World world)
    {
        foreach (var gameMode in GetConfigGameModes(world.Config))
        {
            gameMode.ModifyWorldItemPools(world.ItemPools);
        }
    }

    public IEnumerable<RomPatch> GetPatches(World world)
    {
        return GetConfigGameModes(world.Config).SelectMany(x => x.GetPatches(world));
    }

    private IEnumerable<GameModeBase> GetConfigGameModes(Config config)
    {
        return GetConfigGameModes(config.GameModeConfigs);
    }

    private IEnumerable<GameModeBase> GetConfigGameModes(GameModeConfigs gameModeConfigs)
    {
        var enabledGameModes = gameModeConfigs.GetEnabledGameModes();
        return _allGameModes.Where(x => enabledGameModes.Contains(x.GameModeType));
    }

    public void ItemTracked(Item item, Location? location)
    {
        _trackerGameModes.ForEach(x => x.ItemTracked(item, location, _tracker));
    }

    public void DungeonCleared(IDungeon dungeon)
    {
        _trackerGameModes.ForEach(x => x.DungeonCleared(dungeon, _tracker));
    }

    public void BossDefeated(Boss boss)
    {
        _trackerGameModes.ForEach(x => x.BossDefeated(boss, _tracker));
    }

    public void ZeldaStateChanged(AutoTrackerZeldaState currentState, AutoTrackerZeldaState? previousState)
    {
        _trackerGameModes.ForEach(x => x.ZeldaStateChanged(currentState, previousState, _tracker));
    }

    public void MetroidStateChanged(AutoTrackerMetroidState currentState, AutoTrackerMetroidState? previousState)
    {
        _trackerGameModes.ForEach(x => x.MetroidStateChanged(currentState, previousState, _tracker));
    }

}
