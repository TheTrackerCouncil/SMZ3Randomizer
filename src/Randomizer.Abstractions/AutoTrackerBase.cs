using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.Shared.Enums;

namespace Randomizer.Abstractions;

public abstract class AutoTrackerBase
{
    /// <summary>
    /// The tracker associated with this auto tracker
    /// </summary>
    protected TrackerBase TrackerBase { get; set; } = null!;

    /// <summary>
    /// The type of connector that the auto tracker is currently using
    /// </summary>
    public EmulatorConnectorType ConnectorType { get; protected set; }

    /// <summary>
    /// The game that the player is currently in
    /// </summary>
    public Game CurrentGame { get; protected set; }

    /// <summary>
    /// The latest state that the player in LTTP (location, health, etc.)
    /// </summary>
    public AutoTrackerZeldaState? ZeldaState { get; protected set; }

    /// <summary>
    /// The latest state that the player in Super Metroid (location, health, etc.)
    /// </summary>
    public AutoTrackerMetroidState? MetroidState { get; protected set;  }

    /// <summary>
    /// Disables the current connector and creates the requested type
    /// </summary>
    public abstract void SetConnector(EmulatorConnectorType type, string? qusb2SnesIp);

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
    /// If the auto tracker is currently sending messages
    /// </summary>
    protected bool IsSendingMessages { get; set; }

    /// <summary>
    /// If the player currently has a fairy
    /// </summary>
    public bool PlayerHasFairy { get; protected set; }

    /// <summary>
    /// If the user is activately in an SMZ3 rom
    /// </summary>
    public abstract bool IsInSMZ3 { get; }

    /// <summary>
    /// Writes a particular action to the emulator memory
    /// </summary>
    /// <param name="action">The action to write to memory</param>
    public abstract void WriteToMemory(EmulatorAction action);

    protected void OnAutoTrackerEnabled()
    {
        AutoTrackerEnabled?.Invoke(this, EventArgs.Empty);
    }

    protected void OnAutoTrackerDisabled()
    {
        AutoTrackerDisabled?.Invoke(this, EventArgs.Empty);
    }

    protected void OnAutoTrackerConnected()
    {
        AutoTrackerConnected?.Invoke(this, EventArgs.Empty);
    }

    protected void OnAutoTrackerDisconnected()
    {
        AutoTrackerDisconnected?.Invoke(this, EventArgs.Empty);
    }

}
