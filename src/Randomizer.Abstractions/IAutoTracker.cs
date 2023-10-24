using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.Shared.Enums;

namespace Randomizer.Abstractions;

public interface IAutoTracker
{
    /// <summary>
    /// The tracker associated with this auto tracker
    /// </summary>
    public ITracker Tracker { get; }

    /// <summary>
    /// The type of connector that the auto tracker is currently using
    /// </summary>
    public EmulatorConnectorType ConnectorType { get; }

    /// <summary>
    /// The game that the player is currently in
    /// </summary>
    public Game CurrentGame { get; }

    /// <summary>
    /// The latest state that the player in LTTP (location, health, etc.)
    /// </summary>
    public AutoTrackerZeldaState? ZeldaState { get; }

    /// <summary>
    /// The latest state that the player in Super Metroid (location, health, etc.)
    /// </summary>
    public AutoTrackerMetroidState? MetroidState { get; }

    /// <summary>
    /// Disables the current connector and creates the requested type
    /// </summary>
    public void SetConnector(EmulatorConnectorType type, string? qusb2SnesIp);

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
    public bool IsEnabled { get; }

    /// <summary>
    /// If a connector is currently connected to the emulator
    /// </summary>
    public bool IsConnected { get; }

    /// <summary>
    /// If a connector is currently connected to the emulator and a valid game state is detected
    /// </summary>
    public bool HasValidState { get; }

    /// <summary>
    /// If the auto tracker is currently sending messages
    /// </summary>
    public bool IsSendingMessages { get; }

    /// <summary>
    /// If the player currently has a fairy
    /// </summary>
    public bool PlayerHasFairy { get; }

    /// <summary>
    /// If the user is activately in an SMZ3 rom
    /// </summary>
    public bool IsInSMZ3 { get; }

    /// <summary>
    /// Writes a particular action to the emulator memory
    /// </summary>
    /// <param name="action">The action to write to memory</param>
    public void WriteToMemory(EmulatorAction action);

}
