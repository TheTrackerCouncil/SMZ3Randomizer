using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

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
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    public MapModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, ILogger<MapModule> logger, IWorldQueryService worldQueryService, TrackerMapConfig config)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        _logger = logger;
        _config = config;
    }

    private SpeechRecognitionGrammarBuilder GetChangeMapRule()
    {
        var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
        var itemNames = GetItemNames(x => x.Name != "Content");
        var locationNames = GetLocationNames();
        var roomNames = GetRoomNames();

        var maps = new List<GrammarKeyValueChoice>();
        foreach (var map in _config.Maps)
        {
            foreach (var name in map.Name)
            {
                maps.Add(new GrammarKeyValueChoice(name, map.ToString()));
            }
        }

        var version1 = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("update my map to", "change my map to")
            .Append(MapKey, maps);

        var version2 = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("show me")
            .Optional("the")
            .Append(MapKey, maps)
            .Optional("map");

        return SpeechRecognitionGrammarBuilder.Combine(version1, version2);
    }

    private SpeechRecognitionGrammarBuilder DarkRoomRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("it's dark in here", "I can't see", "show me this dark room map");
    }

    private SpeechRecognitionGrammarBuilder CanSeeRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("I can see now", "I can see clearly now", "it's no longer dark", "I'm out of the dark room", "stop showing me the dark room map");
    }

    public override void AddCommands()
    {
        var darkRoomMaps = _config.Maps.Where(x => x.IsDarkRoomMap == true && x.MemoryRoomNumbers?.Count > 0).ToList();

        AddCommand("Update map", GetChangeMapRule(), (result) =>
        {
            var mapName = (string)result.Semantics[MapKey].Value;
            TrackerBase.GameStateTracker.UpdateMap(mapName);
            TrackerBase.Say(x => x.Map.UpdateMap, args: [mapName]);
        });

        AddCommand("Show dark room map", DarkRoomRule(), (result) =>
        {
            // If the player is not in a Zelda cave/dungeon
            if (TrackerBase.AutoTracker?.CurrentGame != Game.Zelda || TrackerBase.AutoTracker?.ZeldaState?.OverworldScreen != 0)
            {
                TrackerBase.Say(x => x.Map.NotInDarkRoom);
                return;
            }

            // Get the room and map for the player
            var roomNumber = TrackerBase.AutoTracker?.ZeldaState?.CurrentRoom ?? -1;
            var map = darkRoomMaps.FirstOrDefault(x => x.MemoryRoomNumbers?.Contains(roomNumber) == true);

            if (map != null)
            {
                if (PlayerProgressionService.IsTracked(ItemType.Lamp))
                {
                    TrackerBase.Say(x => x.Map.HasLamp);
                    return;
                }

                _prevMap = TrackerBase.GameStateTracker.CurrentMap;
                if (string.IsNullOrEmpty(_prevMap))
                {
                    _prevMap = _config.Maps.Last().ToString();
                }
                TrackerBase.GameStateTracker.UpdateMap(map.ToString());
                TrackerBase.Say(x => x.Map.ShowDarkRoomMap, args: [map.Name]);
            }
            else
            {
                TrackerBase.Say(x => x.Map.NotInDarkRoom);
            }
        });

        AddCommand("Hide dark room map", CanSeeRule(), (result) =>
        {
            if (string.IsNullOrEmpty(_prevMap))
            {
                TrackerBase.Say(x => x.Map.NoPrevDarkRoomMap);
            }
            else
            {
                TrackerBase.GameStateTracker.UpdateMap(_prevMap);
                TrackerBase.Say(x => x.Map.HideDarkRoomMap, args: [_prevMap]);
                _prevMap = "";
            }
        });
    }
}
