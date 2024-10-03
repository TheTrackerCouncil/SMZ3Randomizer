using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions;

/// <summary>
/// Defines a region that has treasure (tray-sure) in it
/// </summary>
public interface IHasTreasure
{
    public string Name { get; }

    public RegionInfo Metadata { get; set; }

    public World World { get; }

    /// <summary>
    /// The current tracking state of the treasures
    /// </summary>
    TrackerTreasureState TreasureState { get; set; }

    /// <summary>
    /// Gets the total treasure of this region
    /// </summary>
    public int TotalTreasure => TreasureState.TotalTreasure;

    public bool HasManuallyClearedTreasure
    {
        get => TreasureState.HasManuallyClearedTreasure;
        set => TreasureState.HasManuallyClearedTreasure = value;
    }

    /// <summary>
    /// Gets or sets the amount of remaining treasure in this region
    /// </summary>
    public int RemainingTreasure
    {
        get => TreasureState.RemainingTreasure;
        set => TreasureState.RemainingTreasure = value;
    }

    /// <summary>
    /// Calculates the number of treasures in the dungeon
    /// </summary>
    /// <returns></returns>
    public int GetTreasureCount()
    {
        var region = (Region)this;
        return region.Locations.Count(x => x.Item.Type != ItemType.Nothing && (!x.Item.IsDungeonItem || region.World.Config.ZeldaKeysanity) && x.Type != LocationType.NotInDungeon);
    }

    public void ApplyState(TrackerState? state)
    {
        var region = (Region)this;

        if (state == null)
        {
            var totalTreasure = GetTreasureCount();
            TreasureState = new TrackerTreasureState()
            {
                WorldId = region.World.Id, RegionName = GetType().Name, TotalTreasure = totalTreasure, RemainingTreasure = totalTreasure
            };
        }
        else
        {
            TreasureState = state.TreasureStates.First(x =>
                x.WorldId == region.World.Id && x.RegionName == GetType().Name);
        }
    }
}
