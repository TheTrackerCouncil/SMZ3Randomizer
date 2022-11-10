using System.Collections.Generic;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Multiworld;

public class MultiworldPlayerState
{
    public MultiworldPlayerState(int id, string guid, IEnumerable<TrackerLocationState> locations,
        IEnumerable<TrackerItemState> items, IEnumerable<TrackerBossState> bosses,
        IEnumerable<TrackerDungeonState> dungeons)
    {
        Id = id;
        Guid = guid;
        Locations = locations;
        Items = items;
        Bosses = bosses;
        Dungeons = dungeons;
    }

    public int Id { get; set; }
    public string Guid { get; set; }
    public IEnumerable<TrackerLocationState> Locations { get; set; }
    public IEnumerable<TrackerItemState> Items { get; set; }
    public IEnumerable<TrackerBossState> Bosses { get; set; }
    public IEnumerable<TrackerDungeonState> Dungeons { get; set; }
}
