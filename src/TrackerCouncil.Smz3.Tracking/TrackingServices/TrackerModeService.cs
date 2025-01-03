using System;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;
using BunLabs;
namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerModeService() : TrackerService, ITrackerModeService
{
    public bool GoMode { get; set; }
    public bool PegWorldMode { get; set; }
    public bool ShaktoolMode { get; set; }
    public int PegsPegged { get; set; }
    public bool CheatsEnabled { get; set; }

    public event EventHandler<TrackerEventArgs>? ToggledPegWorldModeOn;
    public event EventHandler<TrackerEventArgs>? ToggledShaktoolMode;
    public event EventHandler<TrackerEventArgs>? PegPegged;
    public event EventHandler<TrackerEventArgs>? GoModeToggledOn;
    public event EventHandler<TrackerEventArgs>? GoModeToggledOff;
    public event EventHandler<TrackerEventArgs>? CheatsToggled;

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

    public void EnableCheats(float? confidence = null)
    {
        if (!CheatsEnabled)
        {
            CheatsEnabled = true;
            Tracker.Say(x => x.Cheats.EnabledCheats);
            CheatsToggled?.Invoke(this, new TrackerEventArgs(confidence));
            AddUndo(() =>
            {
                CheatsEnabled = false;
                CheatsToggled?.Invoke(this, new TrackerEventArgs(confidence));
            });
        }
        else
        {
            Tracker.Say(x => x.Cheats.AlreadyEnabledCheats);
        }
    }

    public void DisableCheats(float? confidence = null)
    {
        if (CheatsEnabled)
        {
            CheatsEnabled = false;
            Tracker.Say(x => x.Cheats.DisabledCheats);
            CheatsToggled?.Invoke(this, new TrackerEventArgs(confidence));
            AddUndo(() =>
            {
                CheatsEnabled = true;
                CheatsToggled?.Invoke(this, new TrackerEventArgs(confidence));
            });
        }
        else
        {
            Tracker.Say(x => x.Cheats.AlreadyDisabledCheats);
        }
    }
}
