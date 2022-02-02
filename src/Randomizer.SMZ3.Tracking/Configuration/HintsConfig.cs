using System;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Provides the phrases for hints.
    /// </summary>
    public class HintsConfig
    {
        /// <summary>
        /// Gets the phrases to respond with when hints are turned on.
        /// </summary>
        public SchrodingersString EnabledHints { get; init; }
            = new("Toggled hints on.");

        /// <summary>
        /// Gets the phrases to respond with when hints are turned off.
        /// </summary>
        public SchrodingersString DisabledHints { get; init; }
            = new("Toggled hints off.");

        /// <summary>
        /// Gets the phrases to respond with when asked about an item and hints
        /// are turned off.
        /// </summary>
        public SchrodingersString PromptEnableItemHints { get; init; }
            = new("If you want me to give a hint, say 'Hey tracker, enable hints'.");

        /// <summary>
        /// Gets the phrases to respond with when asked about a location and
        /// hints are turned off.
        /// </summary>
        public SchrodingersString PromptEnableLocationHints { get; init; }
            = new("If you want me to give a hint, say 'Hey tracker, enable hints'.");

        /// <summary>
        /// Gets the phrases to respond with when there are no applicable hints
        /// for the item or location that was asked about.
        /// </summary>
        public SchrodingersString NoApplicableHints { get; init; }
            = new("Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.");

        /// <summary>
        /// Gets the hints for items that are not in logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemNotInLogic { get; init; }
            = new("You need something else before you can find {0}.");

        /// <summary>
        /// Gets the hints for items that are only found in Super Metroid.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInSuperMetroid { get; init; }
            = new("You might find {0} on a strange planet.");

        /// <summary>
        /// Gets the hints for items that are only found in A Link to the Past.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInALttP { get; init; }
            = new("You might find {0} in a world of light and dark.");

        /// <summary>
        /// Gets the hints for items that are not in logic and mention one or
        /// more missing items.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name(s) of the missing
        /// item(s).
        /// </remarks>
        public SchrodingersString ItemRequiresOtherItem { get; init; }
            = new("You need {1} before you can find {0}.");

        /// <summary>
        /// Gets the hints for items that are not in logic and require too many items to mention.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemRequiresManyOtherItems { get; init; }
            = new("You need a lot of other items before you can find {0}.");

        /// <summary>
        /// Gets the hints for items that are not found in the specified region
        /// or area.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name of the
        /// region/area.
        /// </remarks>
        public SchrodingersString ItemNotInArea { get; init; }
            = new("You won't find {0} in {1}.");

        /// <summary>
        /// Gets the hints for items that are in playthrough sphere zero.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInSphereZero { get; init; }
            = new("How have you not found {0} yet?");

        /// <summary>
        /// Gets the hints for items that are in an early sphere.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInEarlySphere { get; init; }
            = new("{0} can be found pretty early on.");

        /// <summary>
        /// Gets the hints for items that are in a late sphere.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInLateSphere { get; init; }
            = new("Don't count on getting {0} any time soon.");

        /// <summary>
        /// Gets the hints for items that are in a dungeon that has been visited
        /// already.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInPreviouslyVisitedDungeon { get; init; }
            = new("How do you feel about double dipping?");

        /// <summary>
        /// Gets the hints for items that are in a dungeon that has not yet been
        /// visited before.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInUnvisitedDungeon { get; init; }
            = new("It's in a dungeon you haven't visited yet.");

        /// <summary>
        /// Gets the hints for items that are in a region that has been visited
        /// already.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInPreviouslyVisitedRegion { get; init; }
            = new("Deja voo, I've just been in this place before");

        /// <summary>
        /// Gets the hints for items that are in a region that has not been
        /// visited yet.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the".
        /// </remarks>
        public SchrodingersString ItemInUnvisitedRegion { get; init; }
            = new("It's in a place you haven't been yet.");

        /// <summary>
        /// Gets the hints for items that are in locations that have a bad name
        /// in the original randomizer.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name of the location
        /// in the original randomizer code.
        /// </remarks>
        public SchrodingersString ItemHasBadVanillaLocationName { get; init; }
            = new("It's in {1}. Good luck figuring out what that means.");

        /// <summary>
        /// Gets the hints for items that are in locations which have junk items
        /// in the vanilla game.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
        /// or "the". <c>{1}</c> is a placeholder for the name of the vanilla
        /// item in the same location.
        /// </remarks>
        public SchrodingersString ItemIsInVanillaJunkLocation { get; init; }
            = new("In the regular game, you can find {1} there.");

        /// <summary>
        /// Gets the hints for locations that have been cleared already.
        /// </summary>
        public SchrodingersString LocationAlreadyCleared { get; init; }
            = new("Haven't you been there already?");

        /// <summary>
        /// Gets the hints for locations that don't have anything useful.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is placeholder for the name of the location.
        /// </remarks>
        public SchrodingersString LocationHasJunkItem { get; init; }
            = new("There's nothing important there.");

        /// <summary>
        /// Gets the hints for locations that might have something useful.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is placeholder for the name of the location.
        /// </remarks>
        public SchrodingersString LocationHasUsefulItem { get; init; }
            = new("It might be worth checking out.");

        /// <summary>
        /// Gets the hints for locations that have an item from the other game.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is placeholder for the name of the location.
        /// </remarks>
        public SchrodingersString LocationHasItemFromOtherGame { get; init; }
            = new("It's got something from a different universe.");

        /// <summary>
        /// Gets the hints for locations that have an item from the correct
        /// game.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is placeholder for the name of the location.
        /// </remarks>
        public SchrodingersString LocationHasItemFromCorrectGame { get; init; }
            = new("The item there is in the correct game, at least.");
    }
}
