﻿using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Abstractions;

/// <summary>
/// Service for managing the history of events through a playthrough
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// Adds an event to the history log
    /// </summary>
    /// <param name="type">The type of event</param>
    /// <param name="isImportant">If this is an important event or not</param>
    /// <param name="objectName">The name of the event being logged</param>
    /// <param name="location">The optional location of where this event happened</param>
    /// <returns>The created event</returns>
    public TrackerHistoryEvent AddEvent(HistoryEventType type, bool isImportant, string objectName, Location? location = null);

    /// <summary>
    /// Adds an event to the history log
    /// </summary>
    /// <param name="histEvent">The event to add</param>
    public void AddEvent(TrackerHistoryEvent histEvent);

    /// <summary>
    /// Removes the event that was added last to the log
    /// </summary>
    public void RemoveLastEvent();

    /// <summary>
    /// Removes a specific event from the log
    /// </summary>
    /// <param name="histEvent">The event to log</param>
    public void Remove(TrackerHistoryEvent histEvent);

    /// <summary>
    /// Retrieves the current history log
    /// </summary>
    /// <returns>The collection of events</returns>
    public IReadOnlyCollection<TrackerHistoryEvent> GetHistory();
}
