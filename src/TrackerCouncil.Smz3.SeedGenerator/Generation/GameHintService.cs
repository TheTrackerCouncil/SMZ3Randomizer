using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public delegate PlayerHintTile? Hint();

/// <summary>
/// Service for generating hints for the player
/// </summary>
public class GameHintService : IGameHintService
{
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
    private readonly HintTileConfig _hintTileConfig;
    private readonly PlaythroughService _playthroughService;
    private Random _random;

    public GameHintService(Configs configs, IMetadataService metadataService, ILogger<GameHintService> logger, PlaythroughService playthroughService)
    {
        _gameLines = configs.GameLines;
        _logger = logger;
        _playthroughService = playthroughService;
        _hintTileConfig = configs.HintTileConfig;
        _metadataService = metadataService;
        _random = new Random();
    }

    public LocationUsefulness GetLocationUsefulness(Location location, List<World> allWorlds,
        Playthrough playthrough)
    {
        var importantLocations = GetImportantLocations(allWorlds);
        return CheckIfLocationsAreImportant(allWorlds, importantLocations, new List<Location>() { location });
    }

    /// <summary>
    /// Retrieves the hints to display in game for the player
    /// </summary>
    /// <param name="hintPlayerWorld">The player that will be receiving the hints</param>
    /// <param name="allWorlds">All worlds that are a part of the seed</param>
    /// <param name="playthrough">The initial playthrough with all of the spheres</param>
    /// <param name="seed">Seed number for shuffling and randomization</param>
    /// <returns>A collection of strings to use for the in game hints</returns>
    public void GetInGameHints(World hintPlayerWorld, List<World> allWorlds, Playthrough playthrough, int seed)
    {
        if (hintPlayerWorld.Config.CasPatches.HintTiles <= 0 || _hintTileConfig.HintTiles == null)
        {
            return;
        }
        _random = new Random(seed).Sanitize();

        var allHints = new List<Hint>();

        if (hintPlayerWorld.Config.RomGenerator != RomGenerator.Cas)
        {
            allHints.AddRange(GetBasicLocationHints(hintPlayerWorld));
        }
        else
        {
            var importantLocations = GetImportantLocations(allWorlds).ToList();
            allHints.AddRange(GetProgressionItemHints(hintPlayerWorld, playthrough.Spheres, 5));
            allHints.AddRange(GetDungeonHints(hintPlayerWorld, allWorlds, importantLocations));
            allHints.AddRange(GetLocationHints(hintPlayerWorld, allWorlds, importantLocations));
            allHints.AddRange(GetMedallionHints(hintPlayerWorld));
        }

        allHints = allHints.Shuffle(_random);

        var selectedHints = new List<PlayerHintTile>();

        // Get the number of requested hints
        while (selectedHints.Count < hintPlayerWorld.Config.CasPatches.HintTiles && allHints.Any())
        {
            var hint = allHints.First();
            allHints.Remove(hint);
            var hintLine = hint.Invoke();
            if (hintLine != null)
            {
                selectedHints.Add(hintLine);
            }
        }

        var worldHintTiles = new List<PlayerHintTile>();

        // Assign each hint to a tile
        var hintLocations = _hintTileConfig.HintTiles.Select(x => x.HintTileKey).Shuffle(_random);
        var hintIndex = 0;
        foreach (var hintLocation in hintLocations)
        {
            worldHintTiles.Add(selectedHints[hintIndex].GetHintTile(hintLocation));
            hintIndex = (hintIndex + 1) % selectedHints.Count;
        }

        _logger.LogDebug("Hints: ");
        foreach (var hint in worldHintTiles)
        {
            var hintText = GetHintTileText(hint, hintPlayerWorld, allWorlds);
            _logger.LogDebug("{Hint}", hintText);
        }

        hintPlayerWorld.HintTiles = worldHintTiles;
    }

