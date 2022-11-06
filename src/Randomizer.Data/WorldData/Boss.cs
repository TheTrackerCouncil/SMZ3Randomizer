using System.Linq;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData
{
    /// <summary>
    /// Class to represent a Super Metroid boss that can be defeated
    /// </summary>
    public class Boss
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of boss</param>
        /// <param name="world">The world this boss belongs to</param>
        /// <param name="name">The name of the boss</param>
        /// <param name="metadata">The metadata object with additional boss info</param>
        /// <param name="state">The tracking state of the boss</param>
        public Boss(BossType type, World world, string name, BossInfo metadata, TrackerBossState state)
        {
            Type = type;
            World = world;
            Name = name;
            Metadata = metadata;
            State = state;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of boss</param>
        /// <param name="world">The world this boss belongs to</param>
        /// <param name="region">The region where this boss is locatedd</param>
        /// <param name="metadata">The metadata service for looking up additional boss info</param>
        /// <param name="trackerState">The tracking state for this run</param>
        public Boss(BossType type, World world, IHasBoss region, IMetadataService? metadata, TrackerState? trackerState)
        {
            Type = type;
            World = world;
            Region = region;
            Name = type.GetDescription();
            Metadata = metadata?.Boss(type) ?? new BossInfo(Name);
            State = trackerState?.BossStates.First(x => x.WorldId == world.Id && x.BossName == Name) ?? new TrackerBossState();
        }

        public string Name { get; set; }

        public BossType Type { get; set; }

        public TrackerBossState State { get; set; }

        public BossInfo Metadata { get; set; }

        public World World { get; set; }

        public IHasBoss? Region { get; set; }

        /// <summary>
        /// Determines if an item matches the type or name
        /// </summary>
        /// <param name="type">The type to compare against</param>
        /// <param name="name">The name to compare against if the item type is set to Nothing</param>
        /// <see langword="true"/> if the item matches the given type or name
        /// name="type"/> or <paramref name="world"/>; otherwise, <see
        /// langword="false"/>.
        public bool Is(BossType type, string name)
            => (Type != BossType.None && Type == type) || (Type == BossType.None && Name == name);
    }
}
