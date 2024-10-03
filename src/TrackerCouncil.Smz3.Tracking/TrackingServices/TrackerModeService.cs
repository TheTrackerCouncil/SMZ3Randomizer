using System;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;
using BunLabs;
namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

public class TrackerModeService : TrackerService, ITrackerModeService
{

    /// <summary>
    /// Toggles Go Mode on.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void ToggleGoMode(float? confidence = null)
    {
        Tracker.ShutUp();

        if (Random.NextDouble(0, 1) < 0.95)
        {
            Tracker.Say(text: "Toggled Go Mode <break time='1s'/>", wait: true);
        }
        else
        {
            Tracker.Say(text: "Toggled Go Mode <break time='8s'/>", wait: true);
        }

        GoMode = true;
        GoModeToggledOn?.Invoke(this, new TrackerEventArgs(confidence));
        Tracker.Say(text: "on.");

        AddUndo(() =>
        {
            GoMode = false;
            if (Responses.GoModeToggledOff != null)
                Tracker.Say(response: Responses.GoModeToggledOff);
            GoModeToggledOff?.Invoke(this, new TrackerEventArgs(confidence));
        });
    }


    /// <summary>
    /// Pegs a Peg World peg.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void Peg(float? confidence = null)
    {
        if (!PegWorldMode)
            return;

        PegsPegged++;

        if (PegsPegged < PegWorldModeModule.TotalPegs)
            Tracker.Say(response: Responses.PegWorldModePegged);
        else
        {
            PegWorldMode = false;
            Tracker.Say(response: Responses.PegWorldModeDone);
            ToggledPegWorldModeOn?.Invoke(this, new TrackerEventArgs(confidence));
        }

        PegPegged?.Invoke(this, new TrackerEventArgs(confidence));
        AddUndo(() =>
        {
            PegsPegged--;
            PegPegged?.Invoke(this, new TrackerEventArgs(confidence));
        });

        RestartIdleTimers();
    }

    public void SetPegs(int count)
    {
        if (count <= PegsPegged)
            return;

        if (!PegWorldMode)
        {
            PegWorldMode = true;
            ToggledPegWorldModeOn?.Invoke(this, new TrackerEventArgs(null, true));
        }

        if (count <= PegWorldModeModule.TotalPegs)
        {
            var delta = count - PegsPegged;

            PegsPegged = count;
            Tracker.Say(responses: Responses.PegWorldModePeggedMultiple, tieredKey: delta, wait: true);
            PegPegged?.Invoke(this, new TrackerEventArgs(null, true));
        }
    }

    /// <summary>
    /// Starts Peg World mode.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void StartPegWorldMode(float? confidence = null)
    {
        Tracker.ShutUp();
        PegWorldMode = true;
        Tracker.Say(response: Responses.PegWorldModeOn, wait: true);
        ToggledPegWorldModeOn?.Invoke(this, new TrackerEventArgs(confidence));
        AddUndo(() =>
        {
            PegWorldMode = false;
            ToggledPegWorldModeOn?.Invoke(this, new TrackerEventArgs(confidence));
        });
    }

    /// <summary>
    /// Turns Peg World mode off.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void StopPegWorldMode(float? confidence = null)
    {
        PegWorldMode = false;
        Tracker.Say(response: Responses.PegWorldModeDone);
        ToggledPegWorldModeOn?.Invoke(this, new TrackerEventArgs(confidence));
        AddUndo(() =>
        {
            PegWorldMode = true;
            ToggledPegWorldModeOn?.Invoke(this, new TrackerEventArgs(confidence));
        });
    }

    /// <summary>
    /// Starts Peg World mode.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void StartShaktoolMode(float? confidence = null)
    {
        ShaktoolMode = true;
        ToggledShaktoolMode?.Invoke(this, new TrackerEventArgs(confidence));
        AddUndo(() =>
        {
            ShaktoolMode = false;
            ToggledShaktoolMode?.Invoke(this, new TrackerEventArgs(confidence));
        });
    }

    /// <summary>
    /// Turns Peg World mode off.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void StopShaktoolMode(float? confidence = null)
    {
        ShaktoolMode = false;
        ToggledShaktoolMode?.Invoke(this, new TrackerEventArgs(confidence));
        AddUndo(() =>
        {
            ShaktoolMode = true;
            ToggledShaktoolMode?.Invoke(this, new TrackerEventArgs(confidence));
        });
    }


    public bool GoMode { get; set; }
    public bool PegWorldMode { get; set; }
    public bool ShaktoolMode { get; set; }
    public int PegsPegged { get; set; }
    public event EventHandler<TrackerEventArgs>? ToggledPegWorldModeOn;
    public event EventHandler<TrackerEventArgs>? ToggledShaktoolMode;
    public event EventHandler<TrackerEventArgs>? PegPegged;
    public event EventHandler<TrackerEventArgs>? GoModeToggledOn;
    public event EventHandler<TrackerEventArgs>? GoModeToggledOff;
}
