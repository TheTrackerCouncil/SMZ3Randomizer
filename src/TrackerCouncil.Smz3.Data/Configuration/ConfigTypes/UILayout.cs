﻿using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Class that represents a layout of the tracker UI
/// </summary>
public class UILayout
{
    /// <summary>
    /// Constructor
    /// </summary>
    public UILayout() { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The name of this Tracker UI layout</param>
    /// <param name="gridLocations">A collection of all of the objects shown in the tracker UI</param>
    public UILayout(string name, ICollection<UIGridLocation> gridLocations)
    {
        Name = name;
        GridLocations = gridLocations;
    }

    /// <summary>
    /// The name of the layout
    /// </summary>
    [MergeKey]
    public string Name { get; init; } = "";

    /// <summary>
    /// A collection of all objects to be shown in the tracker UI
    /// </summary>
    public ICollection<UIGridLocation> GridLocations { get; set; } = new List<UIGridLocation>();

}
