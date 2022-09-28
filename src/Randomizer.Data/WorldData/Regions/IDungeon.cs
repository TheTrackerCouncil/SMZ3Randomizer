using System.Linq;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions
{
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

        TrackerDungeonState DungeonState { get; set; }

        public int GetTreasureCount()
        {
            var region = (Region)this;
            return region.Locations.Count(x => x.Item != null && (!x.Item.IsDungeonItem || region.World.Config.ZeldaKeysanity) && x.Type != LocationType.NotInDungeon);
        }

        public string DungeonName => ((Region)this).Name;

        public Reward? Reward => HasReward ? ((IHasReward)this).Reward : null;

        public RewardType RewardType => Reward?.Type ?? RewardType.None;

        public RewardType MarkedReward
        {
            get
            {
                return DungeonState?.MarkedReward ?? RewardType.None;
            }
            set
            {
                DungeonState.MarkedReward = value;
            }
        }

        public bool HasReward => this is IHasReward;

        public bool NeedsMedallion => this is INeedsMedallion;

        public ItemType Medallion => NeedsMedallion ? ((INeedsMedallion)this).Medallion : ItemType.Nothing;

        public ItemType MarkedMedallion
        {
            get
            {
                return DungeonState?.MarkedMedallion ?? ItemType.Nothing;
            }
            set
            {
                DungeonState.MarkedMedallion = value;
            }
        }
    }
}
