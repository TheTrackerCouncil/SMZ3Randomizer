using System;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared;

[AttributeUsage(AttributeTargets.Property,
    Inherited = false, AllowMultiple = false)]
public class GameModeTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="GameModeTypeAttribute"/> class with the specified game mode type.
    /// </summary>
    /// <param name="gameModeType">
    /// The game mode type applicable to the property
    /// </param>
    public GameModeTypeAttribute(GameModeType gameModeType)
    {
        GameModeType = gameModeType;
    }

    /// <summary>
    /// The game mode of property
    /// </summary>
    public GameModeType GameModeType { get; }
}
