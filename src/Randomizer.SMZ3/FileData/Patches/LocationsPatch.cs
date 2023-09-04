using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;

namespace Randomizer.SMZ3.FileData.Patches;

public class LocationsPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var patches = new List<GeneratedPatch>();
        patches.AddRange(WriteMetroidLocations(data));
        patches.AddRange(WriteZ3Locations(data));
        return patches;
    }

    private IEnumerable<GeneratedPatch> WriteMetroidLocations(GetPatchesRequest data)
    {
        foreach (var location in data.World.Regions.OfType<SMRegion>().SelectMany(x => x.Locations))
        {
            if (GetPatchesRequest.EnableMultiworld)
            {
                yield return new GeneratedPatch(Snes(location.RomAddress), UshortBytes(GetMetroidItemValue(data, location)));
                yield return ItemTablePatch(location, GetZ3ItemId(data, location));
            }
            else
            {
                var plmId = GetMetroidItemValue(data, location);
                yield return new GeneratedPatch(Snes(location.RomAddress), UshortBytes(plmId));
                if (plmId >= 0xEFE0)
                    yield return new GeneratedPatch(Snes(location.RomAddress + 5), new[] { GetZ3ItemId(data, location) });
            }
        }
    }

    private ushort GetMetroidItemValue(GetPatchesRequest data, Location location)
    {
        var plmId = GetPatchesRequest.EnableMultiworld ?
            0xEFE0 :
            location.Item.Type switch
            {
                ItemType.ETank => 0xEED7,
                ItemType.Missile => 0xEEDB,
                ItemType.Super => 0xEEDF,
                ItemType.PowerBomb => 0xEEE3,
                ItemType.Bombs => 0xEEE7,
                ItemType.Charge => 0xEEEB,
                ItemType.Ice => 0xEEEF,
                ItemType.HiJump => 0xEEF3,
                ItemType.SpeedBooster => 0xEEF7,
                ItemType.Wave => 0xEEFB,
                ItemType.Spazer => 0xEEFF,
                ItemType.SpringBall => 0xEF03,
                ItemType.Varia => 0xEF07,
                ItemType.Plasma => 0xEF13,
                ItemType.Grapple => 0xEF17,
                ItemType.Morph => 0xEF23,
                ItemType.ReserveTank => 0xEF27,
                ItemType.Gravity => 0xEF0B,
                ItemType.XRay => 0xEF0F,
                ItemType.SpaceJump => 0xEF1B,
                ItemType.ScrewAttack => 0xEF1F,
                _ => 0xEFE0,
            };

        plmId += plmId switch
        {
            0xEFE0 => location.Type switch
            {
                LocationType.Chozo => 4,
                LocationType.Hidden => 8,
                _ => 0
            },
            _ => location.Type switch
            {
                LocationType.Chozo => 0x54,
                LocationType.Hidden => 0xA8,
                _ => 0
            }
        };

        return (ushort)plmId;
    }

    private IEnumerable<GeneratedPatch> WriteZ3Locations(GetPatchesRequest data)
    {
        foreach (var location in data.World.Regions.OfType<Z3Region>().SelectMany(x => x.Locations))
        {
            if (location.Type == LocationType.HeraStandingKey)
            {
                yield return new GeneratedPatch(Snes(0x9E3BB), location.Item.Type == ItemType.KeyTH ? new byte[] { 0xE4 } : new byte[] { 0xEB });
            }

            if (GetPatchesRequest.EnableMultiworld)
            {
                yield return new GeneratedPatch(Snes(location.RomAddress), new[] { (byte)(location.Id - 256) });
                yield return ItemTablePatch(location, GetZ3ItemId(data, location));
            }
            else
            {
                yield return new GeneratedPatch(Snes(location.RomAddress), new[] { GetZ3ItemId(data, location) });
            }
        }
    }

    private byte GetZ3ItemId(GetPatchesRequest data, Location location)
    {
        var item = location.Item;
        var itemType = item.Type;
        var value = location.Type == LocationType.NotInDungeon ||
            !(item.IsDungeonItem && location.Region.IsRegionItem(item) && item.World == data.World) ? itemType : item switch
            {
                _ when item.IsKey => ItemType.Key,
                _ when item.IsBigKey => ItemType.BigKey,
                _ when item.IsMap => ItemType.Map,
                _ when item.IsCompass => ItemType.Compass,
                _ => throw new InvalidOperationException($"Tried replacing {item} with a dungeon region item"),
            };
        return (byte)value;
    }

    private GeneratedPatch ItemTablePatch(Location location, byte itemId)
    {
        var type = location.Item.World == location.Region.World ? 0 : 1;
        var owner = location.Item.World.Id;
        return new GeneratedPatch(0x386000 + ((int)location.Id * 8), new[] { type, itemId, owner, 0 }.SelectMany(UshortBytes).ToArray());
    }
}
