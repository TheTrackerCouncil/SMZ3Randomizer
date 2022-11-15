using Randomizer.Data.Multiworld;
using Randomizer.Data.Options;

namespace Randomizer.Multiworld.Server;

public class MultiworldPlayer
{
    public MultiworldPlayer(MultiworldGame game, string playerGuid, string playerKey, string playerName, string connectionId)
    {
        Game = game;
        Guid = playerGuid;
        Key = playerKey;
        ConnectionId = connectionId;
        State = new MultiworldPlayerState
        {
            Guid = playerGuid,
            PlayerName = playerName
        };
    }
    public MultiworldGame Game { get; }
    public string Guid { get; set; }
    public string Key { get; set; }
    public string ConnectionId { get; set; }
    public MultiworldPlayerState State { get; set; }
}
