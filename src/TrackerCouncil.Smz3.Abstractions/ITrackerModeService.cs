using TrackerCouncil.Smz3.Data.Tracking;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerModeService
{
    /// <summary>
    /// Indicates whether Tracker is in Go Mode.
    /// </summary>
    public bool GoMode { get; protected set; }

    /// <summary>
    /// Indicates whether Tracker is in Peg World mode.
    /// </summary>
    public bool PegWorldMode { get; protected set; }

    /// <summary>
    /// Indicates whether Tracker is in Shaktool mode.
    /// </summary>
    public bool ShaktoolMode { get; protected set; }

    /// <summary>
    /// The number of pegs that have been pegged for Peg World mode
    /// </summary>
    public int PegsPegged { get; protected set; }

    /// <summary>
    /// If cheats are currently enabled
    /// </summary>
    public bool CheatsEnabled { get; protected set; }

    // <summary>
    /// Occurs when Peg World mode has been toggled on.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? ToggledPegWorldModeOn;

    /// <summary>
    /// Occurs when going to Shaktool
    /// </summary>
    public event EventHandler<TrackerEventArgs>? ToggledShaktoolMode;

    /// <summary>
    /// Occurs when a Peg World peg has been pegged.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? PegPegged;

    /// <summary>
    /// Occurs when Go mode has been turned on.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? GoModeToggledOn;

    /// <summary>
    /// Occurs when Go mode has been turned off.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? GoModeToggledOff;

    /// <summary>
    /// Occurs when cheats are enabled or disabled
    /// </summary>
    public event EventHandler<TrackerEventArgs>? CheatsToggled;

    /// <summary>
    /// Toggles Go Mode on.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void ToggleGoMode(float? confidence = null);

    /// <summary>
    /// Pegs a Peg World peg.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void Peg(float? confidence = null);

    public void SetPegs(int count);

    /// <summary>
    /// Starts Peg World mode.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void StartPegWorldMode(float? confidence = null);

    /// <summary>
    /// Turns Peg World mode off.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void StopPegWorldMode(float? confidence = null);

    /// <summary>
    /// Starts Peg World mode.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void StartShaktoolMode(float? confidence = null);

    /// <summary>
    /// Turns Peg World mode off.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void StopShaktoolMode(float? confidence = null);

    /// <summary>
    /// Enables cheats
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void EnableCheats(float? confidence = null);

    /// <summary>
    /// Disables cheats
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void DisableCheats(float? confidence = null);
}
