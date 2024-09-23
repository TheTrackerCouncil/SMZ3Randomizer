using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Represents a single spot in the Tracker UI
/// </summary>
public class UIGridLocation
{
    /// <summary>
    /// Constructor
    /// </summary>
    public UIGridLocation() { }

    /// <summary>
    /// The type of object this location represents
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public UIGridLocationType Type { get; set; }

    /// <summary>
    /// The row in the UI where this spot is located
    /// </summary>
    public int Row { get; init; }

    /// <summary>
    /// The column in the UI where this spot is located
    /// </summary>
    public int Column { get; init; }

    /// <summary>
    /// Image to display in this location
    /// </summary>
    public string? Image { get; init; }

    /// <summary>
    /// Collection of object identifiers to look up for this location
    /// </summary>
    public ICollection<string> Identifiers { get; set; } = new List<string>();

    /// <summary>
    /// Map of identifier to images to use
    /// </summary>
    public Dictionary<string, string> ReplacementImages { get; set; } = new();
}
