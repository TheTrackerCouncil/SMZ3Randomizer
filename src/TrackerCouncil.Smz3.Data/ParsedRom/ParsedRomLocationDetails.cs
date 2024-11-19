using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ParsedRom;

public class ParsedRomLocationDetails
{
    public required LocationId Location { get; set; }
    public required bool IsLocalPlayerItem { get; set; }
    public required string PlayerName{ get; set; }
    public required ItemType? ItemType { get; set; }
    public required bool IsProgression { get; set; }
    public required string ItemName { get; set; }
}
