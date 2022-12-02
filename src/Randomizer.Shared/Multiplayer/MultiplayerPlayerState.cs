using System.Collections.Generic;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerPlayerState
{
    public string Guid { get; init; } = null!;
    public string PlayerName { get; init; } = null!;
    public string PhoneticName { get; init; } = null!;
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

    /// <summary>
    /// Marks a location as accessed
    /// </summary>
    /// <param name="locationId"></param>
    public void TrackLocation(int locationId)
    {
        if (Locations == null) return;
        Locations[locationId] = true;
    }

    /// <summary>
    /// Updates the amount of an item that have been retrieved
    /// </summary>
    /// <param name="type"></param>
    /// <param name="trackedValue"></param>
    public void TrackItem(ItemType type, int trackedValue)
    {
        if (Items == null) return;
        Items[type] = trackedValue;
    }

    /// <summary>
    /// Marks a boss as defeated
    /// </summary>
    /// <param name="type"></param>
    public void TrackBoss(BossType type)
    {
        if (Bosses == null) return;
        Bosses[type] = true;
    }

    /// <summary>
    /// Marks a dungeon as completed
    /// </summary>
    /// <param name="name"></param>
    public void TrackDungeon(string name)
    {
        if (Dungeons == null) return;
        Dungeons[name] = true;
    }
}
