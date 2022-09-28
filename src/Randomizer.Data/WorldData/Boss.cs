using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData
{
    public class Boss
    {
        public Boss(BossType type, World world, string name)
        {
            Type = type;
            World = world;
            Name = name;
        }

        public Boss(BossType type, World world, IHasBoss region)
        {
            Type = type;
            World = world;
            Region = region;
            Name = type.GetDescription();
        }

        public string Name { get; set; }

        public BossType Type { get; set; }

        public TrackerBossState? State { get; set; }

        public BossInfo? Metadata { get; set; }

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
