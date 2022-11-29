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
    public string? PlayerGenerationData { get; set; }
    public bool IsGameAdmin => Game.AdminPlayer == this;
}
