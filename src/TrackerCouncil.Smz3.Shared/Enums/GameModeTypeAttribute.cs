using System;

namespace TrackerCouncil.Smz3.Shared.Enums;

/// <summary>
/// Specifies the game mode type for a given game mode base
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GameModeTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="TrackerCouncil.Smz3.Shared.Enums.GameModeTypeAttribute"/> class with the specified type.
    /// </summary>
    /// <param name="type">
    /// The game mode type to assign
    /// </param>
    public GameModeTypeAttribute(GameModeType type)
    {
        GameModeType = type;
    }

    /// <summary>
    /// Gets the game mode type of the class
    /// </summary>
    public GameModeType GameModeType { get; }
}
