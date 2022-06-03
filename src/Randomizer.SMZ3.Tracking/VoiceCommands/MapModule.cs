using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Module for changing the map
    /// </summary>
    public class MapModule : TrackerModule
    {
        Tracker _tracker;
        TrackerMapConfig _config;
        ILogger<MapModule> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public MapModule(Tracker tracker, ILogger<MapModule> logger, TrackerMapConfig config)
            : base(tracker, logger)
        {
            _tracker = tracker;
            _logger = logger;
            _config = config;

            var maps = config.Maps;
            foreach(var map in maps)
            {
                logger.LogInformation(map.Name);
            }

            AddCommand("Update map", GetChangeMapRule(), (tracker, result) =>
            {
                var mapName = (string)result.Semantics[MapKey].Value;
                Tracker.UpdateMap(mapName);
            });
        }

        private GrammarBuilder GetChangeMapRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
            var itemNames = GetItemNames(x => x.Name[0] != "Content");
            var locationNames = GetLocationNames();
            var roomNames = GetRoomNames();

            var maps = new Choices();
            foreach (var map in _config.Maps)
            {
                foreach (var name in map.Name)
                {
                    maps.Add(new SemanticResultValue(name, map.ToString()));
                }
            }

            var version1 = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("update my map to", "change my map to")
                .Append(MapKey, maps);

            var version2 = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("show me")
                .Optional("the")
                .Append(MapKey, maps)
                .Optional("map");

            return GrammarBuilder.Combine(version1, version2);
        }
    }
}
