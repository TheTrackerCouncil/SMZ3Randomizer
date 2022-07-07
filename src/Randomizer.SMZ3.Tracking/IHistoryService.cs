using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Module for changing the map
    /// </summary>
    public interface IHistoryService
    {

        public void StartHistory(Tracker tracker);

        public void LoadHistory(Tracker tracker, TrackerState state);

        public void AddEvent(HistoryEventType type, bool isImportant, string objectName, Location? location = null);

        public void AddEvent(HistoryEventType type, bool isImportant, string objectName, LocationInfo? location);

        public void AddEvent(TrackerHistoryEvent histEvent);

        public void RemoveLastEvent();

        public void Remove(TrackerHistoryEvent histEvent);

        public IReadOnlyCollection<TrackerHistoryEvent> GetHistory();

        public string GenerateHistoryText(GeneratedRom rom, IReadOnlyCollection<TrackerHistoryEvent> history, bool importantOnly = true);
    }
}
