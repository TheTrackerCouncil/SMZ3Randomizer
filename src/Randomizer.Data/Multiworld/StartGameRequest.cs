using System.Collections.Generic;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Multiworld;

public class StartGameRequest : MultiworldRequest
{
    public StartGameRequest(List<string> playerGuids, TrackerState initialState)
    {
        PlayerGuids = playerGuids;
        TrackerState = initialState;
    }

    public List<string> PlayerGuids { get; }
    public TrackerState TrackerState { get; }

}
