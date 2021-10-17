using System;
using System.Linq;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class LocationTrackingModule : TrackerModule
    {
        private const string LocationKey = "LocationName";

        public LocationTrackingModule(Tracker tracker) : base(tracker)
        {
            AddCommand("TrackItemAtLocationRule", GetTrackItemAtLocationRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out _);
                var location = GetLocationFromResult(tracker, result);
                tracker.MarkLocation(location, item, result.Confidence);
            });
        }

        private static Location GetLocationFromResult(Tracker tracker, RecognitionResult result)
        {
            var id = (int)result.Semantics[LocationKey].Value;
            var location = tracker.World.Locations.SingleOrDefault(x => x.Id == id);
            return location ?? throw new Exception($"Could not find a location with ID {id} (\"{result.Text}\")");
        }

        private GrammarBuilder GetTrackItemAtLocationRule()
        {
            var itemNames = GetItemNames();
            var locationNames = GetLocationNames();

            var itemIsAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("a", "an", "the")
                .Append(ItemNameKey, itemNames)
                .OneOf("is at")
                .Append(LocationKey, locationNames);

            var locationHasItem = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(LocationKey, locationNames)
                .OneOf("has", "has a", "has an", "has the")
                .Append(ItemNameKey, itemNames);

            var markAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(ItemNameKey, itemNames)
                .Append("at")
                .Append(LocationKey, locationNames);

            return GrammarBuilder.Combine(
                itemIsAtLocation, locationHasItem, markAtLocation);
        }

        private Choices GetLocationNames()
        {
            var locationNames = new Choices();

            // There's too many different locations and they don't all have
            // unique names. Sort this mess out later for problematic locations.
            foreach (var location in Tracker.World.Locations)
            {
                locationNames.Add(new SemanticResultValue(location.Name, location.Id));
                foreach (var name in location.AlternateNames)
                    locationNames.Add(new SemanticResultValue(name, location.Id));
            }

            return locationNames;
        }
    }
}
