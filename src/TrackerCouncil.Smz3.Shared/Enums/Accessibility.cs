namespace TrackerCouncil.Smz3.Shared.Enums;

/// <summary>
/// Indicates the current tracking status of the location
/// </summary>
public enum Accessibility
{
    /// <summary>
    /// The location's status is currently unknown
    /// </summary>
    Unknown,

    /// <summary>
    /// Already cleared by the player
    /// </summary>
    Cleared,

    /// <summary>
    /// Currently in logic
    /// </summary>
    Available,

    /// <summary>
    /// Currently in logic if keys are assumed
    /// </summary>
    AvailableWithKeys,

    /// <summary>
    /// Currently in logic, but the player must do something else before
    /// it's available
    /// </summary>
    Relevant,

    /// <summary>
    /// Currently in logic, but the player must do something else before
    /// it's available
    /// </summary>
    RelevantWithKeys,

    /// <summary>
    /// This location is not currently available to the player
    /// </summary>
    OutOfLogic
}
