namespace TrackerCouncil.Smz3.Data.ParsedRom;

public class ParsedRomPlayer
{
    public required string PlayerName { get; set; }
    public required int PlayerIndex { get; set; }
    public required bool IsLocalPlayer { get; set; }
}
