using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Server;

public class MultiplayerPlayer
{
    public MultiplayerPlayer(MultiplayerGame game, string playerGuid, string playerKey, string playerName, string connectionId)
    {
        Game = game;
        Guid = playerGuid;
        Key = playerKey;
        ConnectionId = connectionId;
        State = new MultiplayerPlayerState
        {
            Guid = playerGuid,
            PlayerName = playerName
        };
    }
    public MultiplayerGame Game { get; }
    public string Guid { get; set; }
    public string Key { get; set; }
    public string ConnectionId { get; set; }
    public MultiplayerPlayerState State { get; set; }
}
