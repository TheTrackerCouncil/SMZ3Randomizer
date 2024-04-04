using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Enums;
using SnesConnectorLibrary;

namespace Randomizer.Abstractions;

public abstract class AutoTrackerBase : IDisposable
{
    /// <summary>
    /// The tracker associated with this auto tracker
    /// </summary>
    protected TrackerBase TrackerBase { get; set; } = null!;

    /// <summary>
    /// The type of connector that the auto tracker is currently using
    /// </summary>
    public SnesConnectorType ConnectorType { get; protected set; }

    /// <summary>
    /// The game that the player is currently in
    /// </summary>
    public Game CurrentGame { get; protected set; }

    /// <summary>
    /// The game that the player was previous in
    /// </summary>
    public Game PreviousGame { get; protected set; }

    /// <summary>
    /// The latest state that the player in LTTP (location, health, etc.)
    /// </summary>
    public AutoTrackerZeldaState? ZeldaState { get; set; }

    /// <summary>
    /// The latest state that the player in Super Metroid (location, health, etc.)
    /// </summary>
    public AutoTrackerMetroidState? MetroidState { get; set;  }

    public abstract void SetConnector(SnesConnectorSettings snesConnectorSettings, SnesConnectorType? connectorTypeOverride = null);

    /// <summary>
    /// Occurs when the tracker's auto tracker is enabled
    /// </summary>
    public event EventHandler? AutoTrackerEnabled;

    /// <summary>
    /// Occurs when the tracker's auto tracker is disabled
    /// </summary>
    public event EventHandler? AutoTrackerDisabled;

    /// <summary>
    /// Occurs when the tracker's auto tracker is connected
    /// </summary>
    public event EventHandler? AutoTrackerConnected;

    /// <summary>
    /// Occurs when the tracker's auto tracker is disconnected
    /// </summary>
    public event EventHandler? AutoTrackerDisconnected;

    /// <summary>
    /// The action to run when the player asks Tracker to look at the game
    /// </summary>
    public AutoTrackerViewedAction? LatestViewAction { get; set; }

    /// <summary>
    /// The unique key for the auto tracker viewed action to avoid duplicates
    /// </summary>
    public string? LatestViewActionKey { get; set; }

    /// <summary>
    /// If a connector is currently enabled
    /// </summary>
    public abstract bool IsEnabled { get; }

    /// <summary>
    /// If a connector is currently connected to the emulator
    /// </summary>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// If a connector is currently connected to the emulator and a valid game state is detected
    /// </summary>
    public abstract bool HasValidState { get; }

    /// <summary>
    /// If the player currently has a fairy
    /// </summary>
    public bool PlayerHasFairy { get; set; }

    /// <summary>
    /// If the user is activately in an SMZ3 rom
    /// </summary>
    public abstract bool IsInSMZ3 { get; }

    /// <summary>
    /// Sets the latest view action to use by voice command, or if specified in the options, automatically execute it
    /// </summary>
    /// <param name="key">Unique key for the action type</param>
    /// <param name="action">The viewed action</param>
    public abstract void SetLatestViewAction(string key, Action action);

    public abstract void UpdateGame(Game game);

    public abstract void UpdateValidState(bool hasValidState);

    public abstract void IncrementGTItems(Location location);

    public bool HasStarted { get; set; }

    public bool HasDefeatedBothBosses { get; set; }

    /// <summary>
    /// Invokes the AutoTrackerEnabled event
    /// </summary>
    protected virtual void OnAutoTrackerEnabled()
    {
        AutoTrackerEnabled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invokes the OnAutoTrackerDisabled event
    /// </summary>
    protected virtual void OnAutoTrackerDisabled()
    {
        AutoTrackerDisabled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invokes the OnAutoTrackerConnected event
    /// </summary>
    protected virtual void OnAutoTrackerConnected()
    {
        AutoTrackerConnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invokes the OnAutoTrackerDisconnected event
    /// </summary>
    protected virtual void OnAutoTrackerDisconnected()
    {
        AutoTrackerDisconnected?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Dispose();
}
