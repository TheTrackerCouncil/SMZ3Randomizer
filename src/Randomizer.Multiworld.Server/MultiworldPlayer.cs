using Randomizer.Data.Multiworld;
using Randomizer.Data.Options;

namespace Randomizer.Multiworld.Server;

public class MultiworldPlayer
{
    public string PlayerName { get; set; } = null!;
    public string Guid { get; set; } = null!;
    public string Key { get; set; } = null!;
    public int WorldId { get; set; }
    public Config? Config { get; set; }
    public MultiworldPlayerState? State { get; set; }
}
