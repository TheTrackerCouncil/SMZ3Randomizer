using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.Options;

public class GameModeConfigs
{
    [GameModeType(GameModeType.Default)]
    public DefaultGameConfig DefaultGameConfig { get; set; } = new()
    {
        Enabled = true;
    }

    [GameModeType(GameModeType.Keysanity)]
    public KeysanityConfig KeysanityConfig { get; set; } = new();

    public ICollection<GameModeType> GetEnabledGameModes()
    {
        var toReturn = new List<GameModeType>();

        var properties = GetType().GetProperties().Where(x =>
            x.PropertyType.IsSubclassOf(typeof(GameModeConfig)) &&
            x.GetCustomAttribute<GameModeTypeAttribute>() != null);

        foreach (var property in properties)
        {
            var config = property.GetValue(this) as GameModeConfig;
            if (config?.Enabled == true)
            {
                toReturn.Add(property.GetCustomAttribute<GameModeTypeAttribute>()!.GameModeType);
            }
        }

        return toReturn;
    }
}
