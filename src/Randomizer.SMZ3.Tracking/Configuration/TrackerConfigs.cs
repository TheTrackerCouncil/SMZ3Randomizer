using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    public class TrackerConfigs
    {

        public TrackerConfigs(TrackerOptionsAccessor options, TrackerConfigProvider provider)
        {
            var profiles = options.Options?.TrackerProfiles;
            Bosses = provider.GetBossConfig(profiles);
            Dungeons = provider.GetDungeonConfig(profiles);
            Items = provider.GetItemConfig(profiles);
            Locations = provider.GetLocationsConfig(profiles);
            Regions = provider.GetRegionConfig(profiles);
            Requests = provider.GetRequestConfig(profiles);
            Responses = provider.GetResponseConfig(profiles);
            Rooms = provider.GetRoomConfig(profiles);
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        public ItemConfig Items { get; }

        /// <summary>
        /// Gets the peg world peg configuration.
        /// </summary>
        public IReadOnlyCollection<Peg> Pegs { get; }

        /// <summary>
        /// Gets a collection of configured responses.
        /// </summary>
        public ResponseConfig Responses { get; }

        /// <summary>
        /// Gets a collection of basic requests and responses.
        /// </summary>
        public RequestConfig Requests { get; }

        /// <summary>
        /// Gets a collection of extra information about regions.
        /// </summary>
        public RegionConfig Regions { get; }

        /// <summary>
        /// Gets a collection of extra information about dungeons.
        /// </summary>
        public DungeonConfig Dungeons { get; }

        /// <summary>
        /// Gets a collection of bosses.
        /// </summary>
        public BossConfig Bosses { get; init; }

        /// <summary>
        /// Gets a collection of extra information about rooms.
        /// </summary>
        public RoomConfig Rooms { get; }

        /// <summary>
        /// Gets a collection of extra information about locations.
        /// </summary>
        public ConfigFiles.LocationConfig Locations { get; }
    }
}
