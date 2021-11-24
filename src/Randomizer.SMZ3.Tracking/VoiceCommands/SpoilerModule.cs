using System;
using System.Linq;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands that give hints or spoilers about items and
    /// locations.
    /// </summary>
    public class SpoilerModule : TrackerModule, IOptionalModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpoilerModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to write logging information.</param>
        public SpoilerModule(Tracker tracker, ILogger<SpoilerModule> logger)
            : base(tracker, logger)
        {
            AddCommand("Enable spoilers", GetEnableSpoilersRule(), (tracker, result) =>
            {
                SpoilersEnabled = true;
                tracker.Say("Toggled spoilers on.");
            });
            AddCommand("Disable spoilers", GetDisableSpoilersRule(), (tracker, result) =>
            {
                SpoilersEnabled = false;
                tracker.Say("Toggled spoilers off.");
            });

            AddCommand("Reveal item location", GetItemSpoilerRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out var itemName);
                RevealItemLocation(item);
            });
        }

        /// <summary>
        /// Gets or sets a value indicating whether Tracker may give spoilers
        /// when asked about items or locations.
        /// </summary>
        public bool SpoilersEnabled { get; set; }

        /// <summary>
        /// Gives a hint or spoiler about the location of an item.
        /// </summary>
        /// <param name="item">The item to find.</param>
        public void RevealItemLocation(ItemData item)
        {
            if (item.HasStages && item.TrackingState >= item.MaxStage)
            {
                Tracker.Say(string.Format("You already have every {0}.", item.Name));
                return;
            }
            else if (!item.Multiple && item.TrackingState > 0)
            {
                Tracker.Say(string.Format("You already have {0}.", item.NameWithArticle));
                return;
            }

            var markedLocation = Tracker.MarkedLocations
                .Where(x => x.Value.InternalItemType == item.InternalItemType)
                .Select(x => Tracker.World.Locations.Single(y => y.Id == x.Key))
                .Where(x => !x.Cleared)
                .Random();
            if (markedLocation != null)
            {
                var locationName = Tracker.GetName(markedLocation);
                var regionName = Tracker.WorldInfo.Region(markedLocation.Region).Name;
                Tracker.Say(string.Format("You've marked {0} at {1} <break strength='weak'/> in {2}", item.NameWithArticle, locationName, regionName));
                return;
            }

            if (!SpoilersEnabled)
            {
                Tracker.Say("If you want me to spoil it, say 'Hey tracker, enable spoilers'.");
                return;
            }

            var progression = Tracker.GetProgression(assumeKeys: Tracker.World.Config.Keysanity);
            var reachableLocation = Tracker.World.Locations
                .Where(x => x.Item.Type == item.InternalItemType)
                .Where(x => !x.Cleared)
                .Where(x => x.IsAvailable(progression))
                .Random();
            if (reachableLocation != null)
            {
                var locationName = Tracker.WorldInfo.Location(reachableLocation).Name;
                var regionName = Tracker.WorldInfo.Region(reachableLocation.Region).Name;
                if (item.Multiple || item.HasStages)
                    Tracker.Say(string.Format("There is {0} at {1} <break strength='weak'/> in {2}", item.NameWithArticle, locationName, regionName));
                else
                    Tracker.Say(string.Format("{0} is at {1} <break strength='weak'/> in {2}.", item.NameWithArticle, locationName, regionName));
                return;
            }

            var worldLocation = Tracker.World.Locations
                .Where(x => x.Item.Type == item.InternalItemType)
                .Where(x => !x.Cleared)
                .Random();
            if (worldLocation != null)
            {
                var locationName = Tracker.WorldInfo.Location(worldLocation).Name;
                var regionName = Tracker.WorldInfo.Region(worldLocation.Region).Name;
                if (item.Multiple || item.HasStages)
                    Tracker.Say(string.Format("There is {0} at {1} <break strength='weak'/> in {2}, but you cannot get it yet.", item.NameWithArticle, locationName, regionName));
                else
                    Tracker.Say(string.Format("{0} is at {1} <break strength='weak'/> in {2}, but it is out of logic.", item.NameWithArticle, locationName, regionName));
                return;
            }

            // RIP

            if (item.Multiple || item.HasStages)
                Tracker.Say(string.Format("I cannot find any more {0}.", item.Plural));
            else
                Tracker.Say(string.Format("I cannot find {0}.", item.NameWithArticle));
        }

        private GrammarBuilder GetItemSpoilerRule()
        {
            var items = GetItemNames();

            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("where is", "where's", "where are")
                .Optional("the", "a", "an")
                .Append(ItemNameKey, items);
        }

        private GrammarBuilder GetEnableSpoilersRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("enable", "turn on")
                .Append("spoilers");
        }

        private GrammarBuilder GetDisableSpoilersRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("disable", "turn off")
                .Append("spoilers");
        }
    }
}
