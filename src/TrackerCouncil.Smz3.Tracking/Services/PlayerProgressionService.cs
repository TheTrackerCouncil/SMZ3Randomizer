using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Manages items and their tracking state.
/// </summary>
internal class PlayerProgressionService (IWorldAccessor world) : IPlayerProgressionService
{
    private IWorldAccessor _world => world;
    private readonly Dictionary<string, Progression> _progression = new();

    /// <summary>
    /// Enumarates all currently tracked items for the local player.
    /// </summary>
    /// <returns>
    /// A collection of items that have been tracked at least once.
    /// </returns>
    public IEnumerable<Item> TrackedItems()
        => _world.Worlds.SelectMany(x => x.AllItems).Where(x => x.World == _world.World && x.TrackingState > 0);

    public bool IsTracked(ItemType itemType) => _world.Worlds.SelectMany(x => x.AllItems)
        .FirstOrDefault(x => x.World == _world.World && x.Type == itemType)?.TrackingState > 0;

    public IEnumerable<Reward> TrackedRewards()
        => _world.World.Rewards.Where(x => x.HasReceivedReward);

    public IEnumerable<Boss> TrackedBosses()
        => _world.World.Bosses.Where(x => x.Defeated);

    /// <summary>
    /// Gets the current progression based on the items the user has collected,
    /// bosses that the user has beaten, and rewards that the user has received
    /// </summary>
    /// <param name="assumeKeys">If it should be assumed that the player has all keys</param>
    /// <returns>The progression object</returns>
    public Progression GetProgression(bool assumeKeys)
    {
        var key = $"{assumeKeys}";

        if (_progression.TryGetValue(key, out var prevProgression))
        {
            return prevProgression;
        }

        var progression = new Progression();

        if (!_world.World.Config.MetroidKeysanity || assumeKeys)
        {
            progression.AddRange(_world.World.ItemPools.Keycards);
            if (assumeKeys)
                progression.AddRange(_world.World.ItemPools.Dungeon);
        }

        foreach (var item in TrackedItems().Select(x => x.State).Distinct())
        {
            if (item.Type is null or ItemType.Nothing) continue;
            progression.AddRange(Enumerable.Repeat(item.Type.Value, item.TrackingState));
        }

        foreach (var reward in TrackedRewards())
        {
            progression.Add(reward);
        }

        foreach (var boss in TrackedBosses())
        {
            progression.Add(boss);
        }

        if (_world.World.Config.RomGenerator != RomGenerator.Cas)
        {
            progression.InitLegacyProgression();
            _world.World.UpdateLegacyWorld();
        }

        _progression[key] = progression;
        return progression;
    }

    /// <summary>
    /// Gets the current progression based on the items the user has collected,
    /// bosses that the user has beaten, and rewards that the user has received
    /// </summary>
    /// <param name="area">The area to check to see if keys should be assumed
    /// or not</param>
    /// <returns>The progression object</returns>
    public Progression GetProgression(IHasLocations area)
    {
        switch (area)
        {
            case Z3Region:
            case Room { Region: Z3Region }:
                return GetProgression(assumeKeys: !_world.World.Config.ZeldaKeysanity);
            case SMRegion:
            case Room { Region: SMRegion }:
                return GetProgression(assumeKeys: !_world.World.Config.MetroidKeysanity);
            default:
                return GetProgression(assumeKeys: _world.World.Config.KeysanityMode == KeysanityMode.None);
        }
    }

    public Progression GetProgression(Location location)
    {
        return location.Region is Z3Region
            ? GetProgression(assumeKeys: !_world.World.Config.ZeldaKeysanity)
            : GetProgression(assumeKeys: !_world.World.Config.MetroidKeysanity);
    }

    /// <summary>
    /// Clears the progression cache after collecting new items, rewards, or bosses
    /// </summary>
    public void ResetProgression()
    {
        _progression.Clear();
    }


    // TODO: Tracking methods
}
