using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Server;

/// <summary>
/// A particular player in a multiplayer game
/// </summary>
public class MultiplayerPlayer
{
    public MultiplayerPlayer(MultiplayerGame game, string playerGuid, string playerKey, string playerName, string phoneticName, string connectionId)
    {
        Game = game;
        ConnectionId = connectionId;
        State = new MultiplayerPlayerState
        {
            Guid = playerGuid,
            Key = playerKey,
            PlayerName = playerName,
            PhoneticName = string.IsNullOrEmpty(phoneticName) ? playerName : phoneticName,
            GameId = game.State.Id,
            IsConnected = true
        };
    }

    public MultiplayerPlayer(MultiplayerGame game, MultiplayerPlayerState state)
    {
        Game = game;
        State = state;
        State.IsConnected = false;
    }

    public MultiplayerGame Game { get; }
    public string Guid => State.Guid;
    public string Key => State.Key;
    public string ConnectionId { get; set; } = "";
    public MultiplayerPlayerState State { get; set; }
    public string? PlayerGenerationData => State.GenerationData;
    public bool IsGameAdmin => State.IsAdmin;
}
