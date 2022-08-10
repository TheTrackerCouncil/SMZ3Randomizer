using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Speech.Recognition;
using System.Text;
using Microsoft.Extensions.Logging;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Service for managing the history of events through a playthrough
    /// </summary>
    public class HistoryService : IHistoryService
    {
        private ILogger<HistoryService> _logger;
        private Tracker? _tracker;
        private List<TrackerHistoryEvent>? _events;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public HistoryService(ILogger<HistoryService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Starts a new history list for a tracker instance
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        public void StartHistory(Tracker tracker)
        {
            _tracker = tracker;
            _events = new List<TrackerHistoryEvent>();
        }

        /// <summary>
        /// Loads the history from the tracker state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="state">The tracker state with the history</param>
        public void LoadHistory(Tracker tracker, TrackerState state)
        {
            _tracker = tracker;
            _events = state.HistoryEvents.OrderBy(x => x.Time).ToList();
        }

        /// <summary>
        /// Adds an event to the history log
        /// </summary>
        /// <param name="type">The type of event</param>
        /// <param name="isImportant">If this is an important event or not</param>
        /// <param name="objectName">The name of the event being logged</param>
        /// <param name="location">The optional location of where this event happened</param>
        /// <returns>The created event</returns>
        public TrackerHistoryEvent AddEvent(HistoryEventType type, bool isImportant, string objectName, Location? location = null)
        {
            var regionName = location?.Region.Name.ToString();
            var locationName = location?.Room != null ? $"{location.Room.Name} - {location.Name}" : location?.Name;
            var addedEvent = new TrackerHistoryEvent()
            {
                Type = type,
                IsImportant = isImportant,
                ObjectName = objectName,
                LocationName = location != null ? $"{regionName} - {locationName}" : null,
                LocationId = location?.Id,
                Time = _tracker?.TotalElapsedTime.TotalSeconds ?? 0
            };
            AddEvent(addedEvent);
            return addedEvent;
        }

        /// <summary>
        /// Adds an event to the history log
        /// </summary>
        /// <param name="type">The type of event</param>
        /// <param name="isImportant">If this is an important event or not</param>
        /// <param name="objectName">The name of the event being logged</param>
        /// <param name="location">The optional location of where this event happened</param>
        /// <returns>The created event</returns>
        public TrackerHistoryEvent AddEvent(HistoryEventType type, bool isImportant, string objectName, LocationInfo? location)
        {
            var addedEvent = new TrackerHistoryEvent()
            {
                Type = type,
                IsImportant = isImportant,
                ObjectName = objectName,
                LocationName = location?.Name.ToString(),
                LocationId = location?.LocationNumber,
                Time = _tracker?.TotalElapsedTime.TotalSeconds ?? 0
            };
            AddEvent(addedEvent);
            return addedEvent;
        }

        /// <summary>
        /// Adds an event to the history log
        /// </summary>
        /// <param name="histEvent">The event to add</param>
        public void AddEvent(TrackerHistoryEvent histEvent)
        {
            _events?.Add(histEvent);
        }

        /// <summary>
        /// Removes the event that was added last to the log
        /// </summary>
        public void RemoveLastEvent()
        {
            if (_events?.Count > 0)
            {
                Remove(_events[_events.Count-1]);
            }
        }

        /// <summary>
        /// Removes a specific event from the log
        /// </summary>
        /// <param name="histEvent">The event to log</param>
        public void Remove(TrackerHistoryEvent histEvent)
        {
            _events?.Remove(histEvent);
        }

        /// <summary>
        /// Retrieves the current history log
        /// </summary>
        /// <returns>The collection of events</returns>
        public IReadOnlyCollection<TrackerHistoryEvent> GetHistory() => _events;

        /// <summary>
        /// Creates the progression log based off of the history
        /// </summary>
        /// <param name="rom">The rom that the history is for</param>
        /// <param name="history">All of the events to log</param>
        /// <param name="importantOnly">If only important events should be logged or not</param>
        /// <returns>The generated log text</returns>
        public string GenerateHistoryText(GeneratedRom rom, IReadOnlyCollection<TrackerHistoryEvent> history, bool importantOnly = true)
        {
            var log = new StringBuilder();
            log.AppendLine(Underline($"SMZ3 Cas’ run progression log", '='));
            log.AppendLine($"Generated on {DateTime.Now:F}");
            log.AppendLine($"Seed: {rom.Seed}");
            log.AppendLine($"Settings String: {rom.Settings}");
            log.AppendLine();

            var enumType = typeof(HistoryEventType);
            
            foreach (var historyEvent in history.Where(x => !x.IsUndone && (!importantOnly || x.IsImportant)).OrderBy(x => x.Time))
            {
                var time = TimeSpan.FromSeconds(historyEvent.Time).ToString(@"hh\:mm\:ss");

                var field = enumType.GetField(Enum.GetName(historyEvent.Type) ?? "");
                var verb = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;

                if (historyEvent.LocationId.HasValue)
                {
                    log.AppendLine($"[{time}] {verb} {historyEvent.ObjectName} from {historyEvent.LocationName}");
                }
                else
                {
                    log.AppendLine($"[{time}] {verb} {historyEvent.ObjectName}");
                }
            }

            var finalTime = TimeSpan.FromSeconds(rom.TrackerState.SecondsElapsed).ToString(@"hh\:mm\:ss");
            
            log.AppendLine();
            log.AppendLine($"Final time: {finalTime}");

            return log.ToString();
        }

        /// <summary>
        /// Underlines text in the spoiler log
        /// </summary>
        /// <param name="text">The text to be underlined</param>
        /// <param name="line">The character to use for underlining</param>
        /// <returns>The text to be underlined followed by the underlining text</returns>
        private static string Underline(string text, char line = '-')
            => text + "\n" + new string(line, text.Length);
    }
}
