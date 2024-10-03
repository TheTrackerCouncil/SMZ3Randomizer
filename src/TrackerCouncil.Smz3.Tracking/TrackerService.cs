using System;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;

namespace TrackerCouncil.Smz3.Tracking;

public abstract class TrackerService
{
    protected static readonly Random Random = new();

    public virtual void Initialize()
    {

    }

    internal TrackerBase Tracker { get; set; } = null!;
    protected Configs Configs => Tracker.Configs;
    protected ResponseConfig Responses => Tracker.Responses;
    protected World World => Tracker.World;
    protected TrackerOptions Options => Tracker.Options;
    protected IHistoryService History => Tracker.History;
    protected bool HintsEnabled => Tracker.HintsEnabled;
    protected bool SpoilersEnabled => Tracker.SpoilersEnabled;
    protected Config? LocalConfig => Tracker.LocalConfig;
    public void RestartIdleTimers() => Tracker.RestartIdleTimers();

    protected bool IsDirty
    {
        get => Tracker.IsDirty;
        set => Tracker.MarkAsDirty(value);
    }
    internal void AddUndo(Action undo) => Tracker.AddUndo(undo);
    internal (Action Action, DateTime UndoTime) PopUndo() => Tracker.PopUndo();
}
