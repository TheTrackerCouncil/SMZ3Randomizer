using System.Collections.Generic;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerPlayerState
{
    public string Guid { get; init; } = null!;
    public string PlayerName { get; init; } = null!;
    public int? WorldId { get; set; }
    public string? Config { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsConnected { get; set; } = true;
    public bool HasForfeited { get; set; }
    public bool HasCompleted { get; set; }
    public MultiplayerPlayerStatus Status { get; set; }
    public Dictionary<int, bool>? Locations { get; set; }
    public Dictionary<ItemType, int>? Items { get; set; }
    public Dictionary<BossType, bool>? Bosses { get; set; }
    public Dictionary<string, bool>? Dungeons { get; set; }
    public string? AdditionalData { get; set; }

    //public List<TrackerLocationState>? Locations { get; set; }
    //public List<TrackerItemState>? Items { get; set; }
    //public List<TrackerBossState>? Bosses { get; set; }
    //public List<TrackerDungeonState>? Dungeons { get; set; }
}
