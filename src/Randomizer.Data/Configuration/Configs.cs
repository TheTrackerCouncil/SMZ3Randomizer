using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;

namespace Randomizer.Data.Configuration
{
    /// <summary>
    /// Class that contains a collection of all configs with the user selected tracker profiles
    /// </summary>
    public class Configs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="optionsFactory">The tracker options for determining the selected tracker profiles</param>
        /// <param name="provider">The config provider for loading configs</param>
        public Configs(OptionsFactory optionsFactory, ConfigProvider provider)
        {
            var options = optionsFactory.Create();
            var profiles = options.GeneralOptions.SelectedProfiles.ToArray();
            Bosses = provider.GetBossConfig(profiles);
            Dungeons = provider.GetDungeonConfig(profiles);
            Items = provider.GetItemConfig(profiles);
            Locations = provider.GetLocationConfig(profiles);
            Regions = provider.GetRegionConfig(profiles);
            Requests = provider.GetRequestConfig(profiles);
            Responses = provider.GetResponseConfig(profiles);
            Rooms = provider.GetRoomConfig(profiles);
            Rewards = provider.GetRewardConfig(profiles);
            UILayouts = provider.GetUIConfig(profiles);
            GameLines = provider.GetGameConfig(profiles);
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        public ItemConfig Items { get; }

        /// <summary>
        /// Gets the peg world peg configuration. This will be moved to UI
        /// configs in a future release, but just storing it here for now
        /// </summary>
        public IReadOnlyCollection<Peg> Pegs { get; } = new List<Peg>
        {
            new Peg(1, 0),
            new Peg(2, 0),
            new Peg(3, 0),
            new Peg(4, 0),
            new Peg(5, 0),
            new Peg(1, 1),
            new Peg(2, 1),
            new Peg(3, 1),
            new Peg(4, 1),
            new Peg(5, 1),
            new Peg(0, 2),
            new Peg(1, 2),
            new Peg(2, 2),
            new Peg(3, 2),
            new Peg(4, 2),
            new Peg(5, 2),
            new Peg(0, 3),
            new Peg(1, 3),
            new Peg(2, 3),
            new Peg(0, 4),
            new Peg(1, 4),
            new Peg(2, 4)
        };

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
        public BossConfig Bosses { get; }

        /// <summary>
        /// Gets a collection of extra information about rooms.
        /// </summary>
        public RoomConfig Rooms { get; }

        /// <summary>
        /// Gets a collection of extra information about locations.
        /// </summary>
        public LocationConfig Locations { get; }

        /// <summary>
        /// Gets a collection of extra information about rewards
        /// </summary>
        public RewardConfig Rewards { get; }

        /// <summary>
        /// Gets a collection of available UI layouts
        /// </summary>
        public UIConfig UILayouts { get; }

        /// <summary>
        /// Gets the in game lines
        /// </summary>
        public GameLinesConfig GameLines { get; }
    }
}
