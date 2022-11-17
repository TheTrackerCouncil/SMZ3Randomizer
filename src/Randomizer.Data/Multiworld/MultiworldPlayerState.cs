using System.Collections.Generic;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Multiworld;

public class MultiworldPlayerState
{
    public string Guid { get; init; } = null!;
    public string PlayerName { get; init; } = null!;
    public int? WorldId { get; set; }
    public Config? Config { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsConnected { get; set; } = true;
    public IEnumerable<TrackerLocationState>? Locations { get; set; }
    public IEnumerable<TrackerItemState>? Items { get; set; }
    public IEnumerable<TrackerBossState>? Bosses { get; set; }
    public IEnumerable<TrackerDungeonState>? Dungeons { get; set; }
}
