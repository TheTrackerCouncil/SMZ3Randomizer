using System;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal abstract class TrackerService
{
    protected static readonly Random Random = new();

    internal TrackerBase Tracker { get; set; } = null!;
    protected ResponseConfig Responses => Tracker.Responses;
    protected World World => Tracker.World;
    protected TrackerOptions Options => Tracker.Options;
    protected IHistoryService History => Tracker.History;
    protected bool HintsEnabled => Tracker.HintsEnabled;
    protected bool SpoilersEnabled => Tracker.SpoilersEnabled;
    protected Config? LocalConfig => Tracker.LocalConfig;

    protected bool IsDirty
    {
        get => Tracker.IsDirty;
        set => Tracker.MarkAsDirty(value);
    }

    public virtual void Initialize() {}
    public virtual void PostInitialize() {}
    public void RestartIdleTimers() => Tracker.RestartIdleTimers();
    internal void AddUndo(Action undo) => Tracker.AddUndo(undo);
    internal void AddUndo(bool autoTracked, Action undo)
    {
        if (autoTracked) return;
        Tracker.AddUndo(undo);
    }

    internal (Action Action, DateTime UndoTime) PopUndo() => Tracker.PopUndo();

    protected void UpdateAllAccessibility(bool itemRemoved, params Item[] items) => Tracker.UpdateAllAccessibility(itemRemoved, items);
}
