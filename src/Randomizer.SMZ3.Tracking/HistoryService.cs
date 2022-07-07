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
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Module for changing the map
    /// </summary>
    public class HistoryService : IHistoryService
    {
        ILogger<HistoryService> _logger;
        Tracker _tracker;
        List<TrackerHistoryEvent> _events;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public HistoryService(ILogger<HistoryService> logger)
        {
            _logger = logger;
        }

        public void StartHistory(Tracker tracker)
        {
            _tracker = tracker;
            _events = new List<TrackerHistoryEvent>();
        }

        public void LoadHistory(Tracker tracker, TrackerState state)
        {
            _tracker = tracker;
            _events = state.HistoryEvents.OrderBy(x => x.Time).ToList();
        }

        public void AddEvent(HistoryEventType type, bool isImportant, string objectName, Location? location = null)
        {
            var regionName = location?.Region.Name.ToString();
            var locationName = location?.Room != null ? $"{location.Room.Name} - {location.Name}" : location?.Name;
            AddEvent(new TrackerHistoryEvent()
            {
                Type = type,
                IsImportant = isImportant,
                ObjectName = objectName,
                LocationName = location != null ? $"{regionName} - {locationName}" : null,
                LocationId = location?.Id,
                Time = _tracker.TotalElapsedTime.TotalSeconds
            });
        }

        public void AddEvent(HistoryEventType type, bool isImportant, string objectName, LocationInfo? location)
        {
            AddEvent(new TrackerHistoryEvent()
            {
                Type = type,
                IsImportant = isImportant,
                ObjectName = objectName,
                LocationName = location.Name.ToString(),
                LocationId = location?.Id,
                Time = _tracker.TotalElapsedTime.TotalSeconds
            });
        }

        public void AddEvent(TrackerHistoryEvent histEvent)
        {
            _events.Add(histEvent);
        }

        public void RemoveLastEvent()
        {
            if (_events.Count > 0)
            {
                Remove(_events[_events.Count-1]);
            }
        }

        public void Remove(TrackerHistoryEvent histEvent)
        {
            _events.Remove(histEvent);
        }

        public IReadOnlyCollection<TrackerHistoryEvent> GetHistory() => _events;

        public string GenerateHistoryText(GeneratedRom rom, IReadOnlyCollection<TrackerHistoryEvent> history, bool importantOnly = true)
        {
            var log = new StringBuilder();
            log.AppendLine(Underline($"SMZ3 Cas’ run progression", '='));
            log.AppendLine($"Generated on {DateTime.Now:F}");
            log.AppendLine($"Seed: {rom.Seed}");
            log.AppendLine($"Settings String: {rom.Settings}");
            log.AppendLine();

            var enumType = typeof(HistoryEventType);
            

            foreach (var historyEvent in history.Where(x => !importantOnly || x.IsImportant).OrderBy(x => x.Time))
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
