using System.Collections.Generic;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Multiworld;

public class SubmitConfigRequest : MultiworldRequest
{
    public SubmitConfigRequest(string gameGuid, string playerGuid, string playerKey, Config config)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
        Config = config;
    }

    public Config Config { get; }

}
