using System;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ParsedRom;

public class ParsedRomPrerequisiteDetails
{
    public required Type RegionType { get; set; }
    public required string RegionName { get; set; }
    public required ItemType PrerequisiteItem { get; set; }
}
