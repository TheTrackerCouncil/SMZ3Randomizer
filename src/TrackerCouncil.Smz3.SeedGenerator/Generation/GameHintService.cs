using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.AltGameModes;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public delegate PlayerHintTile? Hint();

/// <summary>
/// Service for generating hints for the player
/// </summary>
public class GameHintService(
    IMetadataService metadataService,
    ILogger<GameHintService> logger,
    PlaythroughService playthroughService,
    AltGameModeFactory altgameModeFactory)
    : IGameHintService
{
    private readonly GameLinesConfig _gameLines = metadataService.GameLines;
    private readonly HintTileConfig _hintTileConfig = metadataService.HintTiles;
    private Random _random = new();

    private static readonly Dictionary<LocationUsefulness, int> s_usefulnessPriority = new()
    {
        { LocationUsefulness.Mandatory, 0 },
        { LocationUsefulness.Goal, 1 },
        { LocationUsefulness.Key, 2 },
        { LocationUsefulness.Sword, 3 },
        { LocationUsefulness.NiceToHave, 4 },
        { LocationUsefulness.Useless, 5 },
    };

    public LocationUsefulness GetLocationUsefulness(Location location, List<World> allWorlds,
        Playthrough playthrough)
    {
        return CheckIfLocationsAreImportant(allWorlds, [location], null);
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

        var goalLocations = allWorlds.SelectMany(x => altgameModeFactory.GetGameModeLocations(x, allWorlds) ?? []).ToList();

        var allHints = new List<Hint>();

        if (hintPlayerWorld.Config.RomGenerator != RomGenerator.Cas)
        {
            allHints.AddRange(GetBasicLocationHints(hintPlayerWorld));
        }
        else
        {
            allHints.AddRange(GetProgressionItemHints(hintPlayerWorld, playthrough.Spheres, 5));
            allHints.AddRange(GetDungeonHints(hintPlayerWorld, allWorlds, goalLocations));
            allHints.AddRange(GetLocationHints(hintPlayerWorld, allWorlds, goalLocations));
            allHints.AddRange(GetMedallionHints(hintPlayerWorld));
        }

        allHints = allHints.Shuffle(_random).Take(hintPlayerWorld.Config.CasPatches.HintTiles).ToList();

        var selectedHints = new ConcurrentDictionary<long, PlayerHintTile>();

        // Get the number of requested hints
        Parallel.ForEach(allHints, (hint, _, index) =>
        {
            var hintLine = hint.Invoke();
            if (hintLine != null)
            {
                selectedHints[index] = hintLine;
            }
        });

        var worldHintTiles = new List<PlayerHintTile>();

        // Assign each hint to a tile
        var hintLocations = _hintTileConfig.HintTiles.Select(x => x.HintTileKey).Shuffle(_random);
        var hintIndex = 0;
        foreach (var hintLocation in hintLocations)
        {
            worldHintTiles.Add(selectedHints[hintIndex].GetHintTile(hintLocation));
            hintIndex = (hintIndex + 1) % selectedHints.Count;
        }

        logger.LogDebug("Hints: ");
        foreach (var hint in worldHintTiles)
        {
            var hintText = GetHintTileText(hint, hintPlayerWorld, allWorlds);
            logger.LogDebug("{Hint}", hintText);
        }

        hintPlayerWorld.HintTiles = worldHintTiles;
    }

    public (Location Location, LocationUsefulness Usefulness)? FindMostValueableLocation(List<World> allWorlds,
        List<Location> locationsToCheck, List<Location>? goalLocations)
    {
        locationsToCheck = locationsToCheck.Where(l => l.Item.Progression ||
                                                       l.Item.Type.IsInAnyCategory(ItemCategory.SmallKey,
                                                           ItemCategory.BigKey, ItemCategory.Keycard,
                                                           ItemCategory.PossibleProgression,
                                                           ItemCategory.ProgressionOnLimitedAmount)).ToList();

        if (!locationsToCheck.Any())
        {
            return null;
        }

        goalLocations ??= allWorlds.SelectMany(x => altgameModeFactory.GetGameModeLocations(x, allWorlds) ?? []).ToList();

        var locationUsefulness = new ConcurrentBag<(Location Location, LocationUsefulness Usefulness)>();

        Parallel.ForEach(locationsToCheck, location =>
        {
            var usefulness =
                CheckIfLocationsAreImportant(allWorlds, [location], goalLocations);
            locationUsefulness.Add((location, usefulness));
        });

        return locationUsefulness.MinBy(x => s_usefulnessPriority[x.Usefulness]);
    }


    public LocationUsefulness GetUsefulness(List<Location> locations, List<World> allWorlds, Reward? ignoredReward, List<Location>? goalLocations = null)
    {
        goalLocations ??= allWorlds.SelectMany(x => altgameModeFactory.GetGameModeLocations(x, allWorlds) ?? []).ToList();
        return CheckIfLocationsAreImportant(allWorlds, locations, goalLocations, ignoredReward);
    }

    /// <summary>
    /// Retrieves hints stating the location of items for the current player
    /// that are in the provided spheres
    /// </summary>
    private IEnumerable<Hint> GetProgressionItemHints(World hintPlayerWorld, IEnumerable<Playthrough.Sphere> spheres, int count)
    {
        // Grab items for the player marked as progression that are not junk
        var progressionLocations = spheres
            .SelectMany(x => x.Locations)
            .Where(x => x.Item.World == hintPlayerWorld && x.Item.Type.IsInCategory(ItemCategory.PossibleProgression) &&
                        !x.Item.Type.IsInCategory(ItemCategory.Scam)).ToList();
        progressionLocations =
            progressionLocations.TakeLast(progressionLocations.Count() / 3).Shuffle(_random).Take(count).ToList();

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
    private IEnumerable<Hint> GetDungeonHints(World hintPlayerWorld, List<World> allWorlds, List<Location> goalLocations)
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
            var usefulness = CheckIfLocationsAreImportant(allWorlds, locations, goalLocations);
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
    private IEnumerable<Hint> GetLocationHints(World hintPlayerWorld, List<World> allWorlds, List<Location> goalLocations)
    {
        var hints = new List<Hint>();

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PinkBrinstarWaterwayEnergyTank).ToList(), goalLocations));

        // Wrecked pool
        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.WreckedShipEnergyTank).ToList(), goalLocations));

        // Shaktool
        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaSpringBall).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaPlasma).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Sahasrahla).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.MasterSwordPedestal).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.KingZora).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Catfish).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.TowerOfHeraBigKeyChest).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.RedBrinstarXRayScope).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PinkBrinstarHoptank).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.EtherTablet).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.BombosTablet).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.CrateriaSuper).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.SpikeCave).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.SwampPalaceWestChest or LocationId.SwampPalaceBigKeyChest).ToList(), goalLocations, "The left side of swamp palace"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x =>
                x.Id is LocationId.WreckedShipGravitySuit or LocationId.WreckedShipBowlingAlleyTop
                    or LocationId.WreckedShipBowlingAlleyBottom).ToList(), goalLocations, "Wrecked Ship post Chozo concert area"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.DarkWorldNorthEast.PyramidFairy.Locations.ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.DarkWorldSouth.HypeCave.Locations.ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.DarkWorldDeathMountainEast.HookshotCave.Locations.ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.LightWorldDeathMountainEast.ParadoxCave.Locations.ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.UpperNorfairCrocomire.Locations.ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.InnerMaridia.LeftSandPit.Locations.ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.InnerMaridia.RightSandPit.Locations.ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.InnerMaridiaAqueductLeft or LocationId.InnerMaridiaAqueductRight).ToList(), goalLocations, "Inner Maridia Aqueduct"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.Blacksmith or LocationId.PurpleChest).ToList(), goalLocations, "Smith chain"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.CrateriaGauntletEnergyTank or LocationId.CrateriaGauntletShaftLeft or LocationId.CrateriaGauntletShaftRight).ToList(), goalLocations, "Crateria gauntlet"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.FluteSpot).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.MagicBat).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.PotionShop).ToList(), goalLocations));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x =>
                x.Id is LocationId.GreenBrinstarEarlySupersBottom or LocationId.GreenBrinstarEarlySupersTop
                    or LocationId.GreenBrinstarReserveTankChozo or LocationId.GreenBrinstarReserveTankHidden
                    or LocationId.GreenBrinstarReserveTankVisible).ToList(), goalLocations,
            "Green Brinstar machball hall"));

        hints.Add(() => GetLocationHint(hintPlayerWorld, allWorlds,
            hintPlayerWorld.Locations.Where(x => x.Id is LocationId.GreenBrinstarEtecoonSuper or LocationId.GreenBrinstarEtecoonEnergyTank or LocationId.GreenBrinstarMainShaft).ToList(), goalLocations, "Green Brinstar bottom left area"));

        return hints;
    }

    /// <summary>
    /// Adds a hint for a given set of location(s) to the list of hints
    /// </summary>
    private PlayerHintTile GetLocationHint(World hintPlayerWorld, List<World> allWorlds, List<Location> locations, List<Location> goalLocations, string? areaName = null)
    {
        var typeAndKey = GetHintTileTypeAndKey(locations, areaName);

        return new PlayerHintTile()
        {
            Type = typeAndKey.Item1,
            WorldId = hintPlayerWorld.Id,
            LocationKey = typeAndKey.Item2,
            LocationWorldId = locations.First().World.Id,
            Locations = locations.Select(x => x.Id),
            Usefulness = locations.Count > 1 ? CheckIfLocationsAreImportant(allWorlds, locations, goalLocations) : null
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
                    location.World.Config.MetroidKeysanity, location.Item.IsLocalPlayerItem)
                    ? LocationUsefulness.NiceToHave
                    : LocationUsefulness.Useless
        };
    }

    /// <summary>
    /// Checks how useful a location is based on if the seed can be completed if we remove those
    /// locations from the playthrough and if the items there are at least slightly useful
    /// </summary>
    private LocationUsefulness CheckIfLocationsAreImportant(List<World> allWorlds, List<Location> locations, List<Location>? goalLocations, Reward? ignoredReward = null, List<Item>? defaultItems = null)
    {
        var worldLocations = allWorlds.SelectMany(x => x.Locations)
            .Select(x => locations.Contains(x) ? new Location(x, ItemType.TwentyRupees) : x)
            .ToList();

        try
        {
            playthroughService.GenerateSpheres(worldLocations, out var fullyCollected, ignoredReward, defaultItems, true);

            var usefulness = allWorlds.Select(world => VerifyWorld(fullyCollected, world, locations, goalLocations)).ToList();

            // If it's thought to be mandatory and any world has key sanity, we need to make sure it's not a key preventing a
            if (usefulness.Contains(LocationUsefulness.Mandatory) && allWorlds.Any(x => x.Config.Keysanity))
            {
                var checkMandatory = VerifyMandatoryUsefulness(allWorlds, locations, goalLocations, ignoredReward);
                if (checkMandatory == LocationUsefulness.Key)
                {
                    usefulness.RemoveAll(x => x == LocationUsefulness.Mandatory);
                    usefulness.Add(LocationUsefulness.Key);
                }
            }

            return usefulness.Count == 1 ? usefulness[0] : usefulness.OrderBy(x => s_usefulnessPriority[x]).First();
        }
        catch
        {
            return VerifyMandatoryUsefulness(allWorlds, locations, goalLocations, ignoredReward);
        }
    }

    private LocationUsefulness VerifyWorld(Playthrough.Sphere sphere, World world, List<Location> checkedLocations, List<Location>? goalLocations)
    {
        var worldLocations = sphere.Locations.Where(x => x.World == world).ToDictionary(x => x.Id, x => x);

        var progression = new Progression(sphere.Items.Where(x => x.World == world),
            sphere.Rewards.Where(x => x.World == world), sphere.Bosses.Where(x => x.World == world));

        // If the player must fight Ganon and Mother Brain
        if (!world.Config.GameModeOptions.LiftOffOnGoalCompletion)
        {
            // Make sure the player can defeat Ganon
            if (!progression.MasterSword || !progression.Contains(ItemType.SilverArrows))
            {
                return LocationUsefulness.Mandatory;
            }

            // Make sure the player can access tourian
            if (world.Config is { MetroidKeysanity: true, GameModeOptions.SkipTourianBossDoor: false } &&
                !progression.Contains(ItemType.CardCrateriaBoss))
            {
                return LocationUsefulness.Mandatory;
            }

            // Make sure the player can access Ganon
            if (!world.Config.GameModeOptions.OpenPyramid && !worldLocations.ContainsKey(LocationId.GanonsTowerMoldormChest))
            {
                return LocationUsefulness.Mandatory;
            }
        }

        // If vanilla game mode, make sure the player can get the required crystal counts
        if (world.Config.GameModeOptions.SelectedGameModeType == GameModeType.Vanilla && (progression.CrystalCount < world.Config.GameModeOptions.GanonCrystalCount ||
                progression.MetroidBossCount < world.Config.GameModeOptions.TourianBossCount))
        {
            return LocationUsefulness.Mandatory;
        }
        // If it's a different game mode, return that it's goal blocking if it's either a goal location or prevents access to a goal location
        else if (world.Config.GameModeOptions.SelectedGameModeType != GameModeType.Vanilla && goalLocations?.Any(x =>
                     !x.IsAvailable(progression) || checkedLocations.Any(y => y.Id == x.Id && y.World == x.World)) == true)
        {
            return LocationUsefulness.Goal;
        }

        if (checkedLocations.Any(x => x.Item.Type == ItemType.ProgressiveSword))
        {
            return LocationUsefulness.Sword;
        }

        var usefulItems = checkedLocations.Where(x =>
                (x.Item.Type.IsInCategory(ItemCategory.PossibleProgression) && !x.Item.Type.IsInCategory(ItemCategory.Junk)) ||
                x.Item.Type.IsInCategory(ItemCategory.Nice) || x.Item.Type == ItemType.ProgressiveSword)
            .Select(x => x.Item);
        return usefulItems.Any() ? LocationUsefulness.NiceToHave : LocationUsefulness.Useless;
    }

    private LocationUsefulness VerifyMandatoryUsefulness(List<World> allWorlds, List<Location> locations, List<Location>? goalLocations, Reward? ignoredReward = null)
    {
        var keysanityKeyLocations = locations.Where(x => IsKeysanityKeyItemLocation(x.Item)).ToList();
        if (keysanityKeyLocations.Count == 0)
        {
            return LocationUsefulness.Mandatory;
        }

        var remainingLocations = locations.Where(x => !keysanityKeyLocations.Contains(x)).ToList();
        var keyItems = keysanityKeyLocations.Select(x => x.Item).ToList();
        var verifiedUsefulness = CheckIfLocationsAreImportant(allWorlds, remainingLocations, goalLocations, ignoredReward, keyItems);
        if (verifiedUsefulness == LocationUsefulness.Mandatory)
        {
            return verifiedUsefulness;
        }
        else
        {
            return LocationUsefulness.Key;
        }
    }

    private bool IsKeysanityKeyItemLocation(Item item)
    {
        return (item.Type.IsInAnyCategory(ItemCategory.SmallKey, ItemCategory.BigKey) &&
                item.World.Config.ZeldaKeysanity) ||
               (item.Type.IsInCategory(ItemCategory.Keycard) && item.World.Config.MetroidKeysanity);
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
        var dungeonName = metadataService.Region(region).Name?.ToString() ?? hasTreasure.Name;
        return $"{dungeonName}{GetMultiworldSuffix(hintPlayerWorld, region.World)}";
    }

    private string GetRoomName(World hintPlayerWorld, Room room)
    {
        var roomName = metadataService.Room(room)?.Name?.ToString() ?? room.Name;
        return $"{roomName}{GetMultiworldSuffix(hintPlayerWorld, room.World)}";
    }

    private string GetRegionName(World hintPlayerWorld, Region region)
    {
        var dungeonName = metadataService.Region(region).Name?.ToString() ?? region.Name;
        return $"{dungeonName}{GetMultiworldSuffix(hintPlayerWorld, region.World)}";
    }

    private string GetLocationName(World hintPlayerWorld, Location location)
    {
        var name = $"{metadataService.Region(location.Region).Name} - {metadataService.Location(location.Id).Name}";
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

        var itemName = metadataService.Item(item.Type)?.NameWithArticle;
        if (itemName == null || itemName.Contains('<'))
            itemName = item.Name;
        return $"{itemName}{GetMultiworldSuffix(hintPlayerWorld, item)}";
    }

    private string GetItemName(ItemType item)
    {
        var itemName = metadataService.Item(item)?.NameWithArticle;
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

    public string? GetHintTileText(PlayerHintTile tile, World hintPlayerWorld, List<World> worlds)
    {
        if (tile.Type == HintTileType.Requirement && tile.MedallionType != null)
        {
            var dungeon = metadataService.Regions.FirstOrDefault(x => x.Region == tile.LocationKey);
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
            else if (usefulness == LocationUsefulness.Key)
            {
                return _gameLines.HintLocationHasKey?.Format(areaName);
            }
            else if (usefulness == LocationUsefulness.NiceToHave)
            {
                return _gameLines.HintLocationHasUsefulItem?.Format(areaName);
            }
            else if (usefulness == LocationUsefulness.Sword)
            {
                return _gameLines.HintLocationHasSword?.Format(areaName);
            }
            else if (usefulness == LocationUsefulness.Goal)
            {
                return _gameLines.HintLocationAltGoal?.Format(areaName);
            }
            else
            {
                return _gameLines.HintLocationEmpty?.Format(areaName);
            }
        }
    }


}
