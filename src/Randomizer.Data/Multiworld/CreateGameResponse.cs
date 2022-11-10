namespace Randomizer.Data.Multiworld;

public class CreateGameResponse : MultiworldResponse
{
    public string? GameGuid { get; set; }
    public string? PlayerGuid { get; set; }
    public string? PlayerKey { get; set; }
}
