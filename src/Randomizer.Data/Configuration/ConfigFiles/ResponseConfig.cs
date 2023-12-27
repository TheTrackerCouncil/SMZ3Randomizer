using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Represents the various phrases that Tracker can respond with.
    /// </summary>
    public class ResponseConfig : IMergeable<ResponseConfig>, IConfigFile<ResponseConfig>
    {
        /// <summary>
        /// Gets the phrases to respond with when tracker starts.
        /// </summary>
        [Display(Name = "StartedTracking")]
        public SchrodingersString? StartedTracking { get; init; }
            = new SchrodingersString("Tracker started");

        /// <summary>
        /// Gets the phrases to respond with when tracker starts in "alternate"
        /// mode.
        /// </summary>
        [Display(Name = "StartingTrackingAlternate")]
        public SchrodingersString? StartingTrackingAlternate { get; init; }
        = new SchrodingersString("Hello. I'm your substitude tracker for today.");

        /// <summary>
        /// Gets the phrases to respond with when tracker stops.
        /// </summary>
        [Display(Name = "StoppedTracking")]
        public SchrodingersString? StoppedTracking { get; init; }
            = new SchrodingersString("Tracker stopped");

        /// <summary>
        /// Gets the phrases to respond with when tracker stops after go mode
        /// has been turned on.
        /// </summary>
        [Display(Name = "StoppedTrackingPostGoMode")]
        public SchrodingersString? StoppedTrackingPostGoMode { get; init; }
            = new SchrodingersString("Tracker stopped");

        /// <summary>
        /// Gets the phrases to respond with when speech recognition confidence
        /// does not meet the configured threshold for execution, but is high
        /// enough to be recognized.
        /// </summary>
        [Display(Name = "Misheard")]
        public SchrodingersString Misheard { get; init; }
            = new SchrodingersString("I didn't quite get that.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a simple single-stage
        /// item.
        /// <c>{0}</c> is a placeholder for the name of the item.
        /// </summary>
        [Display(Name = "TrackedItem")]
        public SchrodingersString TrackedItem { get; init; }
            = new SchrodingersString("Toggled {0} on.");

        /// <summary>
        /// Gets the phrases to respond with when tracking two items at once.
        /// <c>{0}</c> is a placeholder for the name of the first item.
        /// <c>{1}</c> is a placeholder for the name of the second item.
        /// </summary>
        [Display(Name = "TrackedTwoItems")]
        public SchrodingersString TrackedTwoItems { get; init; }
            = new SchrodingersString("Tracked {0} and {1}");

        /// <summary>
        /// Gets the phrases to respond with when tracking three items at once.
        /// <c>{0}</c> is a placeholder for the name of the first item.
        /// <c>{1}</c> is a placeholder for the name of the second item.
        /// <c>{2}</c> is a placeholder for the name of the third item.
        /// </summary>
        [Display(Name = "TrackedThreeItems")]
        public SchrodingersString TrackedThreeItems { get; init; }
            = new SchrodingersString("Tracked {0}, {1}, and {2}");

        /// <summary>
        /// Gets the phrases to respond with when tracking more than three items
        /// <c>{0}</c> is a placeholder for the name of the first item.
        /// <c>{1}</c> is a placeholder for the name of the second item.
        /// <c>{2}</c> is the number of remaining items tracked.
        /// </summary>
        [Display(Name = "TrackedManyItems")]
        public SchrodingersString TrackedManyItems { get; init; }
            = new SchrodingersString("Tracked {0}, {1}, and {2} other items");

        /// <summary>
        /// Gets the phrases to respond with when tracking a progressive item
        /// using a specific stage name.
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </summary>
        [Display(Name = "TrackedItemByStage")]
        public SchrodingersString TrackedItemByStage { get; init; }
            = new SchrodingersString("Marked {0} as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that can be
        /// tracked multiple times.
        /// <c>{0}</c> is a placeholder for the plural item name. <c>{1}</c> is
        /// a placeholder for the number of copies.
        /// </summary>
        [Display(Name = "TrackedItemMultiple")]
        public SchrodingersString TrackedItemMultiple { get; init; }
             = new SchrodingersString("Added more {0}.", "You now have {1} {0}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a progressive item.
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </summary>
        [Display(Name = "TrackedProgressiveItem")]
        public SchrodingersString TrackedProgressiveItem { get; init; }
            = new SchrodingersString("Upgraded {0} by one step.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a lower tier of an
        /// item than you already have, e.g. tracking Master Sword when you have
        /// the Tempered Sword.
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </summary>
        [Display(Name = "TrackedOlderProgressiveItem")]
        public SchrodingersString? TrackedOlderProgressiveItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that is
        /// already at the max tier.
        /// </summary>
        [Display(Name = "TrackedTooManyOfAnItem")]
        public SchrodingersString? TrackedTooManyOfAnItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking a single-stage item
        /// that is already tracked.
        /// <c>{0}</c> is a placeholder for the name of the item.
        /// </summary>
        [Display(Name = "TrackedAlreadyTrackedItem")]
        public SchrodingersString? TrackedAlreadyTrackedItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that is at
        /// its correct location according to the original game.
        /// </summary>
        [Display(Name = "TrackedVanillaItem")]
        public SchrodingersString TrackedVanillaItem { get; init; }
            = new("");

        /// <summary>
        /// Gets the phrases to respond with when setting the exact amount of an
        /// item, but that exact amount is already tracked.
        /// <c>{0}</c> is the plural name of the item. <c>{1}</c> is the number
        /// of items.
        /// </summary>
        [Display(Name = "TrackedExactAmountDuplicate")]
        public SchrodingersString TrackedExactAmountDuplicate { get; init; }
            = new("You already have {1} {0}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking multiple items in an
        /// area at once.
        /// </summary>
        [Display(Name = "TrackedMultipleItems")]
        public SchrodingersString TrackedMultipleItems { get; init; }
            = new SchrodingersString("Tracked {2} in {1}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing multiple items in an
        /// area at once.
        /// </summary>
        [Display(Name = "ClearedMultipleItems")]
        public SchrodingersString ClearedMultipleItems { get; init; }
            = new SchrodingersString("Cleared {0} items in {1}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking or clearing multiple
        /// items in an area at once, but there no items left.
        /// <c>{0}</c> is a placeholder for the name of the area.
        /// </summary>
        [Display(Name = "TrackedNothing")]
        public SchrodingersString TrackedNothing { get; init; }
            = new SchrodingersString("There are no items left in {0}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking or clearing multiple
        /// items in an area at once, but the only items left are out of logic.
        /// <c>{0}</c> is a placeholder for the name of the area. <c>{1}</c> is
        /// a placeholder for the number of items that are left but
        /// inaccessible.
        /// </summary>
        public Dictionary<int, SchrodingersString> TrackedNothingOutOfLogic { get; init; } = new Dictionary<int, SchrodingersString>
        {
            [1] = new SchrodingersString("The only item left in {0} is out of logic."),
            [2] = new SchrodingersString("The only items left in {0} are out of logic.")
        };

        /// <summary>
        /// Gets the phrases to respond with when untracking an item.
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </summary>
        [Display(Name = "UntrackedItem")]
        public SchrodingersString UntrackedItem { get; init; }
            = new SchrodingersString("Toggled {0} off.");

        /// <summary>
        /// Gets the phrases to respond with after tracking an item that is not
        /// in logic.
        /// <c>{0}</c> is a placeholder for the name of the item was tracked.
        /// <c>{1}</c> is a placeholder for the name of the location that
        /// contains the item that is out of logic. <c>{2}</c> is a placeholder
        /// for one or more names of required items that are missing.
        /// </summary>
        [Display(Name = "TrackedOutOfLogicItem")]
        public SchrodingersString? TrackedOutOfLogicItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with after tracking an item that is not
        /// in logic.
        /// <c>{0}</c> is a placeholder for the name of the item was tracked.
        /// <c>{1}</c> is a placeholder for the name of the location that
        /// contains the item that is out of logic.
        /// </summary>
        [Display(Name = "TrackedOutOfLogicItemTooManyMissing")]
        public SchrodingersString? TrackedOutOfLogicItemTooManyMissing { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when untracking an item that hasn't
        /// been tracked yet.
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </summary>
        [Display(Name = "UntrackedNothing")]
        public SchrodingersString UntrackedNothing { get; init; }
            = new SchrodingersString("There's nothing to remove.");

        /// <summary>
        /// Gets the phrases to respond with when untracking an item that has
        /// multiple stages.
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </summary>
        [Display(Name = "UntrackedProgressiveItem")]
        public SchrodingersString UntrackedProgressiveItem { get; init; }
            = new SchrodingersString("Decreased {0} by one step.");

        /// <summary>
        /// Gets the phrases to respond with when untracking an item that can be
        /// tracked multiple times.
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </summary>
        [Display(Name = "UntrackedItemMultiple")]
        public SchrodingersString UntrackedItemMultiple { get; init; }
            = new SchrodingersString("Removed {1}.");

        /// <summary>
        /// Gets the phrases to respond with when untracking the last copy of an
        /// item that can be tracked multiple times.
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </summary>
        [Display(Name = "UntrackedItemMultipleLast")]
        public SchrodingersString UntrackedItemMultipleLast { get; init; }
            = new SchrodingersString("Removed your last {0}.");

        /// <summary>
        /// Gets the phrases to respond with when Shaktool becomes available.
        /// </summary>
        [Display(Name = "ShaktoolAvailable")]
        public SchrodingersString? ShaktoolAvailable { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when Peg World becomes available.
        /// </summary>
        [Display(Name = "PegWorldAvailable")]
        public SchrodingersString? PegWorldAvailable { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when Peg World mode is toggled on.
        /// </summary>
        [Display(Name = "PegWorldModeOn")]
        public SchrodingersString? PegWorldModeOn { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when pegging a Peg World peg.
        /// </summary>
        [Display(Name = "PegWorldModePegged")]
        public SchrodingersString? PegWorldModePegged { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when all Peg World pegs have been
        /// pegged or Peg World mode is toggled off manually.
        /// </summary>
        [Display(Name = "PegWorldModeDone")]
        public SchrodingersString? PegWorldModeDone { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when marking the reward at a
        /// dungeon.
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of the reward that was marked.
        /// </summary>
        [Display(Name = "DungeonRewardMarked")]
        public SchrodingersString DungeonRewardMarked { get; init; }
            = new SchrodingersString("Marked {0} as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when marking the reward for all
        /// other unmarked dungeons.
        /// <c>{0}</c> is a placeholder for the name of the reward that was
        /// marked.
        /// </summary>
        [Display(Name = "RemainingDungeonsMarked")]
        public SchrodingersString RemainingDungeonsMarked { get; init; }
            = new SchrodingersString("Marked remaining dungeons as {0}.");

        /// <summary>
        /// Gets the phrases to respond with when there are no unmarked
        /// dungeons.
        /// </summary>
        [Display(Name = "NoRemainingDungeons")]
        public SchrodingersString NoRemainingDungeons { get; init; }
            = new SchrodingersString("You already marked every dungeon.");

        /// <summary>
        /// Gets the phrases to respond with when clearing all locations in a
        /// dungeon.
        /// <c>{0}</c> is a placeholder for the name of the dungeon.
        /// </summary>
        [Display(Name = "DungeonCleared")]
        public SchrodingersString DungeonCleared { get; init; }
             = new("Cleared everything in {0}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing all location in a
        /// dungeon, but all locations are already cleared.
        /// <c>{0}</c> is a placeholder for the name of the dungeon.
        /// </summary>
        [Display(Name = "DungeonAlreadyCleared")]
        public SchrodingersString DungeonAlreadyCleared { get; init; }
            = new("But you already got everything in {0}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing all locations in a
        /// dungeon, but some of the cleared locations were out of logic.
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of a location that was missed.
        /// <c>{2}</c> is a placeholder for the items that are required for a
        /// missed location.
        /// </summary>
        [Display(Name = "DungeonClearedWithInaccessibleItems")]
        public SchrodingersString DungeonClearedWithInaccessibleItems { get; init; }
            = new("Including some out of logic checks that require {2}, such as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing all locations in a
        /// dungeon, but some of the cleared locations were out of logic with too many missing items.
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of a location that was missed.
        /// </summary>
        [Display(Name = "DungeonClearedWithTooManyInaccessibleItems")]
        public SchrodingersString DungeonClearedWithTooManyInaccessibleItems { get; init; }
            = new("Are you sure you got everything?");

        /// <summary>
        /// Gets the phrases to respond with when clearing a dungeon.
        /// <c>{0}</c> is a placeholder for the name of the dungeon that was
        /// cleared. <c>{1}</c> is a placeholder for the boss of the dungeon.
        /// </summary>
        [Display(Name = "DungeonBossCleared")]
        public SchrodingersString DungeonBossCleared { get; init; }
            = new SchrodingersString("Cleared {0}.", "Marked {1} as defeated.");

        /// <summary>
        /// Gets the phrases to respond with when clearing a dungeon that was
        /// already cleared.
        /// <c>{0}</c> is a placeholder for the name of the dungeon that was
        /// cleared. <c>{1}</c> is a placeholder for the boss of the dungeon.
        /// </summary>
        [Display(Name = "DungeonBossAlreadyCleared")]
        public SchrodingersString DungeonBossAlreadyCleared { get; init; }
            = new SchrodingersString("You already cleared {0}.", "You already defeated {1}.");

        /// <summary>
        /// Gets the phrases to respond with when reverting the cleared status
        /// of a dungeon.
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the boss of the dungeon.
        /// </summary>
        [Display(Name = "DungeonBossUncleared")]
        public SchrodingersString DungeonBossUncleared { get; init; }
            = new SchrodingersString("Reset {0}.", "Marked {1} as still alive.");

        /// <summary>
        /// Gets the phrases to respond with when reverting the cleared status
        /// of a dungeon that wasn't cleared yet.
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the boss of the dungeon.
        /// </summary>
        [Display(Name = "DungeonBossNotYetCleared")]
        public SchrodingersString DungeonBossNotYetCleared { get; init; }
            = new SchrodingersString("You haven't cleared {0} yet.", "You never defeated {1} in the first place.");

        /// <summary>
        /// Gets the phrases to respond with when the player defeated a boss
        /// with remarkable skill and add content!
        /// </summary>
        [Display(Name = "DungeonBossClearedAddContent")]
        public SchrodingersString DungeonBossClearedAddContent { get; init; }
            = new SchrodingersString("That was some impressive work.");

        /// <summary>
        /// Gets the phrases to respond with when marking the medallion
        /// requirement of a dungeon.
        /// <c>{0}</c> is a placeholder for the name of the medallion.
        /// <c>{1}</c> is a placeholder for the name of the dungeon.
        /// </summary>
        [Display(Name = "DungeonRequirementMarked")]
        public SchrodingersString DungeonRequirementMarked { get; init; }
            = new SchrodingersString("Marked {0} as required for {1}");

        /// <summary>
        /// Gets the phrases to respond with when marking the wrong dungeon with
        /// a required medallion.
        /// <c>{0}</c> is a placeholder for the name of the dungeon.
        /// </summary>
        [Display(Name = "DungeonRequirementInvalid")]
        public SchrodingersString DungeonRequirementInvalid { get; init; }
            = new SchrodingersString("{0} doesn't need medallions");

        /// <summary>
        /// Gets the phrases to respond with when marking a dungeon with a
        /// requirement that doesn't match the seed logic.
        /// <c>{0}</c> is a placeholder for the name of the medallion in the
        /// seed. <c>{1}</c> is a placeholder for the name of the dungeon.
        /// <c>{2}</c> is a placeholder for the name of the medallion that was
        /// tracked.
        /// </summary>
        [Display(Name = "DungeonRequirementMismatch")]
        public SchrodingersString? DungeonRequirementMismatch { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking treasure in a
        /// dungeon. The dictionary key represents the amount of items left,
        /// where 2 is 2 or more and -1 is when the dungeon is already empty.
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is the amount of items left after tracking.
        /// </summary>
        public Dictionary<int, SchrodingersString> DungeonTreasureTracked { get; init; } = new()
        {
            [2] = new SchrodingersString("{1} items left in {0}."),
            [1] = new SchrodingersString("One item left in {0}."),
            [0] = new SchrodingersString("Nothing left in {0}."),
            [-1] = new SchrodingersString("You already got everything in {0}.")
        };

        /// <summary>
        /// Gets the phrases to respond with when clearing more treasure chests
        /// in a dungeon than there are left.
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is the actual amount of items left. <c>{2}</c> is the amount of
        /// items requested to clear.
        /// </summary>
        [Display(Name = "DungeonTooManyTreasuresTracked")]
        public SchrodingersString? DungeonTooManyTreasuresTracked { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item in a specific
        /// dungeon, but that dungeon does not have that item in the seed.
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of the item, including "a" or "the",
        /// as appropriate.
        /// </summary>
        [Display(Name = "ItemTrackedInIncorrectDungeon")]
        public SchrodingersString? ItemTrackedInIncorrectDungeon { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when marking an item at a location.
        /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
        /// is a placeholder for the name of the item.
        /// </summary>
        [Display(Name = "LocationMarked")]
        public SchrodingersString LocationMarked { get; init; }
            = new SchrodingersString("Marked {1} at {0}.");

        /// <summary>
        /// Gets the phrases to respond with when marking an item at a location
        /// that has already been marked.
        /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
        /// is a placeholder for the name of the item. <c>{2}</c> is a
        /// placeholder for the name of the item that was previously marked
        /// here.
        /// </summary>
        [Display(Name = "LocationMarkedAgain")]
        public SchrodingersString LocationMarkedAgain { get; init; }
            = new SchrodingersString("Replaced {2} with {1} at {0}.");

        /// <summary>
        /// Gets the phrases to respond with when marking an item as
        /// nothing/bullshit.
        /// <c>{0}</c> is a placeholder for the name of the location.
        /// </summary>
        [Display(Name = "LocationMarkedAsBullshit")]
        public SchrodingersString LocationMarkedAsBullshit { get; init; }
            = new SchrodingersString("Cleared {0}.");

        /// <summary>
        /// Gets the phrases to respond with when marking an item at a location that was preconfigured.
        /// <c>{0}</c> is a placeholder for the name of the location.
        /// </summary>
        [Display(Name = "LocationMarkedPreConfigured")]
        public SchrodingersString? LocationMarkedPreConfigured { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item at a location that was preconfigured.
        /// <c>{0}</c> is a placeholder for the name of the location.
        /// </summary>
        [Display(Name = "TrackedPreConfigured")]
        public SchrodingersString? TrackedPreConfigured { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when marking or tracking an item at
        /// a location, when the seed contains a different item at the same
        /// location.
        /// <c>{0}</c> is a placeholder for the item that was tracked or marked.
        /// <c>{1}</c> is a placeholder for the item that was in the same
        /// location in the seed.
        /// </summary>
        [Display(Name = "LocationHasDifferentItem")]
        public SchrodingersString? LocationHasDifferentItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when clearing a location.
        /// <c>{0}</c> is a placeholder for the name of the location.
        /// </summary>
        [Display(Name = "LocationCleared")]
        public SchrodingersString LocationCleared { get; init; }
            = new SchrodingersString("Cleared {0}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing multiple locations.
        /// <c>{0}</c> is a placeholder for the number of locations.
        /// </summary>
        [Display(Name = "LocationsCleared")]
        public SchrodingersString LocationsCleared { get; init; }
            = new SchrodingersString("Cleared {0} locations.");

        /// <summary>
        /// Gets the phrases to respond with when trying to clear the last marked locations when there aren't any
        /// </summary>
        [Display(Name = "NoMarkedLocations")]
        public SchrodingersString NoMarkedLocations { get; init; }
            = new SchrodingersString("There are no marked locations to clear");

        /// <summary>
        /// Gets the phrases to respond with when clearing multiple locations
        /// from the same region.
        /// <c>{0}</c> is a placeholder for the number of locations.
        /// <c>{1}</c> is a placeholder for the name of the region
        /// </summary>
        [Display(Name = "LocationsClearedSameRegion")]
        public SchrodingersString LocationsClearedSameRegion { get; init; }
            = new SchrodingersString("Cleared {0} locations from {1}.");

        /// <summary>
        /// Gets the phrases to request tracker to initiate go mode
        /// </summary>
        public List<string> GoModePrompts { get; init; } = new() { "track Go Mode." };

        /// <summary>
        /// Gets the phrases to respond with when undoing Go Mode.
        /// </summary>
        [Display(Name = "GoModeToggledOff")]
        public SchrodingersString? GoModeToggledOff { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when attempting to track or clear
        /// an item in an area that does not have that item.
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the location. <c>{2}</c> is a
        /// placeholder for the name of the item, including "a"/"the".
        /// </summary>
        [Display(Name = "AreaDoesNotHaveItem")]
        public SchrodingersString? AreaDoesNotHaveItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when attempting to track or clear
        /// an item in an area that has multiple copies of that item.
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the location. <c>{2}</c> is a
        /// placeholder for the name of the item, including "a"/"the".
        /// </summary>
        [Display(Name = "AreaHasMoreThanOneItem")]
        public SchrodingersString? AreaHasMoreThanOneItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with after tracking an item that doesn't
        /// open up new areas.
        /// </summary>
        [Display(Name = "TrackedUselessItem")]
        public SchrodingersString? TrackedUselessItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when an internal error occurs.
        /// </summary>
        [Display(Name = "Error")]
        public SchrodingersString Error { get; init; }
             = new SchrodingersString("Oops. Something went wrong.");

        /// <summary>
        /// Gets the phrases to respond with before undoing the last action.
        /// </summary>
        [Display(Name = "ActionUndone")]
        public SchrodingersString? ActionUndone { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when there is nothing to undo.
        /// </summary>
        [Display(Name = "NothingToUndo")]
        public SchrodingersString NothingToUndo { get; init; }
            = new SchrodingersString("There's nothing to undo.");

        /// <summary>
        /// Gets the phrases to respond with when the latest undo action
        /// has expired
        /// </summary>
        [Display(Name = "UndoExpired")]
        public SchrodingersString UndoExpired { get; init; }
            = new ("There's nothing recent to undo.");

        /// <summary>
        /// Gets the phrases to respond with when changing a Tracker setting.
        /// <c>{0}</c> is a placeholder for the name of the setting that was
        /// changed. <c>{1}</c> is a placeholder for the new value.
        /// </summary>
        [Display(Name = "TrackerSettingChanged")]
        public SchrodingersString TrackerSettingChanged { get; init; }
            = new SchrodingersString("Changed {0} to {1}.");

        /// <summary>
        /// Gets the phrases to respond with when marking a boss as defeated.
        /// <c>{0}</c> is a placeholder for the name of the boss.
        /// </summary>
        [Display(Name = "BossDefeated")]
        public SchrodingersString? BossDefeated { get; init; }
            = new SchrodingersString("Congratulations on defeating {0}.");

        /// <summary>
        /// Gets the phrases to respond with when marking a boss as defeated who
        /// was already marked.
        /// <c>{0}</c> is a placeholder for the name of the boss.
        /// </summary>
        [Display(Name = "BossAlreadyDefeated")]
        public SchrodingersString? BossAlreadyDefeated { get; init; }
            = new SchrodingersString("But you already marked {0} as defeated.");

        /// <summary>
        /// Gets the phrases to respond with when un-marking a boss as defeated.
        /// <c>{0}</c> is a placeholder for the name of the boss.
        /// </summary>
        [Display(Name = "BossUndefeated")]
        public SchrodingersString? BossUndefeated { get; init; }
            = new SchrodingersString("Marking {0} as alive.");

        /// <summary>
        /// Gets the phrases to respond with when un-marking a boss as defeated
        /// who hasn't been defeated yet.
        /// <c>{0}</c> is a placeholder for the name of the boss.
        /// </summary>
        [Display(Name = "BossNotYetDefeated")]
        public SchrodingersString? BossNotYetDefeated { get; init; }
            = new SchrodingersString("But you haven't defeated {0} yet.");

        /// <summary>
        /// Gets the phrases to respond with when the timer is resumed
        /// </summary>
        [Display(Name = "TimerResumed")]
        public SchrodingersString TimerResumed { get; init; }
             = new SchrodingersString("Timer resumed.");

        /// <summary>
        /// Gets the phrases to respond with when the timer is reset
        /// </summary>
        [Display(Name = "TimerReset")]
        public SchrodingersString TimerReset { get; init; }
             = new SchrodingersString("Timer reset");

        /// <summary>
        /// Gets the phrases to respond with when the timer is paused
        /// </summary>
        [Display(Name = "TimerPaused")]
        public SchrodingersString TimerPaused { get; init; }
             = new SchrodingersString("Timer paused");

        /// <summary>
        /// Gets the phrases to respond with when tracker is muted
        /// </summary>
        [Display(Name = "Muted")]
        public SchrodingersString Muted { get; init; }
            = new SchrodingersString("Muting, say \"Hey Tracker, Unmute Yourself\" when you want me to talk again.");

        /// <summary>
        /// Gets the phrases to respond with when tracker is unmuted
        /// </summary>
        [Display(Name = "Unmuted")]
        public SchrodingersString Unmuted { get; init; }
            = new SchrodingersString("Unmuted");

        /// <summary>
        /// Gets the phrases to respond with when the game is beaten
        /// </summary>
        [Display(Name = "BeatGame")]
        public SchrodingersString BeatGame { get; init; }
            = new SchrodingersString("Good job.");

        /// <summary>
        /// Gets a dictionary that contains the phrases to respond with when no
        /// voice commands have been issued after a certain period of time, as
        /// expressed in the dictionary keys.
        /// </summary>
        public Dictionary<string, SchrodingersString> Idle { get; init; } = new();

        /// <summary>
        /// Gets the configured phrases for hints.
        /// </summary>
        public HintsConfig Hints { get; init; } = new();

        /// <summary>
        /// Gets the configured phrases for spoilers.
        /// </summary>
        public SpoilerConfig Spoilers { get; init; } = new();

        /// <summary>
        /// Gets the configured phrases for chat integration.
        /// </summary>
        public ChatConfig Chat { get; init; } = new();

        /// <summary>
        /// Gets the configured phrases for auto tracking.
        /// </summary>
        public AutoTrackerConfig AutoTracker { get; init; } = new();

        /// <summary>
        /// Gets the configured phrases for the map
        /// </summary>
        public MapResponseConfig Map { get; init; } = new();

        /// <summary>
        /// Gets the configured phrases for cheats
        /// </summary>
        public CheatsConfig Cheats { get; init; } = new();

        /// <summary>
        /// Gets the configured phrases for multiplayer
        /// </summary>
        public MultiplayerConfig Multiplayer { get; init; } = new();

        /// <summary>
        /// Returns default response information
        /// </summary>
        /// <returns></returns>
        public static ResponseConfig Default()
        {
            return new ResponseConfig();
        }
    }
}
