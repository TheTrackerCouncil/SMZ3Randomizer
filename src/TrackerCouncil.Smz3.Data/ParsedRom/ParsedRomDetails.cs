using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ParsedRom;

public class ParsedRomDetails
{
    public required string OriginalPath { get; set; }
    public required string OriginalFilename { get; set; }
    public required string RomTitle { get; set; }
    public required int Seed { get; set; }
    public required bool IsMultiworld { get; set; }
    public required bool IsHardLogic { get; set; }
    public required KeysanityMode KeysanityMode { get; set; }
    public required RomGenerator RomGenerator { get; set; }
    public required int GanonsTowerCrystalCount { get; set; }
    public required int GanonCrystalCount { get; set; }
    public required int TourianBossCount { get; set; }
    public required List<ParsedRomPlayer> Players { get; set; }
    public required List<ParsedRomLocationDetails> Locations { get; set; }
    public required List<ParsedRomBossDetails> Bosses { get; set; }
    public required List<ParsedRomRewardDetails> Rewards { get; set; }
    public required List<ParsedRomPrerequisiteDetails> Prerequisites { get; set; }
    public required Dictionary<ItemType, int> StartingItems { get; set; }
    public required List<byte[]> ParsedText { get; set; }
}
