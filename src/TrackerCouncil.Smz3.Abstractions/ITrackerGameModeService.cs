using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

/// <summary>
/// Tracker service for various events that need to interact with the game mode
/// </summary>
public interface ITrackerGameModeService
{
    /// <summary>
    /// The current game mode type
    /// </summary>
    public GameModeType GameModeType { get; }

    /// <summary>
    /// If this is a game mode not handled by the base assembly code
    /// </summary>
    public bool HasAltGameMode { get; }

    /// <summary>
    /// If the game mode has been completed
    /// </summary>
    public bool IsGameModeComplete { get; }

    /// <summary>
    /// Event called when the goal progress has changed
    /// </summary>
    public event EventHandler? GoalStateChanged;

    /// <summary>
    /// Marks the game mode as complete
    /// </summary>
    public void MarkGameModeAsComplete();

    /// <summary>
    /// If the game mode is complete and the player should be aware of it
    /// as they know the required amounts to be considered complete
    /// </summary>
    /// <returns></returns>
    public bool IsGameModeKnowinglyComplete();

    /// <summary>
    /// Informs the game mode that the player has viewed the pyramid so
    /// that any unknown values are populated
    /// </summary>
    public void OnViewingPyramidText();

    /// <summary>
    /// Notifies all listeners that the progress of the game mode has changed
    /// </summary>
    public void NotifyOfGoalStateChange();

    /// <summary>
    /// Retrieve the UI to show the progress of the goals
    /// </summary>
    /// <returns>A list of goal details to show in the tracker UI</returns>
    public List<GoalUiDetails> GetGoalUiDetails();
}
