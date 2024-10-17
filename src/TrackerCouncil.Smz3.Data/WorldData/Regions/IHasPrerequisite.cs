using System;
using System.Linq;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions;

/// <summary>
/// Defines a region that requires an item (Bombos, Ether, Quake) to be
/// accessible.
/// </summary>
public interface IHasPrerequisite
{
    public string Name { get; }

    public RegionInfo Metadata { get; set; }

    public World World { get; }

    /// <summary>
    /// The required item type to enter this region
    /// </summary>
    ItemType RequiredItem
    {
        get => PrerequisiteState.RequiredItem;
        set => PrerequisiteState.RequiredItem = value;
    }

    /// <summary>
    /// The item marked by the player
    /// </summary>
    ItemType? MarkedItem
    {
        get => PrerequisiteState.MarkedItem;
        set
        {
            if (PrerequisiteState.MarkedItem == value)
            {
                return;
            }

            PrerequisiteState.MarkedItem = value;
            OnUpdatedPrerequisite();
        }
    }

    /// <summary>
    /// Gets the default medallion for the dungeon
    /// </summary>
    ItemType DefaultRequiredItem { get; }

    public TrackerPrerequisiteState PrerequisiteState { get; set; }

    public bool HasMarkedCorrectly => RequiredItem == MarkedItem;

    public void ApplyState(TrackerState? state)
    {
        var region = (Region)this;

        if (state == null)
        {
            PrerequisiteState = new TrackerPrerequisiteState
            {
                WorldId = region.World.Id, RegionName = GetType().Name, RequiredItem = DefaultRequiredItem
            };
            RequiredItem = DefaultRequiredItem;
        }
        else
        {
            PrerequisiteState = state.PrerequisiteStates.FirstOrDefault(x =>
                x.WorldId == region.World.Id && x.RegionName == GetType().Name) ?? new TrackerPrerequisiteState
            {
                WorldId = region.World.Id, RegionName = GetType().Name, RequiredItem = DefaultRequiredItem
            };
            RequiredItem = PrerequisiteState?.RequiredItem ?? DefaultRequiredItem;
        }
    }

    event EventHandler? UpdatedPrerequisite;

    void OnUpdatedPrerequisite();
}
