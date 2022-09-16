using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Temporary class for housing special logic for locations
    /// </summary>
    public class TrackerLogic
    {
        private readonly Tracker _tracker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tracker"></param>
        public TrackerLogic(Tracker tracker)
        {
            _tracker = tracker;
            TrackerLocationLogic = new()
            {
                // Mimic Cave
                { 269, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                // Sahasrahla
                { 300, (items) => CountReward(items, RewardType.PendantGreen) == 1 },
                // Pyramid Fairy
                { 336, (items) => CountReward(items, RewardType.CrystalRed) == 2 },
                { 337, (items) => CountReward(items, RewardType.CrystalRed) == 2 },
                // Misery Mire
                { 425, (items) => CheckDungeonMedallion(items, _tracker.World.MiseryMire) },
                { 426, (items) => CheckDungeonMedallion(items, _tracker.World.MiseryMire) },
                { 427, (items) => CheckDungeonMedallion(items, _tracker.World.MiseryMire) },
                { 428, (items) => CheckDungeonMedallion(items, _tracker.World.MiseryMire) },
                { 429, (items) => CheckDungeonMedallion(items, _tracker.World.MiseryMire) },
                { 430, (items) => CheckDungeonMedallion(items, _tracker.World.MiseryMire) },
                { 431, (items) => CheckDungeonMedallion(items, _tracker.World.MiseryMire) },
                { 432, (items) => CheckDungeonMedallion(items, _tracker.World.MiseryMire) },
                // Turtle Rock
                { 433, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 434, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 435, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 436, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 437, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 438, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 439, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 440, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 441, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 442, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 443, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
                { 444, (items) => CheckDungeonMedallion(items, _tracker.World.TurtleRock) },
            };
        }

        /// <summary>
        /// Retrieves a dictionary of requirements for specific locations
        /// </summary>
        public Dictionary<int, Requirement> TrackerLocationLogic { get; private set; }

        /// <summary>
        /// Retrieves the requirements for the location, or an always true requirement
        /// if it is not found
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Requirement FindOrDefault(Location location)
        {
            if (TrackerLocationLogic?.ContainsKey(location.Id) == true)
            {
                return TrackerLocationLogic[location.Id];
            }
            return (items) => true;
        }

        private bool CheckDungeonMedallion(Progression items, Region dungeon)
        {
            if (dungeon is not INeedsMedallion) return true;
            var dungeonInfo = _tracker.WorldInfo.Dungeons.First(x => x.GetRegion(_tracker.World) == dungeon);
            return (dungeonInfo.Requirement == Medallion.Bombos && items.Bombos) ||
                (dungeonInfo.Requirement == Medallion.Ether && items.Ether) ||
                (dungeonInfo.Requirement == Medallion.Quake && items.Quake) ||
                (items.Bombos && items.Ether && items.Quake);
        }

        private int CountReward(Progression items, RewardType reward)
        {
            var dungeons = _tracker.World.Regions
                .Where(x => x is IHasReward rewardRegion && rewardRegion.Reward == reward && rewardRegion.CanComplete(items) && CheckDungeonMedallion(items, x));

            return _tracker.WorldInfo.Dungeons
                .Where(x => dungeons.Contains(x.GetRegion(_tracker.World)))
                .Count(x => x.Reward.ToRewardType() == reward);
        }
    }
}
