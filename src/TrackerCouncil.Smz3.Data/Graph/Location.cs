using System;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.Graph;

public class Location
{
    public Location(LocationId id, String name, ItemType vanillaItem)
    {
        Id = id;
        Name = name;
        VanillaItem = vanillaItem;
    }

    public LocationId Id { get; protected set; }

    public String Name { get; protected set; }

    public ItemType VanillaItem { get; protected set; }
}
