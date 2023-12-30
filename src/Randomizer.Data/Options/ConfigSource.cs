namespace Randomizer.Data.Options;

public class ConfigSource
{
    public required string Owner { get; set; }
    public required string Repo { get; set; }
    public string PriorVersion { get; set; } = "";
}
