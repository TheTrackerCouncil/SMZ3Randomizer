namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Provides the phrases for cheats
/// </summary>
public class CheatsConfig : IMergeable<CheatsConfig>
{
    /// <summary>
    /// Gets the phrases to respond with when cheats are turned on.
    /// </summary>
    public SchrodingersString? EnabledCheats { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when cheats are turned off.
    /// </summary>
    public SchrodingersString? DisabledCheats { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a cheat command is given and cheats
    /// are turned off
    /// </summary>
    public SchrodingersString? PromptEnableCheats { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a cheat command is given and auto
    /// tracker is not connected
    /// </summary>
    public SchrodingersString? PromptEnableAutoTracker { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when connected to to emulator.
    /// </summary>
    public SchrodingersString? CheatPerformed { get; init; }

    /// <summary>
    /// Gets the phrases to respond when the player can't perform a certain cheat.
    /// </summary>
    public SchrodingersString? CheatFailed { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the player tries to cheat
    /// themselves an item that does not exist.
    /// </summary>
    public SchrodingersString? CheatInvalidItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with after successfully killing the player.
    /// </summary>
    public SchrodingersString? KilledPlayer { get; init; }
}
