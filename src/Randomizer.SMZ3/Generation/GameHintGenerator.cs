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
    public class GameHintGenerator : IGameHintGenerator
    {
        private static readonly Regex _spaces = new(@"[\s\r\n]+");
        private readonly ILogger<GameHintGenerator> _logger;
        private readonly IMetadataService _metadataService;
        private GameLinesConfig _gameLines;
        private const int TotalHintCount = 15;
        private int _sphereThreshold = 0;
        private Random _random;

        public GameHintGenerator(Configs configs, IMetadataService metadataService, ILogger<GameHintGenerator> logger)
        {
            _gameLines = configs.GameLines;
            _logger = logger;
            _metadataService = metadataService;
        }

        public IEnumerable<string> GetHints(World world, ICollection<World> allWorlds, Playthrough playthrough, int hintCount, int seed)
        {
            hintCount = hintCount > 15 ? 15 : hintCount;

            _random = new Random(seed);
            var lateSpheres = playthrough.Spheres.TakeLast(_sphereThreshold = (int)(playthrough.Spheres.Count * .5));

            var allHints = new List<string>();

            allHints.AddRange(GetLateProgressionItemHints(lateSpheres, 8));
            allHints.AddRange(GetNonCrystalDungeonHints(world, allWorlds));
            allHints.AddRange(GetOutOfTheWayLocationHints(world, allWorlds));

            var hints = allHints.Distinct().Shuffle(_random).Take(hintCount).Select(x => GameSafeString(x));

            if (hints.Count() < 15)
            {
                hints = hints.Concat(hints.Take(15 - hints.Count())).Shuffle(_random);
            }

            return hints;
        }

        private IEnumerable<string> GetLateProgressionItemHints(IEnumerable<Playthrough.Sphere> lateSpheres, int count)
        {
            // Grab items marked as progression that are not junk or scam items
            var locations = lateSpheres
                .SelectMany(x => x.Locations)
                .Where(x => x.Item.Progression && !x.Item.Type.IsInAnyCategory(ItemCategory.Junk, ItemCategory.Scam))
                .Shuffle(_random)
                .Take(count);

            var hints = locations.Select(x => _gameLines.HintLocationHasItem.Format(GetLocationName(x), GetItemName(x.Item.Type)));
            foreach (var hint in hints)
                _logger.LogInformation(hint);

            return hints;
        }

        private IEnumerable<string> GetNonCrystalDungeonHints(World world, ICollection<World> allWorlds)
        {
            var nonCrystalDungeons = world.Dungeons.Where(x => world.Config.ZeldaKeysanity || x.IsPendantDungeon || x is HyruleCastle or GanonsTower);
            var hints = new List<string>();

            foreach (var dungeon in nonCrystalDungeons)
            {
                var dungeonRegion = dungeon as Region;
                var usefulNess = CheckIfLocationsAreImportant(allWorlds, dungeonRegion.Locations);
                var dungeonName = _metadataService.Dungeon(dungeon).Name.ToString();

                if (usefulNess == LocationUsefulness.Mandatory)
                {
                    var hint = _gameLines.HintLocationIsMandatory.Format(dungeonName);
                    hints.Add(hint);
                    _logger.LogInformation(hint);
                }
                else if (usefulNess == LocationUsefulness.NiceToHave)
                {
                    var hint = _gameLines.HintLocationHasUsefulItem.Format(dungeonName);
                    hints.Add(hint);
                    _logger.LogInformation(hint);
                }
                else
                {
                    var hint = _gameLines.HintLocationEmpty.Format(dungeonName);
                    hints.Add(hint);
                    _logger.LogInformation(hint);
                }
            }

            return hints;
        }

        private IEnumerable<string> GetOutOfTheWayLocationHints(World world, ICollection<World> allWorlds)
        {
            var hints = new List<string>();

            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 33)); // Waterway
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 132)); // Wrecked pool
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 129)); // Wrecked ship post chozo speed booster item
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 150)); // Shaktool
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 143)); // Plasma beam
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 256 + 44)); // Sahasrahla
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 256 + 14)); // Ped
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 256 + 36)); // Zora
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 256 + 78)); // Catfish
            AddLocationHint(hints, allWorlds, world.Locations.Where(x => x.Id is 256 + 139 or 256 + 140), "The left side of swamp palace");
            AddLocationHint(hints, allWorlds, world.DarkWorldNorthEast.PyramidFairy.Locations);
            AddLocationHint(hints, allWorlds, world.DarkWorldSouth.HypeCave.Locations);
            AddLocationHint(hints, allWorlds, world.DarkWorldDeathMountainEast.HookshotCave.Locations);
            AddLocationHint(hints, allWorlds, world.LightWorldDeathMountainEast.ParadoxCave.Locations);
            AddLocationHint(hints, allWorlds, world.UpperNorfairCrocomire.Locations);

            return hints;
        }

        private void AddLocationHint(List<string> hints, ICollection<World> allWorlds, IEnumerable<Location> locations, string? areaName = null)
        {
            // If we only have a single location, just say what's there
            if (locations.Count() == 1)
            {
                var location = locations.First();

                if (areaName == null)
                {
                    areaName = GetLocationName(location);
                }

                var hint = _gameLines.HintLocationHasItem.Format(areaName, GetItemName(location.Item.Type));
                hints.Add(hint);
                _logger.LogInformation(hint);
                return;
            }

            var usefulness = CheckIfLocationsAreImportant(allWorlds, locations);

            if (areaName == null)
            {
                areaName = GetLocationsName(locations);
            }

            if (usefulness == LocationUsefulness.Mandatory)
            {
                var hint = _gameLines.HintLocationIsMandatory.Format(areaName);
                hints.Add(hint);
                _logger.LogInformation(hint);
            }
            else if (usefulness == LocationUsefulness.NiceToHave)
            {
                var hint = _gameLines.HintLocationHasUsefulItem.Format(areaName);
                hints.Add(hint);
                _logger.LogInformation(hint);
            }
            else
            {
                var hint = _gameLines.HintLocationEmpty.Format(areaName);
                hints.Add(hint);
                _logger.LogInformation(hint);
            }
        }

        private LocationUsefulness CheckIfLocationsAreImportant(IEnumerable<World> worlds, IEnumerable<Location> locations)
        {
            var worldLocations = worlds.SelectMany(x => x.Locations).Except(locations).ToList();
            var locationItems = locations.Select(x => x.Item.Type).ToList();
            var region = locations.First().Region;
            try
            {
                var spheres = Playthrough.GenerateSpheres(worldLocations);
                var sphereLocations = spheres.SelectMany(x => x.Locations);

                var canBeatGT = CheckSphereLocationCount(sphereLocations, locations, 256 + 215, worlds.Count());
                var canBeatKraid = CheckSphereLocationCount(sphereLocations, locations, 48, worlds.Count());
                var canBeatPhantoon = CheckSphereLocationCount(sphereLocations, locations, 134, worlds.Count());
                var canBeatDraygon = CheckSphereLocationCount(sphereLocations, locations, 154, worlds.Count());
                var canBeatRidley = CheckSphereLocationCount(sphereLocations, locations, 78, worlds.Count());
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

        private bool CheckSphereLocationCount(IEnumerable<Location> sphereLocations, IEnumerable<Location> checkedLocations, int locationId, int worldCount)
        {
            var ignoreOwnWorld = checkedLocations.Any(x => x.Id == locationId);
            var matchingLocationCount = sphereLocations.Count(x => x.Id == locationId);
            return matchingLocationCount == worldCount - (ignoreOwnWorld ? 1 : 0);
        }

        private bool CheckSphereCrateriaBossKeys(IEnumerable<Location> sphereLocations)
        {
            var numKeysanity = sphereLocations.Select(x => x.World).Distinct().Where(x => x.Config.MetroidKeysanity).Count();
            var numCratieriaBossKeys = sphereLocations.Select(x => x.Item.Type).Count(x => x == ItemType.CardMaridiaBoss);
            return numKeysanity == numCratieriaBossKeys;
        }

        private string GetLocationsName(IEnumerable<Location> locations)
        {
            if (locations.Count() == 1)
            {
                return GetLocationName(locations.First());
            }
            else if (locations.All(x => x.Room != null))
            {
                return locations.First().Room.Name;
            }
            else
            {
                return locations.First().Region.Name;
            }
        }

        private string GetLocationName(Location location)
        {
            return $"{_metadataService.Region(location.Region).Name} - {_metadataService.Location(location.Id).Name}";
        }

        private string GetItemName(ItemType type)
        {
            return _metadataService.Item(type).NameWithArticle.ToString();
        }

        private string GameSafeString(string hint)
        {
            hint = _spaces.Replace(hint, " ");
            var words = hint.Split(" ");
            var output = new List<string>();
            var currentLine = "";
            foreach (var word in words)
            {
                if (word.Length + currentLine.Length > 18)
                {
                    output.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine += " " + word;
                }
            }
            output.Add(currentLine);
            return string.Join("\n", output).Trim();
        }

        private enum LocationUsefulness
        {
            Useless,
            NiceToHave,
            Mandatory
        }
    }
}
