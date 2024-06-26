using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.Options;

public class PlandoZeldaPrizeConfig
{
    /// <summary>
    /// Pool of 56 enemy drops
    /// </summary>
    public ICollection<DropPrize>? EnemyDrops { get; set; }

    /// <summary>
    /// Pool of tree pull prizes
    /// </summary>
    public ICollection<DropPrize>? TreePulls { get; set; }

    /// <summary>
    /// Normal crab drops
    /// </summary>
    public DropPrize? CrabBaseDrop { get; set; }

    /// <summary>
    /// Unique crab eigth drop
    /// </summary>
    public DropPrize? CrabEightDrop { get; set; }

    /// <summary>
    /// Stun Prize
    /// </summary>
    public DropPrize? StunPrize { get; set; }

    /// <summary>
    /// Fish Prize?
    /// </summary>
    public DropPrize? FishPrize { get; set; }
}
