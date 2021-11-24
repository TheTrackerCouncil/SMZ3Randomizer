using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

using BunLabs;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class SpoilerModule : TrackerModule, IOptionalModule
    {
        public SpoilerModule(Tracker tracker, ILogger<SpoilerModule> logger)
            : base(tracker, logger)
        {
            AddCommand("Reveal item location", GetItemSpoilerRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out var itemName);
                RevealItemLocation(item);
            });
        }

        public void RevealItemLocation(ItemData item)
        {
            if (!item.Multiple && !item.HasStages && item.TrackingState > 0)
            {
                Tracker.Say("You already have that.");
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

            // ENTER SPOILER TERRITORY

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
    }
}
