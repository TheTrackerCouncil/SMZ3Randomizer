using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions;

/// <summary>
/// Defines a named area that contains one or more locations.
/// </summary>
public interface IHasLocations
{
    /// <summary>
    /// Gets the name of the area.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a collection of alternative names for the area.
    /// </summary>
    IReadOnlyCollection<string> AlsoKnownAs { get; }

    /// <summary>
    /// Gets all locations in the area.
    /// </summary>
    IEnumerable<Location> Locations { get; }

    public IHasTreasure? GetTreasureRegion()
    {
        return Region as IHasTreasure;
    }

    public Region? Region => this switch
    {
        Room room => room.Region,
        Region region => region,
        _ => null
    };

    public bool IsKeysanityForArea
    {
        get
        {
            if (Region is Z3Region && Region.Config.ZeldaKeysanity) return true;
            return Region is SMRegion && Region.Config.MetroidKeysanity;
        }
    }

    public string RandomAreaName => this is Room room ? room.RandomAreaName : Region?.Name ?? Name;
}
