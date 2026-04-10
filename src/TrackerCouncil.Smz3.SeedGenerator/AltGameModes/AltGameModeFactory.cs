using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.AltGameModes;

public class AltGameModeFactory(IServiceProvider serviceProvider)
{
    private static readonly Dictionary<GameModeType, Type> s_gameModeTypes = [];

    private static readonly Dictionary<GameModeType, string> s_gameModeDescriptions = new()
    {
        {
            GameModeType.Vanilla,
            "Complete Zelda dungeons to obtain crystals to enter Ganon's Tower and defeat Ganon. Defeat bosses in Super Metroid to be able to enter Tourian and defeat Mother Brain."
        }
    };

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

    public AltGameModeBase GetGameMode(GameModeType gameMode)
    {
        if (!s_gameModeTypes.TryGetValue(gameMode, out var gameModeClassType))
        {
            throw new InvalidOperationException($"GameModeType {gameMode} does not have a matching GameMode class");
        }

        return serviceProvider.GetService(gameModeClassType) as AltGameModeBase ??
               throw new InvalidOperationException($"GameModeType {gameMode} does not have a valid GameMode class");
    }

    public World UpdateWorld(World world, int seed)
    {
        var gameModeOptions = world.Config.GameModeOptions;

        if (gameModeOptions.SelectedGameModeType == GameModeType.Vanilla)
        {
            return world;
        }

        var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);
        gameMode.UpdateWorld(world, seed, gameModeOptions);
        return world;
    }

    public string? GetGameStartText(World world)
    {
        var gameModeOptions = world.Config.GameModeOptions;

        if (gameModeOptions.SelectedGameModeType == GameModeType.Vanilla)
        {
            return null;
        }

        var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);
        return gameMode.GetGameStartText(world);
    }

    public AltGameModeInGameText? GetInGameText(World world)
    {
        if (world.Config.GameModeOptions.SelectedGameModeType == GameModeType.Vanilla)
        {
            return null;
        }

        var gameMode = GetGameMode(world.Config.GameModeOptions.SelectedGameModeType);
        return gameMode.GetInGameText(world);
    }

    public Dictionary<GameModeType, string> GetGameModeDescriptions()
    {
        return s_gameModeDescriptions;
    }

    public void UpdateInitialTrackerState(GameModeOptions gameModeOptions, TrackerState trackerState)
    {
        if (gameModeOptions.SelectedGameModeType == GameModeType.Vanilla)
        {
            return;
        }

        var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);
        gameMode.UpdateInitialTrackerState(gameModeOptions, trackerState);
    }

    public string GetSpoilerText(GameModeOptions gameModeOptions)
    {

        string goalText;

        if (gameModeOptions.SelectedGameModeType == GameModeType.Vanilla)
        {
            goalText =
                $"GanonCrystalCount = {gameModeOptions.GanonCrystalCount}, TourianBossCount = {gameModeOptions.TourianBossCount}";
        }
        else
        {
            var gameMode = GetGameMode(gameModeOptions.SelectedGameModeType);
            goalText = gameMode.GetSpoilerText(gameModeOptions);
        }

        return
            $"Goal: {gameModeOptions.SelectedGameModeType} | {goalText}, LiftOffOnGoalCompletion =  {gameModeOptions.LiftOffOnGoalCompletion}";
    }

    public List<Location>? GetGameModeLocations(World world)
    {
        if (world.Config.GameModeOptions.SelectedGameModeType == GameModeType.Vanilla)
        {
            return null;
        }

        var gameMode = GetGameMode(world.Config.GameModeOptions.SelectedGameModeType);
        return gameMode.GetGameModeLocations(world);
    }
}
