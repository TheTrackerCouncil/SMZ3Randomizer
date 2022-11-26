using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Server;

/// <summary>
/// A particular player in a multiplayer game
/// </summary>
public class MultiplayerPlayer
{
    public MultiplayerPlayer(MultiplayerGame game, string playerGuid, string playerKey, string playerName, string connectionId)
    {
        Game = game;
        Key = playerKey;
        ConnectionId = connectionId;
        State = new MultiplayerPlayerState
        {
            Guid = playerGuid,
            PlayerName = playerName
        };
    }
    public MultiplayerGame Game { get; }
    public string Guid => State.Guid;
    public string Key { get; }
    public string ConnectionId { get; set; }
    public MultiplayerPlayerState State { get; set; }
    public bool IsGameAdmin => Game.AdminPlayer == this;

    /// <summary>
    /// Marks a location as accessed
    /// </summary>
    /// <param name="locationId"></param>
    public void TrackLocation(int locationId)
    {
        if (State.Locations != null)
            State.Locations[locationId] = true;
    }

    /// <summary>
    /// Updates the amount of an item that have been retrieved
    /// </summary>
    /// <param name="type"></param>
    public void TrackItem(ItemType type)
    {
        if (State.Items == null) return;
        if (State.Items.TryGetValue(type, out var value))
        {
            State.Items[type] = value + 1;
        }
        else
        {
            State.Items[type] = 1;
        }
    }

    /// <summary>
    /// Marks a boss as defeated
    /// </summary>
    /// <param name="type"></param>
    public void TrackBoss(BossType type)
    {
        if (State.Bosses == null) return;
        State.Bosses[type] = true;
    }

    /// <summary>
    /// Marks a dungeon as completed
    /// </summary>
    /// <param name="name"></param>
    public void TrackDungeon(string name)
    {
        if (State.Dungeons == null) return;
        State.Dungeons[name] = true;
    }
}
