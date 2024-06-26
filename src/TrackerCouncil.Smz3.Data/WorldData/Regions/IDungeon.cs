using System.Linq;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions;

/// <summary>
/// Defines a region that offers a reward for completing it, e.g. a Zelda
/// dungeon or a Super Metroid boss.
/// </summary>
public interface IDungeon
{
    /// <summary>
    /// Gets or sets the reward for completing the region, e.g. pendant or
    /// crystal.
    /// </summary>
    DungeonInfo DungeonMetadata { get; set; }

    /// <summary>
    /// The current tracking state of the dungeon
    /// </summary>
    TrackerDungeonState DungeonState { get; set; }

    /// <summary>
    /// Calculates the number of treasures in the dungeon
    /// </summary>
    /// <returns></returns>
    public int GetTreasureCount()
    {
        var region = (Region)this;
        return region.Locations.Count(x => x.Item.Type != ItemType.Nothing && (!x.Item.IsDungeonItem || region.World.Config.ZeldaKeysanity) && x.Type != LocationType.NotInDungeon);
    }

    /// <summary>
    /// Retrieves the base name of the dungeon
    /// </summary>
    public string DungeonName => ((Region)this).Name;

    /// <summary>
    /// The reward object for the dungeon, if any
    /// </summary>
    public Reward? DungeonReward => HasReward ? ((IHasReward)this).Reward : null;

    /// <summary>
    /// The type of reward in the dungeon, if any
    /// </summary>
    public RewardType? DungeonRewardType => DungeonReward?.Type;

    /// <summary>
    /// If the dungeon has a pendant in it or not
    /// </summary>
    public bool IsPendantDungeon => DungeonRewardType is RewardType.PendantGreen or RewardType.PendantRed or RewardType.PendantBlue;

    /// <summary>
    /// If the dungeon has a crystal in it or not
    /// </summary>
    public bool IsCrystalDungeon => DungeonRewardType is RewardType.CrystalBlue or RewardType.CrystalRed;

    /// <summary>
    /// The MSU song index for the dungeon
    /// </summary>
    public int SongIndex { get; init; }

    public string Abbreviation { get; }

    public LocationId? BossLocationId { get; }

    /// <summary>
    /// The reward marked by the player
    /// </summary>
    public RewardType MarkedReward
    {
        get
        {
            return DungeonState.MarkedReward ?? RewardType.None;
        }
        set
        {
            if (DungeonState != null)
            {
                DungeonState.MarkedReward = value;
            }
        }
    }

    /// <summary>
    /// If this dungeon has a reward in it
    /// </summary>
    public bool HasReward => this is IHasReward;

    /// <summary>
    /// If this dungeon needs a medallion
    /// </summary>
    public bool NeedsMedallion => this is INeedsMedallion;

    /// <summary>
    /// The medallion required to entered the dungeon, if any
    /// </summary>
    public ItemType Medallion => NeedsMedallion ? ((INeedsMedallion)this).Medallion : ItemType.Nothing;

    /// <summary>
    /// The required medallion marked by the player
    /// </summary>
    public ItemType MarkedMedallion
    {
        get
        {
            return DungeonState.MarkedMedallion ?? ItemType.Nothing;
        }
        set
        {
            if (DungeonState != null)
            {
                DungeonState.MarkedMedallion = value;
            }
        }
    }

    /// <summary>
    /// The overworld region that this dungeon is within
    /// </summary>
    public Region ParentRegion { get; }
}
