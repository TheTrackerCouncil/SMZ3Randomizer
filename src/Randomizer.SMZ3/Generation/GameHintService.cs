using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Shared;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Generation
{
    /// <summary>
    /// Service for generating hints for the player
    /// </summary>
    public class GameHintService : IGameHintService
    {
        public static readonly List<string> HintLocations = new List<string>()
        {
            "telepathic_tile_eastern_palace",
            "telepathic_tile_tower_of_hera_floor_4",
            "telepathic_tile_spectacle_rock",
            "telepathic_tile_swamp_entrance",
            "telepathic_tile_thieves_town_upstairs",
            "telepathic_tile_misery_mire",
            "telepathic_tile_palace_of_darkness",
            "telepathic_tile_desert_bonk_torch_room",
            "telepathic_tile_castle_tower",
            "telepathic_tile_ice_large_room",
            "telepathic_tile_turtle_rock",
            "telepathic_tile_ice_entrance",
            "telepathic_tile_ice_stalfos_knights_room",
            "telepathic_tile_tower_of_hera_entrance",
            "telepathic_tile_south_east_darkworld_cave"
        };

        private readonly ILogger<GameHintService> _logger;
        private readonly IMetadataService _metadataService;
        private GameLinesConfig _gameLines;
        private Random _random;

        public GameHintService(Configs configs, IMetadataService metadataService, ILogger<GameHintService> logger)
        {
            _gameLines = configs.GameLines;
            _logger = logger;
            _metadataService = metadataService;
        }

        /// <summary>
        /// Retrieves the hints to display in game for the player
        /// </summary>
        /// <param name="hintPlayerWorld">The player that will be receiving the hints</param>
        /// <param name="allWorlds">All worlds that are a part of the seed</param>
        /// <param name="playthrough">The initial playthrough with all of the spheres</param>
        /// <param name="hintCount">The number of hints to generate</param>
        /// <param name="seed">Seed number for shuffling and randomization</param>
        /// <returns>A collection of strings to use for the in game hints</returns>
        public IEnumerable<string> GetInGameHints(World hintPlayerWorld, ICollection<World> allWorlds, Playthrough playthrough, int hintCount, int seed)
        {
            if (hintCount <= 0)
            {
                return Enumerable.Empty<string>();
            }

            hintCount = hintCount > HintLocations.Count ? HintLocations.Count : hintCount;

            _random = new Random(seed);
            var lateSpheres = playthrough.Spheres.TakeLast((int)(playthrough.Spheres.Count * .5));

            var allHints = new List<string>();

            allHints.AddRange(GetProgressionItemHints(hintPlayerWorld, lateSpheres, 8));
            allHints.AddRange(GetDungeonHints(hintPlayerWorld, allWorlds));
            allHints.AddRange(GetLocationHints(hintPlayerWorld, allWorlds));

            _logger.LogDebug("Possible in game hints");
            foreach (var hint in allHints.Distinct())
            {
                _logger.LogDebug(hint);
            }

            var hints = allHints.Distinct().Shuffle(_random).Take(hintCount);
            while (hints.Count() < HintLocations.Count)
            {
                hints = hints.Concat(hints.Take(Math.Min(HintLocations.Count() - hints.Count(), hints.Count())));
            }
            hints = hints.Shuffle(_random);

            _logger.LogDebug("Selected in game hints");
            foreach (var hint in hints)
            {
                _logger.LogDebug(hint);
            }

            return hints;
        }

        /// <summary>
        /// Retrieves hints stating the location of items for the current player
        /// that are in the provided spheres
        /// </summary>
        private IEnumerable<string> GetProgressionItemHints(World hintPlayerWorld, IEnumerable<Playthrough.Sphere> spheres, int count)
        {
            // Grab items for the player marked as progression that are not junk or scam items
            var locations = spheres
                .SelectMany(x => x.Locations)
                .Where(x => x.World == hintPlayerWorld && x.Item.Progression && !x.Item.Type.IsInAnyCategory(ItemCategory.Junk, ItemCategory.Scam))
                .Shuffle(_random)
                .Take(count);

            var hints = locations.Select(x => _gameLines.HintLocationHasItem.Format(GetLocationName(hintPlayerWorld, x), GetItemName(hintPlayerWorld, x.Item)));
            foreach (var hint in hints)
                _logger.LogInformation(hint);

            return hints;
        }

        /// <summary>
        /// Retrives hints stating how important dungeons are to beating the game
        /// </summary>
        private IEnumerable<string> GetDungeonHints(World hintPlayerWorld, ICollection<World> allWorlds)
        {
            // For keysanity/multiworld check all dungeons, otherwise check non-crystal dungeons
            var dungeons = hintPlayerWorld.Dungeons
                .Where(x => hintPlayerWorld.Config.MultiWorld || hintPlayerWorld.Config.ZeldaKeysanity || x.IsPendantDungeon || x is HyruleCastle or GanonsTower);
            var hints = new List<string>();

            foreach (var dungeon in dungeons)
            {
                var dungeonRegion = dungeon as Region;
                var usefulNess = CheckIfLocationsAreImportant(allWorlds, dungeonRegion.Locations);
                var dungeonName = _metadataService.Dungeon(dungeon).Name.ToString();

                if (usefulNess == LocationUsefulness.Mandatory)
                {
                    hints.Add(_gameLines.HintLocationIsMandatory.Format(dungeonName));
                }
                else if (usefulNess == LocationUsefulness.NiceToHave)
                {
                    hints.Add(_gameLines.HintLocationHasUsefulItem.Format(dungeonName));
                }
                else
                {
                    hints.Add(_gameLines.HintLocationEmpty.Format(dungeonName));
                }
            }

            return hints;
        }

        /// <summary>
        /// Retrieves hints for out of the way locations and rooms to the pool of hints
        /// </summary>
        private IEnumerable<string> GetLocationHints(World hintPlayerWorld, ICollection<World> allWorlds)
        {
            var hints = new List<string>();

            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 33)); // Waterway
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 132)); // Wrecked pool
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 129)); // Wrecked ship post chozo speed booster item
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 150)); // Shaktool
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 143)); // Plasma beam
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 256 + 44)); // Sahasrahla
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 256 + 14)); // Ped
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 256 + 36)); // Zora
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 256 + 78)); // Catfish
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 256 + 117)); // Tower of Hera big key chest
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.Locations.Where(x => x.Id is 256 + 139 or 256 + 140), "The left side of swamp palace");
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.DarkWorldNorthEast.PyramidFairy.Locations);
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.DarkWorldSouth.HypeCave.Locations);
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.DarkWorldDeathMountainEast.HookshotCave.Locations);
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.LightWorldDeathMountainEast.ParadoxCave.Locations);
            AddLocationHint(hints, hintPlayerWorld, allWorlds, hintPlayerWorld.UpperNorfairCrocomire.Locations);

            return hints;
        }

        /// <summary>
        /// Adds a hint for a given set of location(s) to the list of hints
        /// </summary>
        private void AddLocationHint(List<string> hints, World hintPlayerWorld, ICollection<World> allWorlds, IEnumerable<Location> locations, string? areaName = null)
        {
            // If we only have a single location, just say what item is there
            if (locations.Count() == 1)
            {
                areaName = areaName == null
                    ? GetLocationName(hintPlayerWorld, locations.First())
                    : $"{areaName}{GetMultiworldSuffix(hintPlayerWorld, locations.First().World)}";

                hints.Add(_gameLines.HintLocationHasItem.Format(areaName, GetItemName(hintPlayerWorld, locations.First().Item)));
                return;
            }

            var usefulness = CheckIfLocationsAreImportant(allWorlds, locations);

            areaName = areaName == null
                ? GetLocationsName(hintPlayerWorld, locations)
                : $"{areaName}{GetMultiworldSuffix(hintPlayerWorld, locations.First().World)}";

            if (usefulness == LocationUsefulness.Mandatory)
            {
                hints.Add( _gameLines.HintLocationIsMandatory.Format(areaName));
            }
            else if (usefulness == LocationUsefulness.NiceToHave)
            {
                hints.Add( _gameLines.HintLocationHasUsefulItem.Format(areaName));
            }
            else
            {
                hints.Add(_gameLines.HintLocationEmpty.Format(areaName));
            }
        }

        /// <summary>
        /// Checks how useful a location is based on if the seed can be completed if we remove those
        /// locations from the playthrough and if the items there are at least slightly useful
        /// </summary>
        private LocationUsefulness CheckIfLocationsAreImportant(IEnumerable<World> allWorlds, IEnumerable<Location> locations)
        {
            var worldLocations = allWorlds.SelectMany(x => x.Locations).Except(locations).ToList();
            var locationItems = locations.Select(x => x.Item.Type).ToList();
            var region = locations.First().Region;
            try
            {
                var spheres = Playthrough.GenerateSpheres(worldLocations);
                var sphereLocations = spheres.SelectMany(x => x.Locations);

                var canBeatGT = CheckSphereLocationCount(sphereLocations, locations, 256 + 215, allWorlds.Count());
                var canBeatKraid = CheckSphereLocationCount(sphereLocations, locations, 48, allWorlds.Count());
                var canBeatPhantoon = CheckSphereLocationCount(sphereLocations, locations, 134, allWorlds.Count());
                var canBeatDraygon = CheckSphereLocationCount(sphereLocations, locations, 154, allWorlds.Count());
                var canBeatRidley = CheckSphereLocationCount(sphereLocations, locations, 78, allWorlds.Count());
                var allCrateriaBosSKeys = CheckSphereCrateriaBossKeys(sphereLocations);

                if (!canBeatGT || !canBeatKraid || !canBeatPhantoon || !canBeatDraygon || !canBeatRidley || !allCrateriaBosSKeys)
                {
                    return LocationUsefulness.Mandatory;
                }
                var usefulItems = locations.Where(x => x.Item.Progression || x.Item.Type.IsInCategory(ItemCategory.Nice) || x.Item.Type == ItemType.ProgressiveSword).Select(x => x.Item);
                return usefulItems.Any() ? LocationUsefulness.NiceToHave : LocationUsefulness.Useless;
            }
            catch
            {
                return LocationUsefulness.Mandatory;
            }
        }

        /// <summary>
        /// Checks if a given location is found for all worlds in the locations from all spheres
        /// If that location is in the checked locations list, it'll be ignored
        /// </summary>
        private bool CheckSphereLocationCount(IEnumerable<Location> sphereLocations, IEnumerable<Location> checkedLocations, int locationId, int worldCount)
        {
            var ignoreOwnWorld = checkedLocations.Any(x => x.Id == locationId);
            var matchingLocationCount = sphereLocations.Count(x => x.Id == locationId);
            return matchingLocationCount == worldCount - (ignoreOwnWorld ? 1 : 0);
        }

        /// <summary>
        /// Checks if the crateria boss keycard is found in the spheres for all players
        /// </summary>
        private bool CheckSphereCrateriaBossKeys(IEnumerable<Location> sphereLocations)
        {
            var numKeysanity = sphereLocations.Select(x => x.World).Distinct().Where(x => x.Config.MetroidKeysanity).Count();
            var numCratieriaBossKeys = sphereLocations.Select(x => x.Item.Type).Count(x => x == ItemType.CardMaridiaBoss);
            return numKeysanity == numCratieriaBossKeys;
        }

        private string GetLocationsName(World hintPlayerWorld, IEnumerable<Location> locations)
        {
            if (locations.Count() == 1)
            {
                return GetLocationName(hintPlayerWorld, locations.First());
            }
            else if (locations.All(x => x.Room != null))
            {
                var name = _metadataService.Room(locations.First().Room).Name;
                return $"{name}{GetMultiworldSuffix(hintPlayerWorld, locations.First().World)}";
            }
            else
            {
                var name = _metadataService.Region(locations.First().Region).Name;
                return $"{name}{GetMultiworldSuffix(hintPlayerWorld, locations.First().World)}";
            }
        }

        private string GetLocationName(World hintPlayerWorld, Location location)
        {
            var name = $"{_metadataService.Region(location.Region).Name} - {_metadataService.Location(location.Id).Name}";
            return $"{name}{GetMultiworldSuffix(hintPlayerWorld, location.World)}";
        }

        private string GetItemName(World hintPlayerWorld, Item item)
        {
            var itemName = _metadataService.Item(item.Type).NameWithArticle;
            return $"{itemName}{GetMultiworldSuffix(hintPlayerWorld, item)}";
        }

        private string GetMultiworldSuffix(World hintPlayerWorld, Item item)
        {
            if (!hintPlayerWorld.Config.MultiWorld)
            {
                return "";
            }
            else
            {
                return hintPlayerWorld == item.World
                    ? " belonging to you"
                    : " belonging to another player"; // Will need to update to player name when multiworld is working
            }
        }

        private string GetMultiworldSuffix(World hintPlayerWorld, World locationWorld)
        {
            if (!hintPlayerWorld.Config.MultiWorld)
            {
                return "";
            }
            else
            {
                return hintPlayerWorld == locationWorld
                    ? " in your world"
                    : " in another player's world"; // Will need to update to player name when multiworld is working
            }
        }

        private enum LocationUsefulness
        {
            Useless,
            NiceToHave,
            Mandatory
        }
    }
}
