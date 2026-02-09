using System;
using System.Collections.Generic;
using System.Reflection;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.GameModes;

public class GameModeFactory(IServiceProvider serviceProvider)
{
    private static readonly Dictionary<GameModeType, Type> s_gameModeTypes = [];

    public static void AddGameModeClass(Type gameModeType)
    {
        var gameModeTypeAttribute = gameModeType.GetCustomAttribute<GameModeTypeAttribute>();
        if (gameModeTypeAttribute == null)
        {
            throw new InvalidOperationException($"GameModeType {gameModeType.Name} is missing GameModeTypeAttribute");
        }
        s_gameModeTypes.Add(gameModeTypeAttribute.GameModeType, gameModeType);
    }

    public GameModeBase GetGameMode(GameModeType gameMode)
    {
        if (!s_gameModeTypes.TryGetValue(gameMode, out var gameModeClassType))
        {
            throw new InvalidOperationException($"GameModeType {gameMode} does not have a matching GameMode class");
        }

        var test = serviceProvider.GetService(gameModeClassType);

        return serviceProvider.GetService(gameModeClassType) as GameModeBase ??
               throw new InvalidOperationException($"GameModeType {gameMode} does not have a valid GameMode class");
    }
}
