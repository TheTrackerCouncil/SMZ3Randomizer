namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Represents a Super Metroid keysanity door
/// </summary>
public class TrackerMapSMDoor
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="item">Item identifier for the key to unlock the door</param>
    /// <param name="x">X position on the map</param>
    /// <param name="y">Y position on the map</param>
    /// <param name="skippableOnFastTourian">If the Crateria boss keycard is needed to get to Tourian</param>
    public TrackerMapSMDoor(string item, int x, int y, bool skippableOnFastTourian)
    {
        Item = item;
        X = x;
        Y = y;
        SkippableOnFastTourian = skippableOnFastTourian;
    }

    /// <summary>
    /// Item identifier for the key to unlock the door
    /// </summary>
    public string Item { get; set; }

    /// <summary>
    /// X position on the map
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y position on the map
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// If the Crateria boss keycard is needed to get to Tourian
    /// </summary>
    public bool SkippableOnFastTourian { get; set; }
}
