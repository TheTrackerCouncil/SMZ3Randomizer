using System.Linq;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Module for changing the map
    /// </summary>
    public class MapModule : TrackerModule
    {
        private readonly TrackerMapConfig _config;
        private readonly ILogger<MapModule> _logger;
        private string _prevMap = "";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public MapModule(Tracker tracker, IItemService itemService, ILogger<MapModule> logger, IWorldService worldService, TrackerMapConfig config)
            : base(tracker, itemService, worldService, logger)
        {
            _logger = logger;
            _config = config;

            var darkRoomMaps = config.Maps.Where(x => x.IsDarkRoomMap == true && x.MemoryRoomNumbers?.Count > 0).ToList();

            AddCommand("Update map", GetChangeMapRule(), (result) =>
            {
                var mapName = (string)result.Semantics[MapKey].Value;
                Tracker.UpdateMap(mapName);
                Tracker.Say(x => x.Map.UpdateMap, mapName);
            });

            AddCommand("Show dark room map", DarkRoomRule(), (result) =>
            {
                // If the player is not in a Zelda cave/dungeon
                if (Tracker.AutoTracker?.CurrentGame != AutoTracking.Game.Zelda || Tracker.AutoTracker?.ZeldaState?.OverworldScreen != 0)
                {
                    Tracker.Say(x => x.Map.NotInDarkRoom);
                    return;
                }

                // Get the room and map for the player
                var roomNumber = Tracker.AutoTracker?.ZeldaState?.CurrentRoom ?? -1;
                var map = darkRoomMaps.FirstOrDefault(x => x.MemoryRoomNumbers?.Contains(roomNumber) == true);

                if (map != null)
                {
                    if (itemService.IsTracked(Shared.ItemType.Lamp))
                    {
                        Tracker.Say(x => x.Map.HasLamp);
                        return;
                    }

                    _prevMap = Tracker.CurrentMap;
                    if (string.IsNullOrEmpty(_prevMap))
                    {
                        _prevMap = _config.Maps.Last().ToString();
                    }
                    Tracker.UpdateMap(map.ToString());
                    Tracker.Say(x => x.Map.ShowDarkRoomMap, map.Name);
                }
                else
                {
                    Tracker.Say(x => x.Map.NotInDarkRoom);
                }
            });

            AddCommand("Hide dark room map", CanSeeRule(), (result) =>
            {
                if (string.IsNullOrEmpty(_prevMap))
                {
                    Tracker.Say(x => x.Map.NoPrevDarkRoomMap);
                }
                else
                {
                    Tracker.UpdateMap(_prevMap);
                    Tracker.Say(x => x.Map.HideDarkRoomMap, _prevMap);
                    _prevMap = "";
                }
            });
        }

        private GrammarBuilder GetChangeMapRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
            var itemNames = GetItemNames(x => x.Name != "Content");
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

        private GrammarBuilder DarkRoomRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("it's dark in here", "I can't see", "show me this dark room map");
        }

        private GrammarBuilder CanSeeRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("I can see now", "I can see clearly now", "it's no longer dark", "I'm out of the dark room", "stop showing me the dark room map");
        }
    }
}
