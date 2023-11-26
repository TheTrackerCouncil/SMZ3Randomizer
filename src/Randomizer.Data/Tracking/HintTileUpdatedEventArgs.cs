using System;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Tracking;

/// <summary>
/// EventArgs for when a hint tile is viewed or cleared
/// </summary>
public class HintTileUpdatedEventArgs : EventArgs
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="playerHintTile"></param>
    public HintTileUpdatedEventArgs(PlayerHintTile playerHintTile)
    {
        PlayerHintTile = playerHintTile;
    }

    /// <summary>
    /// The hint tile that was viewed or cleared
    /// </summary>
    public PlayerHintTile PlayerHintTile { get; set; }
}
