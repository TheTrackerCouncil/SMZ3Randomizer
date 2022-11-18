using System.Collections.Generic;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Multiplayer;

public class StartGameRequest : MultiplayerRequest
{
    public StartGameRequest(List<string> playerGuids, TrackerState initialState)
    {
        PlayerGuids = playerGuids;
        TrackerState = initialState;
    }

    public List<string> PlayerGuids { get; }
    public TrackerState TrackerState { get; }

}
