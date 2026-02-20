using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerSpoilerService
{
    /// <summary>
    /// Occurs when hints are enabled or disabled
    /// </summary>
    public event EventHandler<TrackerEventArgs>? HintsToggled;

    /// <summary>
    /// Occurs when spoilers are enabled or disabled
    /// </summary>
    public event EventHandler<TrackerEventArgs>? SpoilersToggled;

    /// <summary>
    /// Toggles hint status
    /// </summary>
    /// <param name="newStatus">Set to true to enable, set to false to disable, set to null to toggle on/off</param>
    /// <param name="confidence">Speech recognition confidence</param>
    public void ToggleHints(bool? newStatus = null, float? confidence = null);

    /// <summary>
    /// Toggles spoiler status
    /// </summary>
    /// <param name="newStatus">Set to true to enable, set to false to disable, set to null to toggle on/off</param>
    /// <param name="confidence">Speech recognition confidence</param>
    public void ToggleSpoilers(bool? newStatus = null, float? confidence = null);

    /// <summary>
    /// Gives a hint about where to go next.
    /// </summary>
    public void GiveProgressionHint();

    /// <summary>
    /// Gives a hint or spoiler about useful items in an area.
    /// </summary>
    /// <param name="area">The area to give hints about.</param>
    public void GiveAreaHint(IHasLocations area, bool ignoreHintsIfSpoilersEnabled = false);

    /// <summary>
    /// Returns hint or spoiler text about the given area.
    /// </summary>
    /// <param name="area">The area to ask about</param>
    /// <param name="ignoreHintsIfSpoilersEnabled"></param>
    /// <returns>The string to display or say</returns>
    public TrackerResponseDetails GetAreaHintResponse(IHasLocations area, bool ignoreHintsIfSpoilersEnabled = false);

    /// <summary>
    /// Gives a hint or spoiler about the location of an item.
    /// </summary>
    /// <param name="item">The item to find.</param>
    public void RevealItemLocation(Item item, bool ignoreHintsIfSpoilersEnabled = false);

    /// <summary>
    /// Returns hint text for displaying of the location of an item
    /// </summary>
    /// <param name="item">The item to find</param>
    /// <param name="ignoreHintsIfSpoilersEnabled"></param>
    /// <returns>The string to display or say</returns>
    public TrackerResponseDetails GetItemLocationHintResponse(Item item, bool ignoreHintsIfSpoilersEnabled = false);

    /// <summary>
    /// Gives a hint or spoiler about the given location.
    /// </summary>
    /// <param name="location">The location to ask about.</param>
    public void RevealLocationItem(Location location, bool ignoreHintsIfSpoilersEnabled = false);

    /// <summary>
    /// Returns hint or spoiler text about the given location.
    /// </summary>
    /// <param name="location">The location to ask about</param>
    /// <param name="ignoreHintsIfSpoilersEnabled"></param>
    /// <returns>The string to display or say</returns>
    public TrackerResponseDetails GetLocationItemHintResponse(Location location, bool ignoreHintsIfSpoilersEnabled = false);

}
