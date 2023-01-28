using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
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
        private readonly Dictionary<string, Progression> _progression = new();

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
        /// Finds the item with the specified name for the local player.
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
            => LocalPlayersItems().FirstOrDefault(x => x.Name == name)
            ?? LocalPlayersItems().FirstOrDefault(x => x.Metadata.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            ?? LocalPlayersItems().FirstOrDefault(x => x.Metadata.GetStage(name) != null);

        /// <summary>
        /// Finds an item with the specified item type for the local player.
        /// </summary>
        /// <param name="itemType">The type of item to find.</param>
        /// <returns>
        /// An <see cref="ItemData"/> representing the item. If there are
        /// multiple configured items with the same type, this method returns
        /// one at random. If there no configured items with the specified type,
        /// this method returns <see langword="null"/>.
        /// </returns>
        public Item? FirstOrDefault(ItemType itemType)
            => LocalPlayersItems().FirstOrDefault(x => x.Type == itemType);

        /// <summary>
        /// Indicates whether an item of the specified type has been tracked
        /// for the local player.
        /// </summary>
        /// <param name="itemType">The type of item to check.</param>
        /// <returns>
        /// <see langword="true"/> if an item with the specified type has been
        /// tracked at least once; otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool IsTracked(ItemType itemType)
            => LocalPlayersItems().Any(x => x.Type == itemType && x.State.TrackingState > 0);

        /// <summary>
        /// Enumerates all items that can be tracked for all players.
        /// </summary>
        /// <returns>A collection of items.</returns>
        public IEnumerable<Item> AllItems() // I really want to discourage this, but necessary for now
            => _world.Worlds.SelectMany(x => x.AllItems);

        /// <summary>
        /// Enumerates all items that can be tracked for the local player.
        /// </summary>
        /// <returns>A collection of items.</returns>
        public IEnumerable<Item> LocalPlayersItems()
            => _world.Worlds.SelectMany(x => x.AllItems).Where(x => x.World == _world.World);

        /// <summary>
        /// Enumarates all currently tracked items for the local player.
        /// </summary>
        /// <returns>
        /// A collection of items that have been tracked at least once.
        /// </returns>
        public IEnumerable<Item> TrackedItems()
            => LocalPlayersItems().Where(x => x.State.TrackingState > 0);

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
            return item?.Metadata.NameWithArticle ?? itemType.GetDescription();
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
            => LocalPlayersRewards().FirstOrDefault(x => x.Type == rewardType);

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
        /// Enumerates all rewards that can be tracked for all players.
        /// </summary>
        /// <returns>A collection of rewards.</returns>

        public virtual IEnumerable<Reward> AllRewards()
            => _world.Worlds.SelectMany(x => x.Rewards);

        /// <summary>
        /// Enumerates all rewards that can be tracked for the local player.
        /// </summary>
        /// <returns>A collection of rewards.</returns>

        public virtual IEnumerable<Reward> LocalPlayersRewards()
            => _world.World.Rewards;

        /// <summary>
        /// Enumarates all currently tracked rewards for the local player.
        /// This uses what the player marked as the reward for dungeons,
        /// not the actual dungeon reward.
        /// </summary>
        /// <returns>
        /// A collection of reward that have been tracked.
        /// </returns>
        public virtual IEnumerable<Reward> TrackedRewards()
            => _world.World.Dungeons.Where(x => x.HasReward && x.DungeonState.Cleared).Select(x => new Reward(x.MarkedReward, _world.World, (IHasReward)x));

        /// <summary>
        /// Enumerates all bosses that can be tracked for all players.
        /// </summary>
        /// <returns>A collection of bosses.</returns>

        public virtual IEnumerable<Boss> AllBosses()
            => _world.Worlds.SelectMany(x => x.AllBosses);

        /// <summary>
        /// Enumerates all bosses that can be tracked for the local player.
        /// </summary>
        /// <returns>A collection of bosses.</returns>

        public virtual IEnumerable<Boss> LocalPlayersBosses()
            => _world.World.AllBosses;

        /// <summary>
        /// Enumarates all currently tracked bosses for the local player.
        /// </summary>
        /// <returns>
        /// A collection of bosses that have been tracked.
        /// </returns>
        public virtual IEnumerable<Boss> TrackedBosses()
            => LocalPlayersBosses().Where(x => x.State.Defeated);

        /// <summary>
        /// Gets the current progression based on the items the user has collected,
        /// bosses that the user has beaten, and rewards that the user has received
        /// </summary>
        /// <param name="assumeKeys">If it should be assumed that the player has all keys</param>
        /// <returns>The progression object</returns>
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
                progression.AddRange(_world.World.ItemPools.Keycards);
                if (assumeKeys)
                    progression.AddRange(_world.World.ItemPools.Dungeon);
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

        /// <summary>
        /// Clears the progression cache after collecting new items, rewards, or bosses
        /// </summary>
        public void ResetProgression()
        {
            _progression.Clear();
        }


        // TODO: Tracking methods
    }
}