    public (Location Location, LocationUsefulness Usefulness)? FindMostValueableLocation(List<World> allWorlds,
        List<Location> locationsToCheck)
    {
        var importantLocations = GetImportantLocations(allWorlds);

        locationsToCheck = locationsToCheck.Where(l => s_importantLocations.Contains(l.Id) || l.Item.Progression ||
                                                       l.Item.Type.IsInAnyCategory(ItemCategory.SmallKey, ItemCategory.BigKey, ItemCategory.Keycard) ||
                                                       l.Item.Type is ItemType.Super or ItemType.PowerBomb or ItemType.ProgressiveSword or ItemType.SilverArrows).ToList();

        if (!locationsToCheck.Any())
        {
            return null;
        }

        var locationUsefulness = new ConcurrentBag<(Location Location, LocationUsefulness Usefulness)>();

        Parallel.ForEach(locationsToCheck, location =>
        {
            var usefulness =
                CheckIfLocationsAreImportant(allWorlds, importantLocations, new List<Location>() { location });
            locationUsefulness.Add((location, usefulness));
        });

        return locationUsefulness.MaxBy(x => (int)x.Usefulness);
    }

    public LocationUsefulness GetUsefulness(List<Location> locations, List<World> allWorlds, Reward? ignoredReward)
    {
        var importantLocations = GetImportantLocations(allWorlds);

        locations = locations.Where(l => s_importantLocations.Contains(l.Id) || l.Item.Progression ||
                                         l.Item.Type.IsInAnyCategory(ItemCategory.SmallKey, ItemCategory.BigKey, ItemCategory.Keycard) ||
                                         l.Item.Type is ItemType.Super or ItemType.PowerBomb or ItemType.ProgressiveSword or ItemType.SilverArrows).ToList();

        if (!locations.Any())
        {
            return LocationUsefulness.Useless;
        }

        return CheckIfLocationsAreImportant(allWorlds, importantLocations, locations, ignoredReward);
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
                        !x.Item.Type.IsInAnyCategory(ItemCategory.Junk, ItemCategory.Scam)).ToList();
        progressionLocations =
            progressionLocations.TakeLast(progressionLocations.Count() / 2).Shuffle(_random).Take(count).ToList();

        return progressionLocations.Select(x => new Hint(() => GetProgressionItemHint(x)));

