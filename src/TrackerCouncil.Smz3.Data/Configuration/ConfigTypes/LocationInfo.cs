﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Represents extra information about a trackable location in SMZ3.
/// </summary>
public class LocationInfo : IMergeable<LocationInfo>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public LocationInfo() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationInfo"/> class
    /// with the specified info.
    /// </summary>
    /// <param name="name">The possible names for the location.</param>
    public LocationInfo(SchrodingersString name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationInfo"/> class
    /// with the specified info.
    /// </summary>
    /// <param name="id">
    /// The ID of the location. Must match an existing <see
    /// cref="LocationId"/>.
    /// </param>
    /// <param name="name">The names for the location.</param>
    public LocationInfo(LocationId id, string name)
    {
        LocationNumber = (int)id;
        Name = new SchrodingersString(name);
    }

    /// <summary>
    /// The internal SMZ3 location id for the location. Also used
    /// for matching configs for merging
    /// </summary>
    [MergeKey]
    public int LocationNumber { get; init; }

    /// <summary>
    /// The corresponding LocationId for the location.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public LocationId LocationId { get; init; }

    /// <summary>
    /// Gets the possible names for the location.
    /// </summary>
    public SchrodingersString? Name { get; set; }

    /// <summary>
    /// Gets the possible hints for the location, if any are defined.
    /// </summary>
    public SchrodingersString? Hints { get; set; }

    /// <summary>
    /// Gets the phrases to reply with when tracking a junk item at this location.
    /// </summary>
    public SchrodingersString? WhenTrackingJunk { get; set; }

    /// <summary>
    /// Gets the phrases to reply with when tracking a progression item at this location.
    /// </summary>
    public SchrodingersString? WhenTrackingProgression { get; set; }

    /// <summary>
    /// Gets the phrases to reply with when marking a junk item at this location.
    /// </summary>
    public SchrodingersString? WhenMarkingJunk { get; set; }

    /// <summary>
    /// Gets the phrases to reply with when marking a progression item at this location.
    /// </summary>
    public SchrodingersString? WhenMarkingProgression { get; set; }

    /// <summary>
    /// Gets the phrases to reply with when the location is out of logic
    /// </summary>
    public SchrodingersString? OutOfLogic { get; set; }

    /// <summary>
    /// Returns a string representation of the location.
    /// </summary>
    /// <returns>A string representation of this location.</returns>
    public override string? ToString() => LocationId.ToString();
}
