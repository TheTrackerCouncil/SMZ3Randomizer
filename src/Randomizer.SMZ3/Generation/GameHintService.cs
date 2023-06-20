using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Generation
{

    public delegate string? Hint();

    /// <summary>
    /// Service for generating hints for the player
    /// </summary>
    public class GameHintService : IGameHintService
    {
        public static readonly List<string> HintLocations = new()
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

        private static readonly List<LocationId> s_importantLocations = new()
        {
            LocationId.KraidsLairVariaSuit, // After Kraid
            LocationId.WreckedShipEastSuper, // After Phantoon
            LocationId.InnerMaridiaSpaceJump, // After Draygon
            LocationId.LowerNorfairRidleyTank, // After Ridley
            LocationId.EasternPalaceArmosKnights,
            LocationId.DesertPalaceLanmolas,
            LocationId.TowerOfHeraMoldorm,
            LocationId.PalaceOfDarknessHelmasaurKing,
            LocationId.SwampPalaceArrghus,
            LocationId.SkullWoodsMothula,
            LocationId.ThievesTownBlind,
            LocationId.IcePalaceKholdstare,
            LocationId.MiseryMireVitreous,
            LocationId.TurtleRockTrinexx,
            LocationId.GanonsTowerMoldormChest,
        };

        private static readonly Dictionary<Type, LocationId> s_dungeonBossLocations = new()
        {
            { typeof(EasternPalace), LocationId.EasternPalaceArmosKnights },
            { typeof(DesertPalace), LocationId.DesertPalaceLanmolas },
            { typeof(TowerOfHera), LocationId.TowerOfHeraMoldorm },
            { typeof(PalaceOfDarkness), LocationId.PalaceOfDarknessHelmasaurKing },
            { typeof(SwampPalace), LocationId.SwampPalaceArrghus },
            { typeof(SkullWoods), LocationId.SkullWoodsMothula },
            { typeof(ThievesTown), LocationId.ThievesTownBlind },
            { typeof(IcePalace), LocationId.IcePalaceKholdstare },
            { typeof(MiseryMire), LocationId.MiseryMireVitreous },
            { typeof(TurtleRock), LocationId.TurtleRockTrinexx },
        };

        private readonly ILogger<GameHintService> _logger;
        private readonly IMetadataService _metadataService;
        private readonly GameLinesConfig _gameLines;
        private Random _random;

        public GameHintService(Configs configs, IMetadataService metadataService, ILogger<GameHintService> logger)
        {
            _gameLines = configs.GameLines;
            _logger = logger;
            _metadataService = metadataService;
            _random = new Random();
        }

        /// <summary>
        /// Retrieves the hints to display in game for the player
        /// </summary>
        /// <param name="hintPlayerWorld">The player that will be receiving the hints</param>
        /// <param name="allWorlds">All worlds that are a part of the seed</param>
        /// <param name="playthrough">The initial playthrough with all of the spheres</param>
        /// <param name="seed">Seed number for shuffling and randomization</param>
        /// <returns>A collection of strings to use for the in game hints</returns>
        public IEnumerable<string> GetInGameHints(World hintPlayerWorld, ICollection<World> allWorlds, Playthrough playthrough, int seed)
        {
            _random = new Random(seed).Sanitize();

            var allHints = new List<Hint>();

            var importantLocations = GetImportantLocations(allWorlds);

            allHints.AddRange(GetProgressionItemHints(hintPlayerWorld, playthrough.Spheres, 5));
            allHints.AddRange(GetDungeonHints(hintPlayerWorld, allWorlds, importantLocations));
            allHints.AddRange(GetLocationHints(hintPlayerWorld, allWorlds, importantLocations));
            allHints.AddRange(GetMedallionHints(hintPlayerWorld));

            allHints = allHints.Shuffle(_random);

            var hintLines = new List<string>();

            while (hintLines.Count < 15 && allHints.Any())
            {
                var hint = allHints.First();
                allHints.Remove(hint);
                var hintLine = hint.Invoke();
                if (!string.IsNullOrEmpty(hintLine) && !hintLines.Contains(hintLine))
                {
                    hintLines.Add(hintLine);
                }
            }

            _logger.LogDebug("Possible in game hints");
            foreach (var hint in hintLines)
            {
                _logger.LogDebug("{Hint}", hint);
            }

            return hintLines;
        }

        /// <summary>
        /// Retrieves hints stating the location of items for the current player
        /// that are in the provided spheres
        /// </summary>
        private IEnumerable<Hint> GetProgressionItemHints(World hintPlayerWorld, IEnumerable<Playthrough.Sphere> spheres, int count)
        {
            // Grab items for the player marked as progression that are not junk or scam items
            var progressionLocations = spheres
                .SelectMany(x => x.Locations)
                .Where(x => x.Item.World == hintPlayerWorld && x.Item.Progression &&
                            !x.Item.Type.IsInAnyCategory(ItemCategory.Junk, ItemCategory.Scam));
            progressionLocations =
                progressionLocations.TakeLast(progressionLocations.Count() / 2).Shuffle(_random).Take(count);

            return progressionLocations.Select(x => new Hint(() => GetProgressionItemHint(x)));

            string? GetProgressionItemHint(Location location)
            {
                return _gameLines.HintLocationHasItem?.Format(GetLocationName(hintPlayerWorld, location),
                    GetItemName(hintPlayerWorld, location.Item));
            }
        }

        /// <summary>
        /// Retrives hints stating how important dungeons are to beating the game
        /// </summary>
        private IEnumerable<Hint> GetDungeonHints(World hintPlayerWorld, ICollection<World> allWorlds, IEnumerable<Location> importantLocations)
        {
            // For keysanity/multiworld check all dungeons, otherwise check non-crystal dungeons
            var dungeons = hintPlayerWorld.Dungeons
                .Where(x => hintPlayerWorld.Config.MultiWorld || hintPlayerWorld.Config.ZeldaKeysanity || x.IsPendantDungeon || x is HyruleCastle or GanonsTower);
            var hints = new List<Hint>();

            foreach (var dungeon in dungeons)
            {
                hints.Add(() => GetDungeonHint(dungeon));
            }

            return hints;

            string? GetDungeonHint(IDungeon dungeon)
            {
                var dungeonRegion = (Region)dungeon;
                var usefulness = CheckIfLocationsAreImportant(allWorlds, importantLocations, dungeonRegion.Locations);
                var dungeonName = GetDungeonName(hintPlayerWorld, dungeon, dungeonRegion);

                if (usefulness == LocationUsefulness.Mandatory)
                {
                    return _gameLines.HintLocationIsMandatory?.Format(dungeonName);
                }
                else if (usefulness == LocationUsefulness.NiceToHave)
                {
                    return _gameLines.HintLocationHasUsefulItem?.Format(dungeonName);
                }
                else if (usefulness == LocationUsefulness.Sword)
                {
                    return _gameLines.HintLocationHasSword?.Format(dungeonName);
                }
                else
                {
                    return _gameLines.HintLocationEmpty?.Format(dungeonName);
                }
            }
        }

        /// <summary>
        /// Retrieves hints for out of the way locations and rooms to the pool of hints
        /// </summary>
        private IEnumerable<Hint> GetLocationHints(World hintPlayerWorld, ICollection<World> allWorlds, IEnumerable<Location> importantLocations)
        {
            var hints = new List<Hint>();

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PinkBrinstarWaterwayEnergyTank)));

            // Wrecked pool
            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.WreckedShipEnergyTank)));

            // Shaktool
            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaSpringBall)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaPlasma)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Sahasrahla)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.MasterSwordPedestal)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.KingZora)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Catfish)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.TowerOfHeraBigKeyChest)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.RedBrinstarXRayScope)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PinkBrinstarHoptank)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.EtherTablet)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.BombosTablet)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.CrateriaSuper)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.SpikeCave)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.SwampPalaceWestChest or LocationId.SwampPalaceBigKeyChest), "The left side of swamp palace"));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.WreckedShipGravitySuit or LocationId.WreckedShipBowlingAlleyTop or LocationId.WreckedShipBowlingAlleyBottom), "Wrecked Ship post Chozo concert area"));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.DarkWorldNorthEast.PyramidFairy.Locations));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.DarkWorldSouth.HypeCave.Locations));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.DarkWorldDeathMountainEast.HookshotCave.Locations));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.LightWorldDeathMountainEast.ParadoxCave.Locations));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.UpperNorfairCrocomire.Locations));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.InnerMaridia.LeftSandPit.Locations));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.InnerMaridia.RightSandPit.Locations));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaAqueductLeft or LocationId.InnerMaridiaAqueductRight), "Inner Maridia Aqueduct"));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Blacksmith or LocationId.PurpleChest), "smith chain"));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.CrateriaGauntletEnergyTank or LocationId.CrateriaGauntletShaftLeft or LocationId.CrateriaGauntletShaftRight), "Crateria gauntlet"));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.FluteSpot)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.MagicBat)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PotionShop)));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.GreenBrinstarEarlySupersBottom or LocationId.GreenBrinstarEarlySupersTop or LocationId.GreenBrinstarReserveTankChozo or LocationId.GreenBrinstarReserveTankHidden or LocationId.GreenBrinstarReserveTankVisible), "Green Brinstar machball hall"));

            hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
                hintPlayerWorld.Locations.Where(x => x.Id is LocationId.GreenBrinstarEtecoonSuper or LocationId.GreenBrinstarEtecoonEnergyTank or LocationId.GreenBrinstarMainShaft), "Green Brinstar bottom left area"));

            return hints;
        }

        /// <summary>
        /// Adds a hint for a given set of location(s) to the list of hints
        /// </summary>
        private string? GetLocationHint(World hintPlayerWorld, ICollection<World> allWorlds, IEnumerable<Location> importantLocations, IEnumerable<Location> locations, string? areaName = null)
        {
            // If we only have a single location, just say what item is there
            if (locations.Count() == 1)
            {
                areaName = areaName == null
                    ? GetLocationName(hintPlayerWorld, locations.First())
                    : $"{areaName}{GetMultiworldSuffix(hintPlayerWorld, locations.First().World)}";

                return  _gameLines.HintLocationHasItem?.Format(areaName,
                    GetItemName(hintPlayerWorld, locations.First().Item));
            }

            var usefulness = CheckIfLocationsAreImportant(allWorlds, importantLocations, locations);

            areaName = areaName == null
                ? GetLocationsName(hintPlayerWorld, locations)
                : $"{areaName}{GetMultiworldSuffix(hintPlayerWorld, locations.First().World)}";

            if (usefulness == LocationUsefulness.Mandatory)
            {
                return _gameLines.HintLocationIsMandatory?.Format(areaName);
            }
            else if (usefulness == LocationUsefulness.NiceToHave)
            {
                return _gameLines.HintLocationHasUsefulItem?.Format(areaName);
            }
            else if (usefulness == LocationUsefulness.Sword)
            {
                return _gameLines.HintLocationHasSword?.Format(areaName);
            }
            else
            {
                return _gameLines.HintLocationEmpty?.Format(areaName);
            }
        }

        /// <summary>
        /// Checks how useful a location is based on if the seed can be completed if we remove those
        /// locations from the playthrough and if the items there are at least slightly useful
        /// </summary>
        private LocationUsefulness CheckIfLocationsAreImportant(IEnumerable<World> allWorlds, IEnumerable<Location> importantLocations, IEnumerable<Location> locations)
        {
            var worldLocations = importantLocations.Except(locations).ToList();
            try
            {
                var spheres = Playthrough.GenerateSpheres(worldLocations);
                var sphereLocations = spheres.SelectMany(x => x.Locations);

                var canBeatGT = CheckSphereLocationCount(sphereLocations, locations, LocationId.GanonsTowerMoldormChest, allWorlds.Count());
                var canBeatKraid = CheckSphereLocationCount(sphereLocations, locations, LocationId.KraidsLairVariaSuit, allWorlds.Count());
                var canBeatPhantoon = CheckSphereLocationCount(sphereLocations, locations, LocationId.WreckedShipEastSuper, allWorlds.Count());
                var canBeatDraygon = CheckSphereLocationCount(sphereLocations, locations, LocationId.InnerMaridiaSpaceJump, allWorlds.Count());
                var canBeatRidley = CheckSphereLocationCount(sphereLocations, locations, LocationId.LowerNorfairRidleyTank, allWorlds.Count());
                var allCrateriaBosSKeys = CheckSphereCrateriaBossKeys(sphereLocations);

                // Make sure all players have the silver arrows
                if (sphereLocations.Count(x => x.Item.Type == ItemType.SilverArrows) < allWorlds.Count())
                {
                    return LocationUsefulness.Mandatory;
                }

                // Make sure all players have the master sword
                foreach (var world in allWorlds)
                {
                    if (sphereLocations.Count(x => x.Item.Type == ItemType.ProgressiveSword && x.Item.World == world) <
                        2)
                    {
                        return LocationUsefulness.Mandatory;
                    }
                }

                // Make sure the required amount of crystal dungeons are beatable
                foreach (var world in allWorlds)
                {
                    var numCrystalsNeeded = world.Config.GanonCrystalCount;
                    if (!world.Config.OpenPyramid && world.Config.GanonsTowerCrystalCount > numCrystalsNeeded)
                    {
                        numCrystalsNeeded = world.Config.GanonsTowerCrystalCount;
                    }
                    var currentCrystalCount = world.Dungeons.Count(d =>
                        d.IsCrystalDungeon && sphereLocations.Any(l =>
                            l.World.Id == world.Id && l.Id == s_dungeonBossLocations[d.GetType()]));
                    if (currentCrystalCount < numCrystalsNeeded)
                    {
                        return LocationUsefulness.Mandatory;
                    }
                }

                if (!canBeatGT || !canBeatKraid || !canBeatPhantoon || !canBeatDraygon || !canBeatRidley || !allCrateriaBosSKeys)
                {
                    return LocationUsefulness.Mandatory;
                }

                if (locations.Any(x => x.Item.Type == ItemType.ProgressiveSword))
                {
                    return LocationUsefulness.Sword;
                }

                var usefulItems = locations.Where(x => (x.Item.Progression && !x.Item.Type.IsInCategory(ItemCategory.Junk)) || x.Item.Type.IsInCategory(ItemCategory.Nice) || x.Item.Type == ItemType.ProgressiveSword).Select(x => x.Item);
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
        private bool CheckSphereLocationCount(IEnumerable<Location> sphereLocations, IEnumerable<Location> checkedLocations, LocationId locationId, int worldCount)
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
            var numKeysanity = sphereLocations.Select(x => x.World).Distinct().Count(x => x.Config.MetroidKeysanity);
            var numCratieriaBossKeys = sphereLocations.Select(x => x.Item.Type).Count(x => x == ItemType.CardMaridiaBoss);
            return numKeysanity == numCratieriaBossKeys;
        }

        private List<Hint> GetMedallionHints(World hintPlayerWorld)
        {
            var hints = new List<Hint>();

            hints.Add(() =>
            {
                var mmMedallion = hintPlayerWorld.MiseryMire.Medallion;
                return _gameLines.HintDungeonMedallion?.Format(hintPlayerWorld.MiseryMire.Metadata.Name, GetItemName(mmMedallion));
            });

            hints.Add(() =>
            {
                var trMedallion = hintPlayerWorld.TurtleRock.Medallion;
                return _gameLines.HintDungeonMedallion?.Format(hintPlayerWorld.TurtleRock.Metadata.Name, GetItemName(trMedallion));
            });

            return hints;
        }

        private string GetLocationsName(World hintPlayerWorld, IEnumerable<Location> locations)
        {
            if (locations.Count() == 1)
            {
                return GetLocationName(hintPlayerWorld, locations.First());
            }
            else
            {
                var room = locations.First().Room;

                if (room != null && locations.All(x => x.Room == room))
                {
                    var roomInfo = _metadataService.Room(room);
                    var name = roomInfo?.Name ?? room.Name;
                    return $"{name}{GetMultiworldSuffix(hintPlayerWorld, locations.First().World)}";
                }
                else
                {
                    var name = _metadataService.Region(locations.First().Region).Name;
                    return $"{name}{GetMultiworldSuffix(hintPlayerWorld, locations.First().World)}";
                }
            }
        }

        private string GetDungeonName(World hintPlayerWorld, IDungeon dungeon, Region region)
        {
            var dungeonName = _metadataService.Dungeon(dungeon).Name.ToString();
            return $"{dungeonName}{GetMultiworldSuffix(hintPlayerWorld, region.World)}";
        }

        private string GetLocationName(World hintPlayerWorld, Location location)
        {
            var name = $"{_metadataService.Region(location.Region).Name} - {_metadataService.Location(location.Id).Name}";
            return $"{name}{GetMultiworldSuffix(hintPlayerWorld, location.World)}";
        }

        private string GetItemName(World hintPlayerWorld, Item item)
        {
            var itemName = _metadataService.Item(item.Type)?.NameWithArticle;
            if (itemName == null || itemName.Contains('<'))
                itemName = item.Name;
            return $"{itemName}{GetMultiworldSuffix(hintPlayerWorld, item)}";
        }

        private string GetItemName(ItemType item)
        {
            var itemName = _metadataService.Item(item)?.NameWithArticle;
            if (itemName == null || itemName.Contains('<'))
                itemName = item.GetDescription();
            return itemName;
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
                    : $" belonging to {item.World.Player}";
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
                    : $" in {locationWorld.Player}'s world";
            }
        }

        private IEnumerable<Location> GetImportantLocations(IEnumerable<World> allWorlds)
        {
            // Get the first accessible ammo locations to make sure early ammo isn't considered mandatory when there
            // are others available
            var spheres = Playthrough.GenerateSpheres(allWorlds.SelectMany(x => x.Locations));
            var ammoItemTypes = new List<ItemType>()
            {
                ItemType.Missile,
                ItemType.Super,
                ItemType.PowerBomb,
                ItemType.ETank,
                ItemType.ReserveTank,
            };
            var ammoLocations = new List<Location>();
            foreach (var sphere in spheres)
            {
                foreach (var location in sphere.Locations.Where(x => ammoItemTypes.Contains(x.Item.Type)))
                {
                    if (ammoLocations.Count(x =>
                            x.Item.World.Id == location.Item.World.Id && x.Item.Type == location.Item.Type) < 3)
                    {
                        ammoLocations.Add(location);
                    }
                }
            }

            return allWorlds.SelectMany(w => w.Locations)
                .Where(l => s_importantLocations.Contains(l.Id) || l.Item.Progression ||
                            l.Item.Type.IsInAnyCategory(ItemCategory.SmallKey, ItemCategory.BigKey,
                                ItemCategory.Keycard) || l.Item.Type is ItemType.Super or ItemType.PowerBomb or ItemType.ProgressiveSword or ItemType.SilverArrows)
                .Concat(ammoLocations)
                .Distinct();
        }

        private enum LocationUsefulness
        {
            Useless,
            NiceToHave,
            Mandatory,
            Sword
        }
    }
}
