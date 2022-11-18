using System;
using System.Collections.Generic;
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
        public SchrodingersString? StartedTracking { get; init; }
            = new SchrodingersString("Tracker started");

        /// <summary>
        /// Gets the phrases to respond with when tracker starts in "alternate"
        /// mode.
        /// </summary>
        public SchrodingersString? StartingTrackingAlternate { get; init; }
        = new SchrodingersString("Hello. I'm your substitude tracker for today.");

        /// <summary>
        /// Gets the phrases to respond with when tracker stops.
        /// </summary>
        public SchrodingersString? StoppedTracking { get; init; }
            = new SchrodingersString("Tracker stopped");

        /// <summary>
        /// Gets the phrases to respond with when tracker stops after go mode
        /// has been turned on.
        /// </summary>
        public SchrodingersString? StoppedTrackingPostGoMode { get; init; }
            = new SchrodingersString("Tracker stopped");

        /// <summary>
        /// Gets the phrases to respond with when speech recognition confidence
        /// does not meet the configured threshold for execution, but is high
        /// enough to be recognized.
        /// </summary>
        public SchrodingersString Misheard { get; init; }
            = new SchrodingersString("I didn't quite get that.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a simple single-stage
        /// item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item.
        /// </remarks>
        public SchrodingersString TrackedItem { get; init; }
            = new SchrodingersString("Toggled {0} on.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a progressive item
        /// using a specific stage name.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </remarks>
        public SchrodingersString TrackedItemByStage { get; init; }
            = new SchrodingersString("Marked {0} as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that can be
        /// tracked multiple times.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the plural item name. <c>{1}</c> is
        /// a placeholder for the number of copies.
        /// </remarks>
        public SchrodingersString TrackedItemMultiple { get; init; }
             = new SchrodingersString("Added more {0}.", "You now have {1} {0}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a progressive item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </remarks>
        public SchrodingersString TrackedProgressiveItem { get; init; }
            = new SchrodingersString("Upgraded {0} by one step.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a lower tier of an
        /// item than you already have, e.g. tracking Master Sword when you have
        /// the Tempered Sword.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </remarks>
        public SchrodingersString? TrackedOlderProgressiveItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that is
        /// already at the max tier.
        /// </summary>
        public SchrodingersString? TrackedTooManyOfAnItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking a single-stage item
        /// that is already tracked.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item.
        /// </remarks>
        public SchrodingersString? TrackedAlreadyTrackedItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that is at
        /// its correct location according to the original game.
        /// </summary>
        public SchrodingersString TrackedVanillaItem { get; init; }
            = new("");

        /// <summary>
        /// Gets the phrases to respond with when setting the exact amount of an
        /// item, but that exact amount is already tracked.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is the plural name of the item. <c>{1}</c> is the number
        /// of items.
        /// </remarks>
        public SchrodingersString TrackedExactAmountDuplicate { get; init; }
            = new("You already have {1} {0}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking multiple items in an
        /// area at once.
        /// </summary>
        public SchrodingersString TrackedMultipleItems { get; init; }
            = new SchrodingersString("Tracked {2} in {1}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing multiple items in an
        /// area at once.
        /// </summary>
        public SchrodingersString ClearedMultipleItems { get; init; }
            = new SchrodingersString("Cleared {0} items in {1}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking or clearing multiple
        /// items in an area at once, but there no items left.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the area.
        /// </remarks>
        public SchrodingersString TrackedNothing { get; init; }
            = new SchrodingersString("There are no items left in {0}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking or clearing multiple
        /// items in an area at once, but the only items left are out of logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the area. <c>{1}</c> is
        /// a placeholder for the number of items that are left but
        /// inaccessible.
        /// </remarks>
        public Dictionary<int, SchrodingersString> TrackedNothingOutOfLogic { get; init; } = new Dictionary<int, SchrodingersString>
        {
            [1] = new SchrodingersString("The only item left in {0} is out of logic."),
            [2] = new SchrodingersString("The only items left in {0} are out of logic.")
        };

        /// <summary>
        /// Gets the phrases to respond with when untracking an item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </remarks>
        public SchrodingersString UntrackedItem { get; init; }
            = new SchrodingersString("Toggled {0} off.");

        /// <summary>
        /// Gets the phrases to respond with after tracking an item that is not
        /// in logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item was tracked.
        /// <c>{1}</c> is a placeholder for the name of the location that
        /// contains the item that is out of logic. <c>{2}</c> is a placeholder
        /// for one or more names of required items that are missing.
        /// </remarks>
        public SchrodingersString? TrackedOutOfLogicItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with after tracking an item that is not
        /// in logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item was tracked.
        /// <c>{1}</c> is a placeholder for the name of the location that
        /// contains the item that is out of logic.
        /// </remarks>
        public SchrodingersString? TrackedOutOfLogicItemTooManyMissing { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when untracking an item that hasn't
        /// been tracked yet.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </remarks>
        public SchrodingersString UntrackedNothing { get; init; }
            = new SchrodingersString("There's nothing to remove.");

        /// <summary>
        /// Gets the phrases to respond with when untracking an item that has
        /// multiple stages.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </remarks>
        public SchrodingersString UntrackedProgressiveItem { get; init; }
            = new SchrodingersString("Decreased {0} by one step.");

        /// <summary>
        /// Gets the phrases to respond with when untracking an item that can be
        /// tracked multiple times.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </remarks>
        public SchrodingersString UntrackedItemMultiple { get; init; }
            = new SchrodingersString("Removed {1}.");

        /// <summary>
        /// Gets the phrases to respond with when untracking the last copy of an
        /// item that can be tracked multiple times.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the item with "a"/"the".
        /// </remarks>
        public SchrodingersString UntrackedItemMultipleLast { get; init; }
            = new SchrodingersString("Removed your last {0}.");

        /// <summary>
        /// Gets the phrases to respond with when Shaktool becomes available.
        /// </summary>
        public SchrodingersString? ShaktoolAvailable { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when Peg World becomes available.
        /// </summary>
        public SchrodingersString? PegWorldAvailable { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when Peg World mode is toggled on.
        /// </summary>
        public SchrodingersString? PegWorldModeOn { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when pegging a Peg World peg.
        /// </summary>
        public SchrodingersString? PegWorldModePegged { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when all Peg World pegs have been
        /// pegged or Peg World mode is toggled off manually.
        /// </summary>
        public SchrodingersString? PegWorldModeDone { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when marking the reward at a
        /// dungeon.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of the reward that was marked.
        /// </remarks>
        public SchrodingersString DungeonRewardMarked { get; init; }
            = new SchrodingersString("Marked {0} as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when marking the reward for all
        /// other unmarked dungeons.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the reward that was
        /// marked.
        /// </remarks>
        public SchrodingersString RemainingDungeonsMarked { get; init; }
            = new SchrodingersString("Marked remaining dungeons as {0}.");

        /// <summary>
        /// Gets the phrases to respond with when there are no unmarked
        /// dungeons.
        /// </summary>
        public SchrodingersString NoRemainingDungeons { get; init; }
            = new SchrodingersString("You already marked every dungeon.");

        /// <summary>
        /// Gets the phrases to respond with when clearing all locations in a
        /// dungeon.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonCleared { get; init; }
             = new("Cleared everything in {0}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing all location in a
        /// dungeon, but all locations are already cleared.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonAlreadyCleared { get; init; }
            = new("But you already got everything in {0}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing all locations in a
        /// dungeon, but some of the cleared locations were out of logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of a location that was missed.
        /// <c>{2}</c> is a placeholder for the items that are required for a
        /// missed location.
        /// </remarks>
        public SchrodingersString DungeonClearedWithInaccessibleItems { get; init; }
            = new("Including some out of logic checks that require {2}, such as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing all locations in a
        /// dungeon, but some of the cleared locations were out of logic with too many missing items.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of a location that was missed.
        /// </remarks>
        public SchrodingersString DungeonClearedWithTooManyInaccessibleItems { get; init; }
            = new("Are you sure you got everything?");

        /// <summary>
        /// Gets the phrases to respond with when clearing a dungeon.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon that was
        /// cleared. <c>{1}</c> is a placeholder for the boss of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonBossCleared { get; init; }
            = new SchrodingersString("Cleared {0}.", "Marked {1} as defeated.");

        /// <summary>
        /// Gets the phrases to respond with when clearing a dungeon that was
        /// already cleared.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon that was
        /// cleared. <c>{1}</c> is a placeholder for the boss of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonBossAlreadyCleared { get; init; }
            = new SchrodingersString("You already cleared {0}.", "You already defeated {1}.");

        /// <summary>
        /// Gets the phrases to respond with when reverting the cleared status
        /// of a dungeon.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the boss of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonBossUncleared { get; init; }
            = new SchrodingersString("Reset {0}.", "Marked {1} as still alive.");

        /// <summary>
        /// Gets the phrases to respond with when reverting the cleared status
        /// of a dungeon that wasn't cleared yet.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the boss of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonBossNotYetCleared { get; init; }
            = new SchrodingersString("You haven't cleared {0} yet.", "You never defeated {1} in the first place.");

        /// <summary>
        /// Gets the phrases to respond with when the player defeated a boss
        /// with remarkable skill and add content!
        /// </summary>
        public SchrodingersString DungeonBossClearedAddContent { get; init; }
            = new SchrodingersString("That was some impressive work.");

        /// <summary>
        /// Gets the phrases to respond with when marking the medallion
        /// requirement of a dungeon.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the medallion.
        /// <c>{1}</c> is a placeholder for the name of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonRequirementMarked { get; init; }
            = new SchrodingersString("Marked {0} as required for {1}");

        /// <summary>
        /// Gets the phrases to respond with when marking the wrong dungeon with
        /// a required medallion.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonRequirementInvalid { get; init; }
            = new SchrodingersString("{0} doesn't need medallions");

        /// <summary>
        /// Gets the phrases to respond with when marking a dungeon with a
        /// requirement that doesn't match the seed logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the medallion in the
        /// seed. <c>{1}</c> is a placeholder for the name of the dungeon.
        /// <c>{2}</c> is a placeholder for the name of the medallion that was
        /// tracked.
        /// </remarks>
        public SchrodingersString? DungeonRequirementMismatch { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking treasure in a
        /// dungeon. The dictionary key represents the amount of items left,
        /// where 2 is 2 or more and -1 is when the dungeon is already empty.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is the amount of items left after tracking.
        /// </remarks>
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
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is the actual amount of items left. <c>{2}</c> is the amount of
        /// items requested to clear.
        /// </remarks>
        public SchrodingersString? DungeonTooManyTreasuresTracked { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item in a specific
        /// dungeon, but that dungeon does not have that item in the seed.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of the item, including "a" or "the",
        /// as appropriate.
        /// </remarks>
        public SchrodingersString? ItemTrackedInIncorrectDungeon { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when marking an item at a location.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
        /// is a placeholder for the name of the item.
        /// </remarks>
        public SchrodingersString LocationMarked { get; init; }
            = new SchrodingersString("Marked {1} at {0}.");

        /// <summary>
        /// Gets the phrases to respond with when marking an item at a location
        /// that has already been marked.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
        /// is a placeholder for the name of the item. <c>{2}</c> is a
        /// placeholder for the name of the item that was previously marked
        /// here.
        /// </remarks>
        public SchrodingersString LocationMarkedAgain { get; init; }
            = new SchrodingersString("Replaced {2} with {1} at {0}.");

        /// <summary>
        /// Gets the phrases to respond with when marking an item as
        /// nothing/bullshit.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the location.
        /// </remarks>
        public SchrodingersString LocationMarkedAsBullshit { get; init; }
            = new SchrodingersString("Cleared {0}.");

        /// <summary>
        /// Gets the phrases to respond with when marking or tracking an item at
        /// a location, when the seed contains a different item at the same
        /// location.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the item that was tracked or marked.
        /// <c>{1}</c> is a placeholder for the item that was in the same
        /// location in the seed.
        /// </remarks>
        public SchrodingersString? LocationHasDifferentItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when clearing a location.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the location.
        /// </remarks>
        public SchrodingersString LocationCleared { get; init; }
            = new SchrodingersString("Cleared {0}.");

        /// <summary>
        /// Gets the phrases to respond with when undoing Go Mode.
        /// </summary>
        public SchrodingersString? GoModeToggledOff { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when attempting to track or clear
        /// an item in an area that does not have that item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the location. <c>{2}</c> is a
        /// placeholder for the name of the item, including "a"/"the".
        /// </remarks>
        public SchrodingersString? AreaDoesNotHaveItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when attempting to track or clear
        /// an item in an area that has multiple copies of that item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
        /// a placeholder for the name of the location. <c>{2}</c> is a
        /// placeholder for the name of the item, including "a"/"the".
        /// </remarks>
        public SchrodingersString? AreaHasMoreThanOneItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with after tracking an item that doesn't
        /// open up new areas.
        /// </summary>
        public SchrodingersString? TrackedUselessItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when an internal error occurs.
        /// </summary>
        public SchrodingersString Error { get; init; }
             = new SchrodingersString("Oops. Something went wrong.");

        /// <summary>
        /// Gets the phrases to respond with before undoing the last action.
        /// </summary>
        public SchrodingersString? ActionUndone { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when there is nothing to undo.
        /// </summary>
        public SchrodingersString NothingToUndo { get; init; }
            = new SchrodingersString("There's nothing to undo.");

        /// <summary>
        /// Gets the phrases to respond with when the latest undo action
        /// has expired
        /// </summary>
        public SchrodingersString UndoExpired { get; init; }
            = new ("There's nothing recent to undo.");

        /// <summary>
        /// Gets the phrases to respond with when changing a Tracker setting.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the setting that was
        /// changed. <c>{1}</c> is a placeholder for the new value.
        /// </remarks>
        public SchrodingersString TrackerSettingChanged { get; init; }
            = new SchrodingersString("Changed {0} to {1}.");

        /// <summary>
        /// Gets the phrases to respond with when marking a boss as defeated.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the boss.
        /// </remarks>
        public SchrodingersString? BossDefeated { get; init; }
            = new SchrodingersString("Congratulations on defeating {0}.");

        /// <summary>
        /// Gets the phrases to respond with when marking a boss as defeated who
        /// was already marked.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the boss.
        /// </remarks>
        public SchrodingersString? BossAlreadyDefeated { get; init; }
            = new SchrodingersString("But you already marked {0} as defeated.");

        /// <summary>
        /// Gets the phrases to respond with when un-marking a boss as defeated.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the boss.
        /// </remarks>
        public SchrodingersString? BossUndefeated { get; init; }
            = new SchrodingersString("Marking {0} as alive.");

        /// <summary>
        /// Gets the phrases to respond with when un-marking a boss as defeated
        /// who hasn't been defeated yet.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the boss.
        /// </remarks>
        public SchrodingersString? BossNotYetDefeated { get; init; }
            = new SchrodingersString("But you haven't defeated {0} yet.");

        /// <summary>
        /// Gets the phrases to respond with when the timer is resumed
        /// </summary>
        public SchrodingersString TimerResumed { get; init; }
             = new SchrodingersString("Timer resumed.");

        /// <summary>
        /// Gets the phrases to respond with when the timer is reset
        /// </summary>
        public SchrodingersString TimerReset { get; init; }
             = new SchrodingersString("Timer reset");

        /// <summary>
        /// Gets the phrases to respond with when the timer is paused
        /// </summary>
        public SchrodingersString TimerPaused { get; init; }
             = new SchrodingersString("Timer paused");

        /// <summary>
        /// Gets the phrases to respond with when tracker is muted
        /// </summary>
        public SchrodingersString Muted { get; init; }
            = new SchrodingersString("Muting, say \"Hey Tracker, Unmute Yourself\" when you want me to talk again.");

        /// <summary>
        /// Gets the phrases to respond with when tracker is unmuted
        /// </summary>
        public SchrodingersString Unmuted { get; init; }
            = new SchrodingersString("Unmuted");

        /// <summary>
        /// Gets the phrases to respond with when the game is beaten
        /// </summary>
        public SchrodingersString BeatGame { get; init; }
            = new SchrodingersString("Good job.");

        /// <summary>
        /// Gets a dictionary that contains the phrases to respond with when no
        /// voice commands have been issued after a certain period of time, as
        /// expressed in the dictionary keys.
        /// </summary>
        public Dictionary<string, SchrodingersString> Idle { get; init; } = new();

        /// <summary>
        /// Get the phrases to respond with when asking tracker about her mood.
        /// </summary>
        public Dictionary<string, SchrodingersString> Moods { get; init; } = new()
        {
            ["non-committal"] = new("Can it wait for a bit? I'm in the middle of some calibrations.")
        };

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
        /// Returns default response information
        /// </summary>
        /// <returns></returns>
        public static ResponseConfig Default()
        {
            return new ResponseConfig();
        }
    }
}
