namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Provides the phrases for hints.
/// </summary>
public class HintsConfig : IMergeable<HintsConfig>
{
    /// <summary>
    /// Gets the phrases to respond with when hints are turned on.
    /// </summary>
    public SchrodingersString? EnabledHints { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when hints are turned off.
    /// </summary>
    public SchrodingersString? DisabledHints { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when asked about an item and hints
    /// are turned off.
    /// </summary>
    public SchrodingersString? PromptEnableItemHints { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when asked about a location and
    /// hints are turned off.
    /// </summary>
    public SchrodingersString? PromptEnableLocationHints { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when there are no applicable hints
    /// for the item or location that was asked about.
    /// </summary>
    public SchrodingersString? NoApplicableHints { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when asked about hints on a seed
    /// where a complete playthrough is likely impossible.
    /// </summary>
    public SchrodingersString? PlaythroughImpossible { get; init; }

    /// <summary>
    /// Gets the hints for items that are not in logic.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemNotInLogic { get; init; }

    /// <summary>
    /// Gets the hints for items that are only found in Super Metroid.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInSuperMetroid { get; init; }

    /// <summary>
    /// Gets the hints for items that are only found in A Link to the Past.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInALttP { get; init; }

    /// <summary>
    /// Gets the hints for items that are not in logic and mention one or
    /// more missing items.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the". <c>{1}</c> is a placeholder for the name(s) of the missing
    /// item(s).
    /// </summary>
    public SchrodingersString? ItemRequiresOtherItem { get; init; }

    /// <summary>
    /// Gets the hints for items that are not in logic and require too many
    /// items to mention.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemRequiresManyOtherItems { get; init; }

    /// <summary>
    /// Gets the hints for items that are not found in the specified region
    /// or area.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the". <c>{1}</c> is a placeholder for the name of the
    /// region/area.
    /// </summary>
    public SchrodingersString? ItemNotInArea { get; init; }

    /// <summary>
    /// Gets the hints for items that are in playthrough sphere zero.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInSphereZero { get; init; }

    /// <summary>
    /// Gets the hints for items that are in an early sphere.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInEarlySphere { get; init; }

    /// <summary>
    /// Gets the hints for items that are in a late sphere.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInLateSphere { get; init; }

    /// <summary>
    /// Gets the hints for items that are in a dungeon that has been visited
    /// already.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInPreviouslyVisitedDungeon { get; init; }

    /// <summary>
    /// Gets the hints for items that are in a dungeon that has not yet been
    /// visited before.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInUnvisitedDungeon { get; init; }

    /// <summary>
    /// Gets the hints for items that are in a region that has been visited
    /// already.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInPreviouslyVisitedRegion { get; init; }

    /// <summary>
    /// Gets the hints for items that are in a region that has not been
    /// visited yet.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the".
    /// </summary>
    public SchrodingersString? ItemInUnvisitedRegion { get; init; }

    /// <summary>
    /// Gets the hints for items that are in locations that have a bad name
    /// in the original randomizer.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the". <c>{1}</c> is a placeholder for the name of the location
    /// in the original randomizer code.
    /// </summary>
    public SchrodingersString? ItemHasBadVanillaLocationName { get; init; }

    /// <summary>
    /// Gets the hints for items that are in locations which have junk items
    /// in the vanilla game.
    /// <c>{0}</c> is a placeholder for the name of the item, with "a", "an"
    /// or "the". <c>{1}</c> is a placeholder for the name of the vanilla
    /// item in the same location.
    /// </summary>
    public SchrodingersString? ItemIsInVanillaJunkLocation { get; init; }

    /// <summary>
    /// Gets the hints for locations that have been cleared already.
    /// <c>{0}</c> is placeholder for the name of the location.
    /// </summary>
    public SchrodingersString? LocationAlreadyCleared { get; init; }

    /// <summary>
    /// Gets the hints for locations that have been cleared already.
    /// <c>{0}</c> is placeholder for the name of the location. <c>{1}</c>
    /// is a placeholder for the name of the item at the location.
    /// </summary>
    public SchrodingersString? LocationAlreadyClearedSpoiler { get; init; }

    /// <summary>
    /// Gets the hints for locations that don't have anything useful.
    /// <c>{0}</c> is placeholder for the name of the location.
    /// </summary>
    public SchrodingersString? LocationHasJunkItem { get; init; }

    /// <summary>
    /// Gets the hints for locations that might have something useful.
    /// <c>{0}</c> is placeholder for the name of the location.
    /// </summary>
    public SchrodingersString? LocationHasUsefulItem { get; init; }

    /// <summary>
    /// Gets the hints for locations that have an item from Super Metroid.
    /// <c>{0}</c> is placeholder for the name of the location. <c>{1}</c>
    /// is a placeholder for the name of the sprite that replaces Samus, or
    /// "Samus".
    /// </summary>
    public SchrodingersString? LocationHasSuperMetroidItem { get; init; }

    /// <summary>
    /// Gets the hints for locations that have an item from A Link to the
    /// Past.
    /// <c>{0}</c> is placeholder for the name of the location. <c>{1}</c>
    /// is a placeholder for the name of the sprite that replaces Link, or
    /// "Link".
    /// </summary>
    public SchrodingersString? LocationHasZeldaItem { get; init; }

    /// <summary>
    /// Gets the suggestions for areas to visit.
    /// <c>{0}</c> is a placeholder for the name of the area that has a
    /// progression item.
    /// </summary>
    public SchrodingersString? AreaSuggestion { get; init; }

    /// <summary>
    /// Gets the hint that describes an item by the text that would be shown
    /// when using the Book of Mudora.
    /// <c>{0}</c> is a placeholder for the name of the location. <c>{1}</c>
    /// is a placeholder for the text that would be displayed when using the
    /// Book of Mudora. <c>{2}</c> is the name of the Book of Mudora,
    /// including "the".
    /// </summary>
    public SchrodingersString? BookHint { get; init; }

    /// <summary>
    /// Gets the phrases to respond with when asking for hints about an area
    /// that was already cleared.
    /// <c>{0}</c> is a placeholder for the name of the area.
    /// </summary>
    public SchrodingersString? AreaAlreadyCleared { get; init; }

    /// <summary>
    /// Gets the hint to give for an area that has one or more useful
    /// items.
    /// <c>{0}</c> is a placeholder for the name of the area.
    /// </summary>
    public SchrodingersString? AreaHasSomethingMandatory { get; init; }

    /// <summary>
    /// Gets the hint to give for an area that might have one or more useful
    /// items.
    /// <c>{0}</c> is a placeholder for the name of the area.
    /// </summary>
    public SchrodingersString? AreaHasSomethingGood { get; init; }

    /// <summary>
    /// Gets the hint to give for an area that only has junk items left.
    /// <c>{0}</c> is a placeholder for the name of the area.
    /// </summary>
    public SchrodingersString? AreaHasJunk { get; init; }

    /// <summary>
    /// Gets the hint to give for an area that only has junk items left, but
    /// also has a crystal as reward for beating the boss.
    /// <c>{0}</c> is a placeholder for the name of the area.
    /// </summary>
    public SchrodingersString? AreaHasJunkAndCrystal { get; init; }

    /// <summary>
    /// Gets the hint to give for an area whose worth is complicated, e.g.
    /// when a dungeon has only junk and is not a crystal dungeon, but the
    /// pendant might result in something good.
    /// <c>{0}</c> is a placeholder for the name of the area.
    /// </summary>
    public SchrodingersString? AreaWorthComplicated { get; init; }

    /// <summary>
    /// Gets the hint to give for an item that is in another world,
    /// but does not mention the specific player
    /// <c>{0}</c> is a placeholder for the name of the item.
    /// </summary>
    public SchrodingersString? ItemInUnknownWorld { get; init; }

    /// <summary>
    /// Gets the hint to give for an item that is in another world,
    /// mentioning the player name
    /// <c>{0}</c> is a placeholder for the name of the item.
    /// <c>{1}</c> is a placeholder for the name of the player.
    /// </summary>
    public SchrodingersString? ItemInPlayerWorld { get; init; }

    /// <summary>
    /// Gets the hint to give for an item that is in another
    /// player's ALttP world
    /// <c>{0}</c> is a placeholder for the name of the item.
    /// <c>{1}</c> is a placeholder for the name of the player.
    /// </summary>
    public SchrodingersString? ItemInPlayerWorldALttP { get; init; }

    /// <summary>
    /// Gets the hint to give for an item that is in another
    /// player's SM world
    /// <c>{0}</c> is a placeholder for the name of the item.
    /// <c>{1}</c> is a placeholder for the name of the player.
    /// </summary>
    public SchrodingersString? ItemInPlayerWorldSuperMetroid { get; init; }

    /// <summary>
    /// When giving a hint for a specific region or room, this states which player
    /// the hint belongs to
    /// <c>{0}</c> is a placeholder for the name of the player.
    /// </summary>
    public SchrodingersString? ItemInPlayerWorldRegionRoomPrefixHint { get; init; }
}
