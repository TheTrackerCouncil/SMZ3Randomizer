using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// List of all the hint tiles
/// </summary>
public class HintTileList : List<HintTile>, IMergeable<HintTile>;
