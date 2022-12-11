using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerPlayerState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [JsonIgnore]
    public long Id { get; set; }
    [JsonIgnore] public long GameId { get; set; }
    [JsonIgnore] public virtual MultiplayerGameState Game { get; init; } = null!;
    public string Guid { get; init; } = null!;
    [JsonIgnore] public string Key { get; init; } = "";
    public string PlayerName { get; init; } = null!;
    public string PhoneticName { get; init; } = null!;
    public int? WorldId { get; set; }
    public string? Config { get; set; }
    public bool IsAdmin { get; set; }
    [NotMapped] public bool IsConnected { get; set; }
    [NotMapped] public bool HasForfeited => Status == MultiplayerPlayerStatus.Forfeit;
    [NotMapped] public bool HasCompleted => Status == MultiplayerPlayerStatus.Completed;
    public MultiplayerPlayerStatus Status { get; set; }
    public ICollection<MultiplayerLocationState>? Locations { get; set; }
    public ICollection<MultiplayerItemState>? Items { get; set; }
    public ICollection<MultiplayerBossState>? Bosses { get; set; }
    public ICollection<MultiplayerDungeonState>? Dungeons { get; set; }
    public string? AdditionalData { get; set; }
    [JsonIgnore] public string? GenerationData { get; set; }

    public MultiplayerLocationState? GetLocation(int id) => Locations?.FirstOrDefault(x => x.LocationId == id);
    public MultiplayerItemState? GetItem(ItemType type) => Items?.FirstOrDefault(x => x.Item == type);
    public MultiplayerBossState? GetBoss(BossType type) => Bosses?.FirstOrDefault(x => x.Boss == type);
    public MultiplayerDungeonState? GetDungeon(string name) => Dungeons?.FirstOrDefault(x => x.Dungeon == name);


    /// <summary>
    /// Marks a location as accessed
    /// </summary>
    /// <param name="locationId"></param>
    public MultiplayerLocationState? TrackLocation(int locationId)
    {
        var location = GetLocation(locationId);
        if (location != null)
        {
            location.Tracked = true;
            location.TrackedTime = DateTimeOffset.Now;
        }
        return location;
    }

    /// <summary>
    /// Updates the amount of an item that have been retrieved
    /// </summary>
    /// <param name="type"></param>
    /// <param name="trackedValue"></param>
    public MultiplayerItemState? TrackItem(ItemType type, int trackedValue)
    {
        var item = GetItem(type);
        if (item != null)
        {
            item.TrackingValue = trackedValue;
            item.TrackedTime = DateTimeOffset.Now;
        }
        return item;
    }

    /// <summary>
    /// Marks a boss as defeated
    /// </summary>
    /// <param name="type"></param>
    public MultiplayerBossState? TrackBoss(BossType type)
    {
        var boss = GetBoss(type);
        if (boss != null)
        {
            boss.Tracked = true;
            boss.TrackedTime = DateTimeOffset.Now;
        }
        return boss;
    }

    /// <summary>
    /// Marks a dungeon as completed
    /// </summary>
    /// <param name="name"></param>
    public MultiplayerDungeonState? TrackDungeon(string name)
    {
        var dungeon = GetDungeon(name);
        if (dungeon != null)
        {
            dungeon.Tracked = true;
            dungeon.TrackedTime = DateTimeOffset.Now;
        }
        return dungeon;
    }

    public PlayerWorldUpdates SyncPlayerWorld(MultiplayerWorldState world)
    {
        if (Locations == null) Locations = new List<MultiplayerLocationState>();
        if (Items == null) Items = new List<MultiplayerItemState>();
        if (Dungeons == null) Dungeons = new List<MultiplayerDungeonState>();
        if (Bosses == null) Bosses = new List<MultiplayerBossState>();
        var worldUpdate = new PlayerWorldUpdates();

        // Sync locations
        foreach (var playerData in world.Locations)
        {
            var dbData = Locations!.FirstOrDefault(x => x.LocationId == playerData.Key);
            if (dbData != null)
            {
                if (playerData.Value && !dbData.Tracked)
                {
                    dbData.Tracked = true;
                    dbData.TrackedTime = DateTimeOffset.Now;
                    worldUpdate.Locations.Add(dbData);
                }
            }
            else
            {
                dbData = new MultiplayerLocationState()
                {
                    GameId = GameId,
                    PlayerId = Id,
                    LocationId = playerData.Key,
                    Tracked = playerData.Value,
                    TrackedTime = playerData.Value ? DateTimeOffset.Now : null
                };
                Locations!.Add(dbData);
                worldUpdate.Locations.Add(dbData);
            }
        }

        // Sync items
        foreach (var playerData in world.Items)
        {
            var dbData = Items!.FirstOrDefault(x => x.Item == playerData.Key);
            if (dbData != null)
            {
                if (playerData.Value > dbData.TrackingValue)
                {
                    dbData.TrackingValue = playerData.Value;
                    dbData.TrackedTime = DateTimeOffset.Now;
                    worldUpdate.Items.Add(dbData);
                }
            }
            else
            {
                dbData = new MultiplayerItemState()
                {
                    GameId = GameId,
                    PlayerId = Id,
                    Item = playerData.Key,
                    TrackingValue = playerData.Value,
                    TrackedTime = playerData.Value > 0 ? DateTimeOffset.Now : null
                };
                Items!.Add(dbData);
                worldUpdate.Items.Add(dbData);
            }
        }

        // Sync dungeons
        foreach (var playerData in world.Dungeons)
        {
            var dbData = Dungeons!.FirstOrDefault(x => x.Dungeon == playerData.Key);
            if (dbData != null)
            {
                if (playerData.Value && !dbData.Tracked)
                {
                    dbData.Tracked = playerData.Value;
                    dbData.TrackedTime = DateTimeOffset.Now;
                    worldUpdate.Dungeons.Add(dbData);
                }
            }
            else
            {
                dbData = new MultiplayerDungeonState()
                {
                    GameId = GameId,
                    PlayerId = Id,
                    Dungeon = playerData.Key,
                    Tracked = playerData.Value,
                    TrackedTime = playerData.Value ? DateTimeOffset.Now : null
                };
                Dungeons!.Add(dbData);
                worldUpdate.Dungeons.Add(dbData);
            }
        }

        // Sync bosses
        foreach (var playerData in world.Bosses)
        {
            var dbData = Bosses!.FirstOrDefault(x => x.Boss == playerData.Key);
            if (dbData != null)
            {
                if (playerData.Value && !dbData.Tracked)
                {
                    dbData.Tracked = playerData.Value;
                    dbData.TrackedTime = DateTimeOffset.Now;
                    worldUpdate.Bosses.Add(dbData);
                }
            }
            else
            {
                dbData = new MultiplayerBossState()
                {
                    GameId = GameId,
                    PlayerId = Id,
                    Boss = playerData.Key,
                    Tracked = playerData.Value,
                    TrackedTime = playerData.Value ? DateTimeOffset.Now : null
                };
                Bosses!.Add(dbData);
                worldUpdate.Bosses.Add(dbData);
            }
        }

        return worldUpdate;
    }

    /// <summary>
    /// Copies properties from the provided state
    /// </summary>
    /// <param name="other"></param>
    public void Copy(MultiplayerPlayerState other)
    {
        IsAdmin = other.IsAdmin;
        Config = other.Config;
        Status = other.Status;
        GenerationData = other.GenerationData;
        WorldId = other.WorldId;
    }
}
