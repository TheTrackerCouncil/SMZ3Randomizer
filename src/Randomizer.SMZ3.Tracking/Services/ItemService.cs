using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.SMZ3.Contracts;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Manages items and their tracking state.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IWorldAccessor _world;
        private Dictionary<string, Progression> _progression = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class
        /// with the specified dependencies.
        /// </summary>
        /// <param name="items">
        /// Specifies the configuration that contains the item data to be
        /// managed.
        /// </param>
        /// <param name="rewards">
        /// Specifies the configuration that contains the reward data
        /// </param>
        /// <param name="world">Accessor to get data of the world</param>
        public ItemService(ItemConfig items, RewardConfig rewards, IWorldAccessor world)
        {
            Items = items;
            Rewards = rewards;
            _world = world;
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        protected IReadOnlyCollection<ItemData> Items { get; }

        /// <summary>
        /// Gets a collection of rewards
        /// </summary>
        protected IReadOnlyCollection<RewardInfo> Rewards { get; }

        /// <summary>
        /// Finds the item with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the item or item stage to find.
        /// </param>
        /// <returns>
        /// An <see cref="ItemData"/> representing the item with the specified
        /// name, or <see langword="null"/> if there is no item that has the
        /// specified name.
        /// </returns>
        public Item? FirstOrDefault(string name)
            => AllItems().FirstOrDefault(x => x.Name == name)
            ?? AllItems().FirstOrDefault(x => x.Metadata != null && x.Metadata.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            ?? AllItems().FirstOrDefault(x => x.Metadata != null && x.Metadata.GetStage(name) != null);

        /// <summary>
        /// Finds an item with the specified item type.
        /// </summary>
        /// <param name="itemType">The type of item to find.</param>
        /// <returns>
        /// An <see cref="ItemData"/> representing the item. If there are
        /// multiple configured items with the same type, this method returns
        /// one at random. If there no configured items with the specified type,
        /// this method returns <see langword="null"/>.
        /// </returns>
        public Item? FirstOrDefault(ItemType itemType)
            => AllItems().FirstOrDefault(x => x.Type == itemType);

        /// <summary>
        /// Indicates whether an item of the specified type has been tracked.
        /// </summary>
        /// <param name="itemType">The type of item to check.</param>
        /// <returns>
        /// <see langword="true"/> if an item with the specified type has been
        /// tracked at least once; otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool IsTracked(ItemType itemType)
            => AllItems().Any(x => x.Type == itemType && x.State.TrackingState > 0);

        /// <summary>
        /// Enumerates all items that can be tracked.
        /// </summary>
        /// <returns>A collection of items.</returns>
        public IEnumerable<Item> AllItems() // I really want to discourage this, but necessary for now
            => _world.World.AllItems;

        /// <summary>
        /// Enumarates all currently tracked items.
        /// </summary>
        /// <returns>
        /// A collection of items that have been tracked at least once.
        /// </returns>
        public IEnumerable<Item> TrackedItems()
            => AllItems().Where(x => x.State.TrackingState > 0);

        /// <summary>
        /// Returns a random name for the specified item including article, e.g.
        /// "an E-Tank" or "the Book of Mudora".
        /// </summary>
        /// <param name="itemType">The type of item whose name to get.</param>
        /// <returns>
        /// The name of the type of item, including "a", "an" or "the" if
        /// applicable.
        /// </returns>
        public virtual string GetName(ItemType itemType)
        {
            var item = FirstOrDefault(itemType);
            return item?.Metadata?.NameWithArticle ?? itemType.GetDescription();
        }


        /// <summary>
        /// Finds an reward with the specified item type.
        /// </summary>
        /// <param name="rewardType">The type of reward to find.</param>
        /// <returns>
        /// An <see cref="RewardInfo"/> representing the reward. If there are
        /// multiple configured rewards with the same type, this method returns
        /// one at random. If there no configured rewards with the specified type,
        /// this method returns <see langword="null"/>.
        /// </returns>
        public virtual Reward? FirstOrDefault(RewardType rewardType)
            => AllRewards().FirstOrDefault(x => x.Type == rewardType);

        /// <summary>
        /// Returns a random name for the specified item including article, e.g.
        /// "a blue crystal" or "the green pendant".
        /// </summary>
        /// <param name="rewardType">The reward of item whose name to get.</param>
        /// <returns>
        /// The name of the reward of item, including "a", "an" or "the" if
        /// applicable.
        /// </returns>
        public virtual string GetName(RewardType rewardType)
        {
            var reward = FirstOrDefault(rewardType);
            return reward?.Metadata.NameWithArticle ?? rewardType.GetDescription();
        }

        /// <summary>
        /// Enumerates all rewards that can be tracked.
        /// </summary>
        /// <returns>A collection of rewards.</returns>

        public virtual IEnumerable<Reward> AllRewards()
            => _world.World.Rewards;

        /// <summary>
        /// Enumarates all currently tracked rewards.
        /// This uses what the player marked as the reward for dungeons,
        /// not the actual dungeon reward.
        /// </summary>
        /// <returns>
        /// A collection of reward that have been tracked.
        /// </returns>
        public virtual IEnumerable<Reward> TrackedRewards()
            => _world.World.Dungeons.Where(x => x.DungeonState.Cleared).Select(x => new Reward(x.MarkedReward));

        /// <summary>
        /// Enumerates all bosses that can be tracked.
        /// </summary>
        /// <returns>A collection of bosses.</returns>

        public virtual IEnumerable<Boss> AllBosses()
            => _world.World.AllBosses;

        /// <summary>
        /// Enumarates all currently tracked bosses.
        /// </summary>
        /// <returns>
        /// A collection of bosses that have been tracked.
        /// </returns>
        public virtual IEnumerable<Boss> TrackedBosses()
            => AllBosses().Where(x => x.State?.Defeated == true);

        public Progression GetProgression(bool assumeKeys)
        {
            var key = $"{assumeKeys}";

            if (_progression.ContainsKey(key))
            {
                return _progression[key];
            }

            var progression = new Progression();

            if (!_world.World.Config.MetroidKeysanity || assumeKeys)
            {
                progression.AddRange(Item.CreateKeycards(_world.World));
                if (assumeKeys)
                    progression.AddRange(Item.CreateDungeonPool(_world.World));
            }

            foreach (var item in TrackedItems().Select(x => x.State).Distinct())
            {
                if (item.Type == null || item.Type == ItemType.Nothing) continue;
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

            _progression[key] = progression;
            return progression;
        }

        public Progression GetProgression(IHasLocations area)
        {
            if (area is Z3Region || (area is Room room1 && room1.Region is Z3Region))
                return GetProgression(assumeKeys: !_world.World.Config.ZeldaKeysanity);
            else if (area is SMRegion || (area is Room room2 && room2.Region is SMRegion))
                return GetProgression(assumeKeys: !_world.World.Config.MetroidKeysanity);
            else
                return GetProgression(assumeKeys: _world.World.Config.KeysanityMode == KeysanityMode.None);
        }

        public void ResetProgression()
        {
            _progression.Clear();
        }


        // TODO: Tracking methods
    }
}
