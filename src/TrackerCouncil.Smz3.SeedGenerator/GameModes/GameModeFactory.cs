using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.GameModes;

public class GameModeFactory(IServiceProvider serviceProvider)
{
    private static readonly Dictionary<GameModeType, Type> s_gameModeTypes = [];
    private static readonly Dictionary<GameModeType, string> s_gameModeDescriptions = new();

    public static void AddGameModeClass(Type gameModeType)
    {
        var gameModeTypeAttribute = gameModeType.GetCustomAttribute<GameModeTypeAttribute>();
        if (gameModeTypeAttribute == null)
        {
            throw new InvalidOperationException($"GameModeType {gameModeType.Name} is missing GameModeTypeAttribute");
        }

        var descriptionAttribute = gameModeType.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttribute == null)
        {
            throw new InvalidOperationException($"GameModeType {gameModeType.Name} is missing DescriptionAttribute");
        }

        s_gameModeTypes.Add(gameModeTypeAttribute.GameModeType, gameModeType);
        s_gameModeDescriptions.Add(gameModeTypeAttribute.GameModeType, descriptionAttribute.Description);
    }

    public GameModeBase GetGameMode(GameModeType gameMode)
    {
        if (!s_gameModeTypes.TryGetValue(gameMode, out var gameModeClassType))
        {
            throw new InvalidOperationException($"GameModeType {gameMode} does not have a matching GameMode class");
        }

        return serviceProvider.GetService(gameModeClassType) as GameModeBase ??
               throw new InvalidOperationException($"GameModeType {gameMode} does not have a valid GameMode class");
    }

    public World UpdateWorld(World world, int seed)
    {
        var gameModeOptions = world.Config.GameModeOptions;

        var rng = new Random(seed);
        rng.Sanitize();

        var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);

        gameMode.UpdateWorld(world, rng, gameModeOptions);

        if (gameModeOptions.RandomizeNumericAmounts)
        {
            gameModeOptions.GanonCrystalCount = rng.Next(gameModeOptions.MinGanonCrystalCount, gameModeOptions.MaxGanonCrystalCount + 1);
            gameModeOptions.TourianBossCount = rng.Next(gameModeOptions.MinTourianBossCount, gameModeOptions.MaxTourianBossCount + 1);
        }

        return world;
    }

    public string? GetGameStartText(World world)
    {
        var gameModeOptions = world.Config.GameModeOptions;
        var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);
        return gameMode.GetGameStartText(world);
    }

    public GameModeInGameText GetInGameText(World world)
    {
        var gameMode = GetGameMode(world.Config.GameModeOptions.SelectedGameModeType);
        return gameMode.GetInGameText(world);
    }

    public Dictionary<GameModeType, string> GetGameModeDescriptions()
    {
        return s_gameModeDescriptions;
    }

    public void UpdateInitialTrackerState(GameModeOptions gameModeOptions, TrackerState trackerState, ParsedRomDetails? parsedRomDetails)
    {
        var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);
        gameMode.UpdateInitialTrackerState(gameModeOptions, trackerState, parsedRomDetails);

        int? markedGanonsTowerCrystalCount = null;

        if (parsedRomDetails == null)
        {
            if (gameModeOptions.RandomizeNumericAmounts)
            {
                markedGanonsTowerCrystalCount =
                    gameModeOptions.MinGanonsTowerCrystalCount == gameModeOptions.MaxGanonsTowerCrystalCount
                        ? gameModeOptions.MinGanonsTowerCrystalCount
                        : null;
            }
            else
            {
                markedGanonsTowerCrystalCount = gameModeOptions.GanonsTowerCrystalCount;
            }
        }

        trackerState.MarkedGanonsTowerCrystalCount = markedGanonsTowerCrystalCount;
    }

    public string GetSpoilerText(GameModeOptions gameModeOptions)
    {
        var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);
        var goalText = gameMode.GetSpoilerText(gameModeOptions);
        return $"Goal: {gameModeOptions.SelectedGameModeType} | {goalText}, LiftOffOnGoalCompletion =  {gameModeOptions.LiftOffOnGoalCompletion}";
    }

    public List<Location>? GetGameModeLocations(World world, List<World> allWorlds)
    {
        var gameMode = GetGameMode(world.Config.GameModeOptions.SelectedGameModeType);
        return gameMode.GetGameModeLocations(world, allWorlds);
    }

    public Stream GetLiftOffOnGoalCompletionIpsPatch(GameModeOptions gameModeOptions)
    {
        var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);
        return gameMode.GetLiftOffOnGoalCompletionIpsPatch();
    }
}