        PlayerHintTile GetProgressionItemHint(Location location)
        {
            return new PlayerHintTile()
            {
                Type = HintTileType.Location,
                WorldId = hintPlayerWorld.Id,
                LocationKey = GetLocationName(hintPlayerWorld, location),
                LocationWorldId = location.World.Id,
                Locations = new List<LocationId>() { location.Id },
            };
        }
    }

    /// <summary>
    /// Retrives hints stating how important dungeons are to beating the game
    /// </summary>
    private IEnumerable<Hint> GetDungeonHints(World hintPlayerWorld, List<World> allWorlds, List<Location> importantLocations)
    {
        var dungeons = hintPlayerWorld.RewardRegions.Where(x =>
                x.RewardType is RewardType.PendantBlue or RewardType.PendantGreen or RewardType.PendantRed)
            .Cast<Region>()
            .Concat([hintPlayerWorld.HyruleCastle, hintPlayerWorld.GanonsTower])
            .OfType<IHasTreasure>();
        var hints = new List<Hint>();

        foreach (var dungeon in dungeons)
        {
            hints.Add(() => GetDungeonHint(dungeon));
        }

        return hints;

        PlayerHintTile GetDungeonHint(IHasTreasure dungeon)
        {
            var dungeonRegion = (Region)dungeon;
            var locations = dungeonRegion.Locations.Where(x => x.Type != LocationType.NotInDungeon).ToList();
            var usefulness = CheckIfLocationsAreImportant(allWorlds, importantLocations, locations);
            return new PlayerHintTile()
            {
                Type = HintTileType.Dungeon,
                WorldId = hintPlayerWorld.Id,
                LocationKey = dungeon.Name,
                LocationWorldId = locations.First().World.Id,
                Locations = locations.Select(x => x.Id),
                Usefulness = usefulness
            };
        }
    }

    private IEnumerable<Hint> GetBasicLocationHints(World hintPlayerWorld)
    {
        List<LocationId> locationIds =
        [
            LocationId.PinkBrinstarWaterwayEnergyTank, LocationId.WreckedShipEnergyTank,
            LocationId.InnerMaridiaSpringBall, LocationId.InnerMaridiaPlasma, LocationId.Sahasrahla,
            LocationId.MasterSwordPedestal, LocationId.KingZora, LocationId.Catfish,
            LocationId.TowerOfHeraBigKeyChest, LocationId.RedBrinstarXRayScope, LocationId.PinkBrinstarHoptank,
            LocationId.EtherTablet, LocationId.BombosTablet, LocationId.CrateriaSuper, LocationId.SpikeCave,
            LocationId.PurpleChest, LocationId.FluteSpot, LocationId.MagicBat, LocationId.PotionShop,
            LocationId.MasterSwordPedestal
        ];

        return locationIds
            .Select(locationId => (Hint)(() => GetBasicLocationHint(hintPlayerWorld.FindLocation(locationId))))
            .ToList();
    }

    /// <summary>
    /// Retrieves hints for out of the way locations and rooms to the pool of hints
    /// </summary>
    private IEnumerable<Hint> GetLocationHints(World hintPlayerWorld, List<World> allWorlds, List<Location> importantLocations)
    {
        var hints = new List<Hint>();

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PinkBrinstarWaterwayEnergyTank).ToList()));

        // Wrecked pool
        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.WreckedShipEnergyTank).ToList()));

        // Shaktool
        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaSpringBall).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaPlasma).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Sahasrahla).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.MasterSwordPedestal).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.KingZora).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Catfish).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.TowerOfHeraBigKeyChest).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.RedBrinstarXRayScope).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PinkBrinstarHoptank).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.EtherTablet).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.BombosTablet).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.CrateriaSuper).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.SpikeCave).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.SwampPalaceWestChest or LocationId.SwampPalaceBigKeyChest).ToList(), "The left side of swamp palace"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x =>
                x.Id is LocationId.WreckedShipGravitySuit or LocationId.WreckedShipBowlingAlleyTop
                    or LocationId.WreckedShipBowlingAlleyBottom).ToList(), "Wrecked Ship post Chozo concert area"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.DarkWorldNorthEast.PyramidFairy.Locations.ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.DarkWorldSouth.HypeCave.Locations.ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.DarkWorldDeathMountainEast.HookshotCave.Locations.ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.LightWorldDeathMountainEast.ParadoxCave.Locations.ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.UpperNorfairCrocomire.Locations.ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.InnerMaridia.LeftSandPit.Locations.ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.InnerMaridia.RightSandPit.Locations.ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaAqueductLeft or LocationId.InnerMaridiaAqueductRight).ToList(), "Inner Maridia Aqueduct"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Blacksmith or LocationId.PurpleChest).ToList(), "Smith chain"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.CrateriaGauntletEnergyTank or LocationId.CrateriaGauntletShaftLeft or LocationId.CrateriaGauntletShaftRight).ToList(), "Crateria gauntlet"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.FluteSpot).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.MagicBat).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PotionShop).ToList()));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.GreenBrinstarEarlySupersBottom or LocationId.GreenBrinstarEarlySupersTop or LocationId.GreenBrinstarReserveTankChozo or LocationId.GreenBrinstarReserveTankHidden or LocationId.GreenBrinstarReserveTankVisible).ToList(), "Green Brinstar machball hall"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds, importantLocations,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.GreenBrinstarEtecoonSuper or LocationId.GreenBrinstarEtecoonEnergyTank or LocationId.GreenBrinstarMainShaft).ToList(), "Green Brinstar bottom left area"));

        return hints;
    }

    /// <summary>
    /// Adds a hint for a given set of location(s) to the list of hints
    /// </summary>
    private PlayerHintTile GetLocationHint(World hintPlayerWorld, List<World> allWorlds, List<Location> importantLocations, List<Location> locations, string? areaName = null)
    {
        var typeAndKey = GetHintTileTypeAndKey(locations, areaName);

        return new PlayerHintTile()
        {
            Type = typeAndKey.Item1,
            WorldId = hintPlayerWorld.Id,
            LocationKey = typeAndKey.Item2,
            LocationWorldId = locations.First().World.Id,
            Locations = locations.Select(x => x.Id),
            Usefulness = locations.Count > 1 ? CheckIfLocationsAreImportant(allWorlds, importantLocations, locations) : null
        };
    }

    /// <summary>
    /// Adds a hint for a given set of location(s) to the list of hints
    /// </summary>
    private PlayerHintTile GetBasicLocationHint(Location location)
    {
        return new PlayerHintTile()
        {
            Type = HintTileType.Location,
            WorldId = location.World.Id,
            LocationKey = location.Name,
            LocationWorldId = location.World.Id,
            Locations = [location.Id],
            Usefulness =
                location.ItemType.IsPossibleProgression(location.World.Config.ZeldaKeysanity,
                    location.World.Config.MetroidKeysanity)
                    ? LocationUsefulness.NiceToHave
                    : LocationUsefulness.Useless
        };
    }

    /// <summary>
    /// Checks how useful a location is based on if the seed can be completed if we remove those
    /// locations from the playthrough and if the items there are at least slightly useful
    /// </summary>
    private LocationUsefulness CheckIfLocationsAreImportant(List<World> allWorlds, IEnumerable<Location> importantLocations, List<Location> locations, Reward? ignoredReward = null)
    {
        var worldLocations = importantLocations.Except(locations).ToList();
        try
        {
            var spheres = _playthroughService.GenerateSpheres(worldLocations, ignoredReward);
            var sphereLocations = spheres.SelectMany(x => x.Locations).ToList();

            var canBeatGT = CheckSphereLocationCount(sphereLocations, locations, LocationId.GanonsTowerMoldormChest, allWorlds.Count);
            var canBeatKraid = CheckSphereLocationCount(sphereLocations, locations, LocationId.KraidsLairVariaSuit, allWorlds.Count);
            var canBeatPhantoon = CheckSphereLocationCount(sphereLocations, locations, LocationId.WreckedShipEastSuper, allWorlds.Count);
            var canBeatDraygon = CheckSphereLocationCount(sphereLocations, locations, LocationId.InnerMaridiaSpaceJump, allWorlds.Count);
            var canBeatRidley = CheckSphereLocationCount(sphereLocations, locations, LocationId.LowerNorfairRidleyTank, allWorlds.Count);
            var allCrateriaBossKeys = CheckSphereCrateriaBossKeys(sphereLocations);

            // Make sure all players have the silver arrows
            if (sphereLocations.Count(x => x.Item.Type == ItemType.SilverArrows) < allWorlds.Count)
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
                var currentCrystalCount = world.RewardRegions.Count(d =>
                    d.RewardType is RewardType.CrystalBlue or RewardType.CrystalRed && sphereLocations.Any(l =>
                        l.World.Id == world.Id && l.Id == s_dungeonBossLocations[d.GetType()])) + (ignoredReward == null ? 0 : 1);
                if (currentCrystalCount < numCrystalsNeeded)
                {
                    return LocationUsefulness.Mandatory;
                }
            }

            if (!canBeatGT || !canBeatKraid || !canBeatPhantoon || !canBeatDraygon || !canBeatRidley || !allCrateriaBossKeys)
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
    private bool CheckSphereCrateriaBossKeys(List<Location> sphereLocations)
    {
        var numKeysanity = sphereLocations.Select(x => x.World).Distinct().Count(x => x.Config.MetroidKeysanity);
        var numCrateriaBossKeys = sphereLocations.Select(x => x.Item.Type).Count(x => x == ItemType.CardCrateriaBoss);
        return numKeysanity == numCrateriaBossKeys;
    }

    private List<Hint> GetMedallionHints(World hintPlayerWorld)
    {
        var hints = new List<Hint>();

        hints.Add(() =>
        {
            var mmMedallion = hintPlayerWorld.MiseryMire.PrerequisiteState.RequiredItem;
            return new PlayerHintTile()
            {
                Type = HintTileType.Requirement,
                WorldId = hintPlayerWorld.Id,
                LocationKey = hintPlayerWorld.MiseryMire.Name,
                MedallionType = mmMedallion
            };
        });

        hints.Add(() =>
        {
            var trMedallion = hintPlayerWorld.TurtleRock.PrerequisiteState.RequiredItem;
            return new PlayerHintTile()
            {
                Type = HintTileType.Requirement,
                WorldId = hintPlayerWorld.Id,
                LocationKey = hintPlayerWorld.TurtleRock.Name,
                MedallionType = trMedallion
            };
        });

        return hints;
    }

    private (HintTileType, string) GetHintTileTypeAndKey(List<Location> locations, string? areaName)
    {
        if (locations.Count == 1)
        {
            return (HintTileType.Location, locations.First().Name);
        }
        else if (!string.IsNullOrEmpty(areaName))
        {
            return (HintTileType.LocationGroup, areaName);
        }
        else
        {
            var room = locations.First().Room;

            if (room != null && locations.All(x => x.Room == room))
            {
                return (HintTileType.Room, room.Name);
            }
            else
            {
                return (HintTileType.Region, locations.First().Region.Name);
            }
        }
    }

    private string GetDungeonName(World hintPlayerWorld, IHasTreasure hasTreasure, Region region)
    {
        var dungeonName = _metadataService.Dungeon(hasTreasure).Name?.ToString() ?? hasTreasure.Name;
        return $"{dungeonName}{GetMultiworldSuffix(hintPlayerWorld, region.World)}";
    }

    private string GetRoomName(World hintPlayerWorld, Room room)
    {
        var roomName = _metadataService.Room(room)?.Name?.ToString() ?? room.Name;
        return $"{roomName}{GetMultiworldSuffix(hintPlayerWorld, room.World)}";
    }

    private string GetRegionName(World hintPlayerWorld, Region region)
    {
        var dungeonName = _metadataService.Region(region).Name?.ToString() ?? region.Name;
        return $"{dungeonName}{GetMultiworldSuffix(hintPlayerWorld, region.World)}";
    }

    private string GetLocationName(World hintPlayerWorld, Location location)
    {
        var name = $"{_metadataService.Region(location.Region).Name} - {_metadataService.Location(location.Id).Name}";
        return $"{name}{GetMultiworldSuffix(hintPlayerWorld, location.World)}";
    }

    private string GetItemName(World hintPlayerWorld, Item item)
    {
        if (item.Type == ItemType.OtherGameProgressionItem)
        {
            return $"something potentially required for {item.PlayerName}";
        }
        else if (item.Type == ItemType.OtherGameItem)
        {
            return $"some junk for {item.PlayerName}";
        }

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
            return item.IsLocalPlayerItem ? "" : $" belonging to {item.PlayerName}";
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

    private IEnumerable<Location> GetImportantLocations(List<World> allWorlds)
    {
        // Get the first accessible ammo locations to make sure early ammo isn't considered mandatory when there
        // are others available
        var spheres = _playthroughService.GenerateSpheres(allWorlds.SelectMany(x => x.Locations));
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

    public string? GetHintTileText(PlayerHintTile tile, World hintPlayerWorld, List<World> worlds)
    {
        if (tile.Type == HintTileType.Requirement && tile.MedallionType != null)
        {
            var dungeon = _metadataService.Dungeons.FirstOrDefault(x => x.Dungeon == tile.LocationKey);
            var dungeonName = dungeon?.Name?.ToString() ?? tile.LocationKey;
            var itemName = GetItemName(tile.MedallionType.Value);
            return _gameLines.HintDungeonMedallion?.Format(dungeonName, itemName);
        }
        else if (tile.Type == HintTileType.Location && tile.Locations?.Any() == true)
        {
            var world = worlds.First(x => x.Id == tile.LocationWorldId);
            var location = world.FindLocation(tile.Locations.First());
            var areaName = GetLocationName(hintPlayerWorld, location);
            return  _gameLines.HintLocationHasItem?.Format(areaName,
                GetItemName(hintPlayerWorld, location.Item));
        }
        else
        {
            var areaName = tile.LocationKey;
            var world = worlds.First(x => x.Id == tile.LocationWorldId);

            if (tile.Type == HintTileType.Region)
            {
                var region = world.Regions.First(x => x.Name == areaName);
                areaName = GetRegionName(hintPlayerWorld, region);
            }
            else if (tile.Type == HintTileType.Dungeon)
            {
                var region = world.Regions.First(x => x.Name == areaName);
                var dungeon = world.TreasureRegions.First(x => x.Name == areaName);
                areaName = GetDungeonName(hintPlayerWorld, dungeon, region);
            }
            else if (tile.Type == HintTileType.Room)
            {
                var room = world.Rooms.First(x => x.Name == areaName);
                areaName = GetRoomName(hintPlayerWorld, room);
            }
            else if (tile.Type == HintTileType.LocationGroup)
            {
                areaName = $"{areaName}{GetMultiworldSuffix(hintPlayerWorld, world)}";
            }

            var usefulness = tile.Usefulness;

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
    }


}
