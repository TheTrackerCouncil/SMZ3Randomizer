using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Service for managing the history of events through a playthrough
    /// </summary>
    public class HistoryService : IHistoryService
    {
        private readonly ILogger<HistoryService> _logger;
        private readonly IWorldAccessor _world;
        private readonly ITrackerTimerService _timerService;
        private ICollection<TrackerHistoryEvent> _historyEvents => _world.World.State?.History ?? new List<TrackerHistoryEvent>();
        private bool _isMultiworld;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="world"></param>
        /// <param name="logger"></param>
        /// <param name="timerService"></param>
        public HistoryService(IWorldAccessor world, ILogger<HistoryService> logger, ITrackerTimerService timerService)
        {
            _world = world;
            _logger = logger;
            _timerService = timerService;
            _isMultiworld = world.World.Config.MultiWorld;
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
            if (_world.World.State == null)
            {
                throw new InvalidOperationException("World tracker state not loaded");
            }

            var regionName = location?.Region.Name;
            var locationName = location?.Room != null ? $"{location.Room.Name} - {location.Name}" : location?.Name;
            var addedEvent = new TrackerHistoryEvent()
            {
                TrackerState = _world.World.State,
                Type = type,
                IsImportant = isImportant,
                ObjectName = objectName,
                LocationName = location != null ? $"{regionName} - {locationName}" : null,
                LocationId = location?.Id,
                Time = _timerService.SecondsElapsed
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
            if (_isMultiworld) return;
            _historyEvents.Add(histEvent);
        }

        /// <summary>
        /// Removes the event that was added last to the log
        /// </summary>
        public void RemoveLastEvent()
        {
            if (_isMultiworld) return;
            if (_historyEvents.Count > 0)
            {
                Remove(_historyEvents.OrderByDescending(x => x.Id).First());
            }
        }

        /// <summary>
        /// Removes a specific event from the log
        /// </summary>
        /// <param name="histEvent">The event to log</param>
        public void Remove(TrackerHistoryEvent histEvent)
        {
            if (_isMultiworld) return;
            _historyEvents.Remove(histEvent);
        }

        /// <summary>
        /// Retrieves the current history log
        /// </summary>
        /// <returns>The collection of events</returns>
        public IReadOnlyCollection<TrackerHistoryEvent> GetHistory() => _isMultiworld ? new List<TrackerHistoryEvent>() : _historyEvents.ToList();

        /// <summary>
        /// Creates the progression log based off of the history
        /// </summary>
        /// <param name="rom">The rom that the history is for</param>
        /// <param name="history">All of the events to log</param>
        /// <param name="importantOnly">If only important events should be logged or not</param>
        /// <returns>The generated log text</returns>
        public static string GenerateHistoryText(GeneratedRom rom, IReadOnlyCollection<TrackerHistoryEvent> history, bool importantOnly = true)
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

            var finalTime = TimeSpan.FromSeconds(rom.TrackerState?.SecondsElapsed ?? 0).ToString(@"hh\:mm\:ss");

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
