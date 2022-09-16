using System;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration
{
    /// <summary>
    /// Provides the phrases for spoilers.
    /// </summary>
    public class SpoilerConfig : IMergeable<SpoilerConfig>
    {
        /// <summary>
        /// Gets the phrases to respond with when spoilers are turned on.
        /// </summary>
        public SchrodingersString EnabledSpoilers { get; init; }
            = new("Toggled spoilers on.");

        /// <summary>
        /// Gets the phrases to respond with when spoilers are turned off.
        /// </summary>
        public SchrodingersString DisabledSpoilers { get; init; }
            = new("Toggled spoilers off.");

        /// <summary>
        /// Gets the phrases to respond with when asked about an item and
        /// spoilers are disabled.
        /// </summary>
        public SchrodingersString PromptEnableItemSpoilers { get; init; }
            = new("If you want me to spoil it, say 'Hey tracker, enable spoilers'.");

        /// <summary>
        /// Gets the phrases to respond with when asked about a location and
        /// spoilers are disabled.
        /// </summary>
        public SchrodingersString PromptEnableLocationSpoilers { get; init; }
            = new("Why don't you go find out? Or just say 'Hey tracker, enable spoilers' and I might tell you.");

        /// <summary>
        /// Gets the phrases to respond with when asking about an item that is
        /// already at the highest stage.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item.
        /// </remarks>
        public SchrodingersString TrackedAllItemsAlready { get; init; }
            = new("You already have every {0}.");

        /// <summary>
        /// Gets the phrases to respond with when asking about an item that has
        /// already been tracked.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, including "a",
        /// "an" or "the".
        /// </remarks>
        public SchrodingersString TrackedItemAlready { get; init; }
            = new("You already have {0}.");

        /// <summary>
        /// Gets the phrases to respond with when asking about an item that has
        /// been marked.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name of the location.
        /// <c>{2}</c> is a placeholder for the name of the region.
        /// </remarks>
        public SchrodingersString MarkedItem { get; init; }
            = new("You've marked {0} at {1} <break strength='weak'/> in {2}");

        /// <summary>
        /// Gets the phrases to respond with when asking about a location whose
        /// item has already been marked.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
        /// is a placeholder for the name of the marked item, with "a", "an" or
        /// "the".
        /// </remarks>
        public SchrodingersString MarkedLocation { get; init; }
             = new("You've marked {1} at {0}.");

        /// <summary>
        /// Gets the phrases to respond with when the location that was asked
        /// about did not have an item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the location.
        /// </remarks>
        public SchrodingersString EmptyLocation { get; init; }
            = new("{0} does not have an item. Did you forget to generate a seed first?");

        /// <summary>
        /// Gets the phrases to respond with when the item could not be found.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemNotFound { get; init; }
            = new("I cannot find {0}.");

        /// <summary>
        /// Gets the phrases to respond with when no instances of an item could
        /// be found anymore.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the plural name of the item.
        /// </remarks>
        public SchrodingersString ItemsNotFound { get; init; }
            = new("I cannot find any more {0}.");

        /// <summary>
        /// Gets the phrases to respond with when all locations that have the item are cleared.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString LocationsCleared { get; init; }
            = new("You already cleared every location that has {0}.");

        /// <summary>
        /// Gets the phrases that spoil the item that is at the requested
        /// location.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
        /// is a placeholder for the name of the item, with "a", "an" or "the".
        /// </remarks>
        public SchrodingersString LocationHasItem { get; init; }
            = new("{0} has {1}");

        /// <summary>
        /// Gets the phrases that spoil the item that is at the requested
        /// location, when the item does not exist in the item data.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
        /// is a placeholder for the name of the item, with "a", "an" or "the".
        /// </remarks>
        public SchrodingersString LocationHasUnknownItem { get; init; }
            = new("{0} has {1}");

        /// <summary>
        /// Gets the phrases that spoil the location that has the requested
        /// item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name of the location.
        /// <c>{2}</c> is a placeholder for the name of the region.
        /// </remarks>
        public SchrodingersString ItemIsAtLocation { get; init; }
            = new("{0} is at {1} <break strength='weak'/> in {2}.");

        /// <summary>
        /// Gets the phrases that spoil one of the locations that has the
        /// requested item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name of the location.
        /// <c>{2}</c> is a placeholder for the name of the region.
        /// </remarks>
        public SchrodingersString ItemsAreAtLocation { get; init; }
            = new("There is {0} at {1} <break strength='weak'/> in {2}");

        /// <summary>
        /// Gets the phrases that spoil the location that has the requested
        /// item, but the location is out of logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name of the location.
        /// <c>{2}</c> is a placeholder for the name of the region.
        /// </remarks>
        public SchrodingersString ItemIsAtOutOfLogicLocation { get; init; }
            = new("{0} is at {1} <break strength='weak'/> in {2}, but it is out of logic.");

        /// <summary>
        /// Gets the phrases that spoil one of the locations that has the
        /// requested item, but the location is out of logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name of the location.
        /// <c>{2}</c> is a placeholder for the name of the region.
        /// </remarks>
        public SchrodingersString ItemsAreAtOutOfLogicLocation { get; init; }
            = new("There is {0} at {1} <break strength='weak'/> in {2}, but you cannot get it yet.");

        /// <summary>
        /// Gets the phrases that mention all the items in an area.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the room or region.
        /// <c>{1}</c> is a placeholder for the names of the items left.
        /// </remarks>
        public SchrodingersString ItemsInArea { get; init; }
            = new("{0} has {1}");

    }
}
