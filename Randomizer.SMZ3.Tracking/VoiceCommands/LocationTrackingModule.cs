using System;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for marking and clearing locations.
    /// </summary>
    public class LocationTrackingModule : TrackerModule
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="LocationTrackingModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        public LocationTrackingModule(Tracker tracker) : base(tracker)
        {
            AddCommand("Track item at specific location", GetTrackItemAtLocationRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out _);
                var location = GetLocationFromResult(tracker, result);
                tracker.MarkLocation(location, item, result.Confidence);
            });

            AddCommand("Clear specific item location", GetClearLocationRule(), (tracker, result) =>
            {
                var location = GetLocationFromResult(tracker, result);
                tracker.Clear(location, result.Confidence);
            });
        }

        private GrammarBuilder GetTrackItemAtLocationRule()
        {
            var itemNames = GetItemNames();
            var locationNames = GetLocationNames();

            var itemIsAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(ItemNameKey, itemNames)
                .OneOf("is at", "are at")
                .Append(LocationKey, locationNames);

            var theItemIsAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("a", "an", "the")
                .Append(ItemNameKey, itemNames)
                .OneOf("is at", "are at")
                .Append(LocationKey, locationNames);

            var thereIsItemAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("there are", "there is a", "there is an")
                .Append(ItemNameKey, itemNames)
                .Append("at")
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
                itemIsAtLocation, theItemIsAtLocation, thereIsItemAtLocation,
                locationHasItem, markAtLocation);
        }

        private GrammarBuilder GetClearLocationRule()
        {
            var locationNames = GetLocationNames();

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("clear", "ignore")
                .Append(LocationKey, locationNames);
        }
    }
}
