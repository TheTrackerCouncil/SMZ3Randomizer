using Randomizer.Data.Multiworld;
using Randomizer.Data.Options;

namespace Randomizer.Multiworld.Server;

public class MultiworldPlayer
{
    public MultiworldPlayer(string playerGuid, string playerKey, string playerName)
    {
        Guid = playerGuid;
        Key = playerKey;
        State = new MultiworldPlayerState
        {
            Guid = playerGuid,
            PlayerName = playerName
        };
    }
    public string Guid { get; set; }
    public string Key { get; set; }
    public MultiworldPlayerState State { get; set; }
}
