using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Represents the various phrases that Tracker can respond with.
/// </summary>
[Description("Config file for various tracker responses to actions that happen")]
public class ResponseConfig : IMergeable<ResponseConfig>, IConfigFile<ResponseConfig>
{
    /// <summary>
    /// Gets the phrases to respond with when tracker starts.
    /// </summary>
    public SchrodingersString? StartedTracking { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracker starts in "alternate"
    /// mode.
    /// </summary>
    public SchrodingersString? StartingTrackingAlternate { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracker stops.
    /// </summary>
    public SchrodingersString? StoppedTracking { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracker stops after go mode
    /// has been turned on.
    /// </summary>
    public SchrodingersString? StoppedTrackingPostGoMode { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when speech recognition confidence
    /// does not meet the configured threshold for execution, but is high
    /// enough to be recognized.
    /// </summary>
    public SchrodingersString? Misheard { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking a simple single-stage
    /// item.
    /// <c>{0}</c> is a placeholder for the name of the item.
    /// </summary>
    public SchrodingersString? TrackedItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking two items at once.
    /// <c>{0}</c> is a placeholder for the name of the first item.
    /// <c>{1}</c> is a placeholder for the name of the second item.
    /// </summary>
    public SchrodingersString? TrackedTwoItems { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking three items at once.
    /// <c>{0}</c> is a placeholder for the name of the first item.
    /// <c>{1}</c> is a placeholder for the name of the second item.
    /// <c>{2}</c> is a placeholder for the name of the third item.
    /// </summary>
    public SchrodingersString? TrackedThreeItems { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking more than three items
    /// <c>{0}</c> is a placeholder for the name of the first item.
    /// <c>{1}</c> is a placeholder for the name of the second item.
    /// <c>{2}</c> is the number of remaining items tracked.
    /// </summary>
    public SchrodingersString? TrackedManyItems { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking a progressive item
    /// using a specific stage name.
    /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
    /// <c>{1}</c> is a placeholder for the current stage of the progressive
    /// item (e.g. Master Sword).
    /// </summary>
    public SchrodingersString? TrackedItemByStage { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking an item that can be
    /// tracked multiple times.
    /// <c>{0}</c> is a placeholder for the plural item name. <c>{1}</c> is
    /// a placeholder for the number of copies.
    /// </summary>
    public SchrodingersString? TrackedItemMultiple { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking a progressive item.
    /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
    /// <c>{1}</c> is a placeholder for the current stage of the progressive
    /// item (e.g. Master Sword).
    /// </summary>
    public SchrodingersString? TrackedProgressiveItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking a lower tier of an
    /// item than you already have, e.g. tracking Master Sword when you have
    /// the Tempered Sword.
    /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
    /// <c>{1}</c> is a placeholder for the current stage of the progressive
    /// item (e.g. Master Sword).
    /// </summary>
    public SchrodingersString? TrackedOlderProgressiveItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking an item that is
    /// already at the max tier.
    /// </summary>
    public SchrodingersString? TrackedTooManyOfAnItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking a single-stage item
    /// that is already tracked.
    /// <c>{0}</c> is a placeholder for the name of the item.
    /// </summary>
    public SchrodingersString? TrackedAlreadyTrackedItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking an item that is at
    /// its correct location according to the original game.
    /// </summary>
    public SchrodingersString? TrackedVanillaItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when setting the exact amount of an
    /// item, but that exact amount is already tracked.
    /// <c>{0}</c> is the plural name of the item. <c>{1}</c> is the number
    /// of items.
    /// </summary>
    public SchrodingersString? TrackedExactAmountDuplicate { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking multiple items in an
    /// area at once.
    /// </summary>
    public SchrodingersString? TrackedMultipleItems { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing multiple items in an
    /// area at once.
    /// </summary>
    public SchrodingersString? ClearedMultipleItems { get; init; }

    /// <summary>
    /// Get the phrases to respond with when playing a parsed AP/Mainline multiworld game and the player picked up an
    /// item for another player that is known by SMZ3 (another SMZ3, Z3, or SM item).
    /// <c>{0}</c> is a placeholder for the name of the item.
    /// <c>{1}</c> is a placeholder for the name of the item prefixed with the article.
    /// <c>{2}</c> is a placeholder for the parsed name of the player this item is for.
    /// </summary>
    public SchrodingersString? TrackedParsedOtherGameKnownItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when playing a parsed AP/Mainline multiworld game and the player picked up an
    /// item for another player that is unknown by SMZ3 but was labeled as progression.
    /// <c>{0}</c> is a placeholder for the parsed name of the player this item is for.
    /// </summary>
    public SchrodingersString? TrackedParsedOtherGameUnknownProgressionItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when playing a parsed AP/Mainline multiworld game and the player picked up an
    /// item for another player that is unknown by SMZ3 but was labeled as not progression.
    /// <c>{0}</c> is a placeholder for the parsed name of the player this item is for.
    /// </summary>
    public SchrodingersString? TrackedParsedOtherGameUnknownRegularItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking or clearing multiple
    /// items in an area at once, but there no items left.
    /// <c>{0}</c> is a placeholder for the name of the area.
    /// </summary>
    public SchrodingersString? TrackedNothing { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking or clearing multiple
    /// items in an area at once, but the only items left are out of logic.
    /// <c>{0}</c> is a placeholder for the name of the area. <c>{1}</c> is
    /// a placeholder for the number of items that are left but
    /// inaccessible.
    /// </summary>
    public Dictionary<int, SchrodingersString>? TrackedNothingOutOfLogic { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when untracking an item.
    /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
    /// a placeholder for the name of the item with "a"/"the".
    /// </summary>
    public SchrodingersString? UntrackedItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with after tracking an item that is not
    /// in logic.
    /// <c>{0}</c> is a placeholder for the name of the item was tracked.
    /// <c>{1}</c> is a placeholder for the name of the location that
    /// contains the item that is out of logic. <c>{2}</c> is a placeholder
    /// for one or more names of required items that are missing.
    /// </summary>
    public SchrodingersString? TrackedOutOfLogicItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with after tracking an item that is not
    /// in logic.
    /// <c>{0}</c> is a placeholder for the name of the item was tracked.
    /// <c>{1}</c> is a placeholder for the name of the location that
    /// contains the item that is out of logic.
    /// </summary>
    public SchrodingersString? TrackedOutOfLogicItemTooManyMissing { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when untracking an item that hasn't
    /// been tracked yet.
    /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
    /// a placeholder for the name of the item with "a"/"the".
    /// </summary>
    public SchrodingersString? UntrackedNothing { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when untracking an item that has
    /// multiple stages.
    /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
    /// a placeholder for the name of the item with "a"/"the".
    /// </summary>
    public SchrodingersString? UntrackedProgressiveItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when untracking an item that can be
    /// tracked multiple times.
    /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
    /// a placeholder for the name of the item with "a"/"the".
    /// </summary>
    public SchrodingersString? UntrackedItemMultiple { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when untracking the last copy of an
    /// item that can be tracked multiple times.
    /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
    /// a placeholder for the name of the item with "a"/"the".
    /// </summary>
    public SchrodingersString? UntrackedItemMultipleLast { get; init; }

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
    /// Gets the phrases to respond with when pegging a single Peg World peg.
    /// </summary>
    public SchrodingersString? PegWorldModePegged { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when pegging multiple Peg World pegs.
    /// </summary>
    public Dictionary<int, SchrodingersString>? PegWorldModePeggedMultiple { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when all Peg World pegs have been
    /// pegged or Peg World mode is toggled off manually.
    /// </summary>
    public SchrodingersString? PegWorldModeDone { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking the reward at a
    /// dungeon.
    /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
    /// is a placeholder for the name of the reward that was marked.
    /// </summary>
    public SchrodingersString? DungeonRewardMarked { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking the reward for all
    /// other unmarked dungeons.
    /// <c>{0}</c> is a placeholder for the name of the reward that was
    /// marked.
    /// </summary>
    public SchrodingersString? RemainingDungeonsMarked { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when there are no unmarked
    /// dungeons.
    /// </summary>
    public SchrodingersString? NoRemainingDungeons { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing all locations in a
    /// dungeon.
    /// <c>{0}</c> is a placeholder for the name of the dungeon.
    /// </summary>
    public SchrodingersString? DungeonCleared { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing all location in a
    /// dungeon, but all locations are already cleared.
    /// <c>{0}</c> is a placeholder for the name of the dungeon.
    /// </summary>
    public SchrodingersString? DungeonAlreadyCleared { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing all locations in a
    /// dungeon, but some of the cleared locations were out of logic.
    /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
    /// is a placeholder for the name of a location that was missed.
    /// <c>{2}</c> is a placeholder for the items that are required for a
    /// missed location.
    /// </summary>
    public SchrodingersString? DungeonClearedWithInaccessibleItems { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing all locations in a
    /// dungeon, but some of the cleared locations were out of logic with too many missing items.
    /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
    /// is a placeholder for the name of a location that was missed.
    /// </summary>
    public SchrodingersString? DungeonClearedWithTooManyInaccessibleItems { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing a dungeon.
    /// <c>{0}</c> is a placeholder for the name of the dungeon that was
    /// cleared. <c>{1}</c> is a placeholder for the boss of the dungeon.
    /// </summary>
    public SchrodingersString? DungeonBossCleared { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing a dungeon that was
    /// already cleared.
    /// <c>{0}</c> is a placeholder for the name of the dungeon that was
    /// cleared. <c>{1}</c> is a placeholder for the boss of the dungeon.
    /// </summary>
    public SchrodingersString? DungeonBossAlreadyCleared { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when reverting the cleared status
    /// of a dungeon.
    /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
    /// is a placeholder for the boss of the dungeon.
    /// </summary>
    public SchrodingersString? DungeonBossUncleared { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when reverting the cleared status
    /// of a dungeon that wasn't cleared yet.
    /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
    /// is a placeholder for the boss of the dungeon.
    /// </summary>
    public SchrodingersString? DungeonBossNotYetCleared { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the player defeated a boss
    /// with remarkable skill and add content!
    /// </summary>
    public SchrodingersString? DungeonBossClearedAddContent { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking the medallion
    /// requirement of a dungeon.
    /// <c>{0}</c> is a placeholder for the name of the medallion.
    /// <c>{1}</c> is a placeholder for the name of the dungeon.
    /// </summary>
    public SchrodingersString? DungeonRequirementMarked { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking the wrong dungeon with
    /// a required medallion.
    /// <c>{0}</c> is a placeholder for the name of the dungeon.
    /// </summary>
    public SchrodingersString? DungeonRequirementInvalid { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking a dungeon with a
    /// requirement that doesn't match the seed logic.
    /// <c>{0}</c> is a placeholder for the name of the medallion in the
    /// seed. <c>{1}</c> is a placeholder for the name of the dungeon.
    /// <c>{2}</c> is a placeholder for the name of the medallion that was
    /// tracked.
    /// </summary>
    public SchrodingersString? DungeonRequirementMismatch { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking treasure in a
    /// dungeon. The dictionary key represents the amount of items left,
    /// where 2 is 2 or more and -1 is when the dungeon is already empty.
    /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
    /// is the amount of items left after tracking.
    /// </summary>
    public Dictionary<int, SchrodingersString>? DungeonTreasureTracked { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing more treasure chests
    /// in a dungeon than there are left.
    /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
    /// is the actual amount of items left. <c>{2}</c> is the amount of
    /// items requested to clear.
    /// </summary>
    public SchrodingersString? DungeonTooManyTreasuresTracked { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking an item in a specific
    /// dungeon, but that dungeon does not have that item in the seed.
    /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
    /// is a placeholder for the name of the item, including "a" or "the",
    /// as appropriate.
    /// </summary>
    public SchrodingersString? ItemTrackedInIncorrectDungeon { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking an item at a location.
    /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
    /// is a placeholder for the name of the item.
    /// </summary>
    public SchrodingersString? LocationMarked { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking an item at a location
    /// that has already been marked.
    /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
    /// is a placeholder for the name of the item. <c>{2}</c> is a
    /// placeholder for the name of the item that was previously marked
    /// here.
    /// </summary>
    public SchrodingersString? LocationMarkedAgain { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking an item as
    /// nothing/bullshit.
    /// <c>{0}</c> is a placeholder for the name of the location.
    /// </summary>
    public SchrodingersString? LocationMarkedAsBullshit { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when attempting to make an item at a location, but that location has already
    /// been cleared.
    /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
    /// is a placeholder for the name of the item.
    /// </summary>
    public SchrodingersString? LocationMarkedButAlreadyCleared { get; set; }

    /// <summary>
    /// Gets the phrases to respond with when marking an item at a location that was preconfigured.
    /// <c>{0}</c> is a placeholder for the name of the location.
    /// </summary>
    public SchrodingersString? LocationMarkedPreConfigured { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracking an item at a location that was preconfigured.
    /// <c>{0}</c> is a placeholder for the name of the location.
    /// </summary>
    public SchrodingersString? TrackedPreConfigured { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking or tracking an item at
    /// a location, when the seed contains a different item at the same
    /// location.
    /// <c>{0}</c> is a placeholder for the item that was tracked or marked.
    /// <c>{1}</c> is a placeholder for the item that was in the same
    /// location in the seed.
    /// </summary>
    public SchrodingersString? LocationHasDifferentItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing a location.
    /// <c>{0}</c> is a placeholder for the name of the location.
    /// </summary>
    public SchrodingersString? LocationCleared { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing multiple locations.
    /// <c>{0}</c> is a placeholder for the number of locations.
    /// </summary>
    public SchrodingersString? LocationsCleared { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when trying to clear the last marked locations when there aren't any
    /// </summary>
    public SchrodingersString? NoMarkedLocations { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when clearing multiple locations
    /// from the same region.
    /// <c>{0}</c> is a placeholder for the number of locations.
    /// <c>{1}</c> is a placeholder for the name of the region
    /// </summary>
    public SchrodingersString? LocationsClearedSameRegion { get; init; }

    /// <summary>
    /// Gets the phrases to request tracker to initiate go mode
    /// </summary>
    public List<string>? GoModePrompts { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when undoing Go Mode.
    /// </summary>
    public SchrodingersString? GoModeToggledOff { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when attempting to track or clear
    /// an item in an area that does not have that item.
    /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
    /// a placeholder for the name of the location. <c>{2}</c> is a
    /// placeholder for the name of the item, including "a"/"the".
    /// </summary>
    public SchrodingersString? AreaDoesNotHaveItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when attempting to track or clear
    /// an item in an area that has multiple copies of that item.
    /// <c>{0}</c> is a placeholder for the name of the item. <c>{1}</c> is
    /// a placeholder for the name of the location. <c>{2}</c> is a
    /// placeholder for the name of the item, including "a"/"the".
    /// </summary>
    public SchrodingersString? AreaHasMoreThanOneItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with after tracking an item that doesn't
    /// open up new areas.
    /// </summary>
    public SchrodingersString? TrackedUselessItem { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when counting Hyper Beam shots.
    /// </summary>
    public Dictionary<int, SchrodingersString>? CountHyperBeamShots { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when an internal error occurs.
    /// </summary>
    public SchrodingersString? Error { get; init; }

    /// <summary>
    /// Response to say when attempting to manually track something when autotracking is enabled
    /// <c>{0}</c> is a placeholder for the command that the user can say to force tracker to do the action.
    /// </summary>
    public SchrodingersString? AutoTrackingEnabledSass { get; init; }

    /// <summary>
    /// Gets the phrases to respond with before undoing the last action.
    /// </summary>
    public SchrodingersString? ActionUndone { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when there is nothing to undo.
    /// </summary>
    public SchrodingersString? NothingToUndo { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the latest undo action
    /// has expired
    /// </summary>
    public SchrodingersString? UndoExpired { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when changing a Tracker setting.
    /// <c>{0}</c> is a placeholder for the name of the setting that was
    /// changed. <c>{1}</c> is a placeholder for the new value.
    /// </summary>
    public SchrodingersString? TrackerSettingChanged { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking a boss as defeated.
    /// <c>{0}</c> is a placeholder for the name of the boss.
    /// </summary>
    public SchrodingersString? BossDefeated { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when marking a boss as defeated who
    /// was already marked.
    /// <c>{0}</c> is a placeholder for the name of the boss.
    /// </summary>
    public SchrodingersString? BossAlreadyDefeated { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when un-marking a boss as defeated.
    /// <c>{0}</c> is a placeholder for the name of the boss.
    /// </summary>
    public SchrodingersString? BossUndefeated { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when un-marking a boss as defeated
    /// who hasn't been defeated yet.
    /// <c>{0}</c> is a placeholder for the name of the boss.
    /// </summary>
    public SchrodingersString? BossNotYetDefeated { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the timer is resumed
    /// </summary>
    public SchrodingersString? TimerResumed { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the timer is reset
    /// </summary>
    public SchrodingersString? TimerReset { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the timer is paused
    /// </summary>
    public SchrodingersString? TimerPaused { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracker is muted
    /// </summary>
    public SchrodingersString? Muted { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when tracker is unmuted
    /// </summary>
    public SchrodingersString? Unmuted { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when the game is beaten
    /// </summary>
    public SchrodingersString? BeatGame { get; init; }

    /// <summary>
    /// Gets the phrases for sass for when tracker has been talking for over a minute
    /// </summary>
    public SchrodingersString? LongSpeechResponse { get; init; }

    /// <summary>
    /// Gets a dictionary that contains the phrases to respond with when no
    /// voice commands have been issued after a certain period of time, as
    /// expressed in the dictionary keys.
    /// </summary>
    public Dictionary<string, SchrodingersString>? Idle { get; init; }

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

    public static object Example()
    {
        return new Dictionary<string, object>()
        {
            ["StartedTracking"] = new SchrodingersString("Message tracker will state when tracking started", new SchrodingersString.Possibility("Another message tracker will state when tracking started", 0.1)),
            ["TrackedNothingOutOfLogic"] = new Dictionary<int, SchrodingersString>()
            {
                [1] = new("The only item left in the area {0} is out of logic."),
                [2] = new("The only items left in area {0} are out of logic.")
            },
            ["DungeonTreasureTracked"] = new Dictionary<int, SchrodingersString>()
            {
                [2] = new ("Message when there are {1} items left in dungeon {0}, and that number is at least 2"),
                [1] = new ("Message when there is only 1 item left in dungeon {0}"),
                [0] = new ("Message when there are no items left in dungeon {0}"),
                [-1] = new ("Message when trying to track an item in dungeon {0} but all items have already been gotten"),
            },
            ["Idle"] = new Dictionary<string, SchrodingersString>()
            {
                ["5m\u00b12m"] = new("Tracker message when not having tracked anything between 3-7 minutes", new SchrodingersString.Possibility("Another possible tracker message", 0.1)),
            },
            ["Chat"] = new Dictionary<string, object>()
            {
                ["RecognizedGreetings"] = new List<string>()
                {
                    "Possible chat greeting",
                    "Regex.*works.*here",
                    "(tracker|another name for tracker)",
                    "helloEmoteName tracker"
                },
                ["UserNamePronunciation"] = new Dictionary<string, string>()
                {
                    ["TwitchUserName"] = "Corrected pronunciation",
                    ["AnotherTwitchUserName"] = "Another corrected pronunciation",
                }
            }
        };
    }
}
