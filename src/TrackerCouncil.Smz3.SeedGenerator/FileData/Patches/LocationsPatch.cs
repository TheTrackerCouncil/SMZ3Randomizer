using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[SkipForParsedRoms]
public class LocationsPatch : RomPatch
{
    private static Dictionary<int, string> s_smCharMap = new()
    {
        { 0x3CE0, "A" },
        { 0x3CE1, "B" },
        { 0x3CE2, "C" },
        { 0x3CE3, "D" },
        { 0x3CE4, "E" },
        { 0x3CE5, "F" },
        { 0x3CE6, "G" },
        { 0x3CE7, "H" },
        { 0x3CE8, "I" },
        { 0x3CE9, "J" },
        { 0x3CEA, "K" },
        { 0x3CEB, "L" },
        { 0x3CEC, "M" },
        { 0x3CED, "N" },
        { 0x3CEE, "O" },
        { 0x3CEF, "P" },
        { 0x3CF0, "Q" },
        { 0x3CF1, "R" },
        { 0x3CF2, "S" },
        { 0x3CF3, "T" },
        { 0x3CF4, "U" },
        { 0x3CF5, "V" },
        { 0x3CF6, "W" },
        { 0x3CF7, "X" },
        { 0x3CF8, "Y" },
        { 0x3CF9, "Z" },
        { 0x3C4E, " " },
        { 0x3CFF, "!" },
        { 0x3CFE, "?" },
        { 0x3CFD, "'" },
        { 0x3CFB, "," },
        { 0x3CFA, "." },
        { 0x3CCF, "-" },
        { 0x3C80, "1" },
        { 0x3C81, "2" },
        { 0x3C82, "3" },
        { 0x3C83, "4" },
        { 0x3C84, "5" },
        { 0x3C85, "6" },
        { 0x3C86, "7" },
        { 0x3C87, "8" },
        { 0x3C88, "9" },
        { 0x3C89, "0" },
        { 0x3C0A, "%" },
        { 0x3C90, "a" },
        { 0x3C91, "b" },
        { 0x3C92, "c" },
        { 0x3C93, "d" },
        { 0x3C94, "e" },
        { 0x3C95, "f" },
        { 0x3C96, "g" },
        { 0x3C97, "h" },
        { 0x3C98, "i" },
        { 0x3C99, "j" },
        { 0x3C9A, "k" },
        { 0x3C9B, "l" },
        { 0x3C9C, "m" },
        { 0x3C9D, "n" },
        { 0x3C9E, "o" },
        { 0x3C9F, "p" },
        { 0x3CA0, "q" },
        { 0x3CA1, "r" },
        { 0x3CA2, "s" },
        { 0x3CA3, "t" },
        { 0x3CA4, "u" },
        { 0x3CA5, "v" },
        { 0x3CA6, "w" },
        { 0x3CA7, "x" },
        { 0x3CA8, "y" },
        { 0x3CA9, "z" },
        { 0x3CAA, "\"" },
        { 0x3CAB, ":" },
        { 0x3CAC, "~" },
        { 0x3CAD, "@" },
        { 0x3CAE, "#" },
        { 0x3CAF, "+" },
        { 0x000E, "_" }
    };

    private static Dictionary<int, ItemType> s_plmItemTypes = new()
    {
        { 0xEED7, ItemType.ETank },
        { 0xEEDB, ItemType.Missile },
        { 0xEEDF, ItemType.Super },
        { 0xEEE3, ItemType.PowerBomb },
        { 0xEEE7, ItemType.Bombs },
        { 0xEEEB, ItemType.Charge },
        { 0xEEEF, ItemType.Ice },
        { 0xEEF3, ItemType.HiJump },
        { 0xEEF7, ItemType.SpeedBooster },
        { 0xEEFB, ItemType.Wave },
        { 0xEEFF, ItemType.Spazer },
        { 0xEF03, ItemType.SpringBall },
        { 0xEF07, ItemType.Varia },
        { 0xEF0B, ItemType.Gravity },
        { 0xEF0F, ItemType.XRay },
        { 0xEF13, ItemType.Plasma },
        { 0xEF17, ItemType.Grapple },
        { 0xEF1B, ItemType.SpaceJump },
        { 0xEF1F, ItemType.ScrewAttack },
        { 0xEF23, ItemType.Morph },
        { 0xEF27, ItemType.ReserveTank }
    };

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var patches = new List<GeneratedPatch>();
        patches.AddRange(WriteMetroidLocations(data));
        patches.AddRange(WriteZ3Locations(data));
        return patches;
    }

    public static List<ParsedRomLocationDetails> GetLocationsFromRom(byte[] rom, List<string> playerNames, World exampleWorld, bool isMultiworldEnabled, List<int> smz3ItemTypes, bool isCasRom)
    {
        var toReturn = new List<ParsedRomLocationDetails>();

        foreach (var location in exampleWorld.Regions.OfType<SMRegion>().SelectMany(x => x.Locations))
        {
            toReturn.Add(GetParsedLocationDetails(rom, location, true, playerNames, isMultiworldEnabled, smz3ItemTypes, isCasRom));
        }

        foreach (var location in exampleWorld.Regions.OfType<Z3Region>().SelectMany(x => x.Locations))
        {
            toReturn.Add(GetParsedLocationDetails(rom, location, false, playerNames, isMultiworldEnabled, smz3ItemTypes, isCasRom));
        }

        return toReturn;
    }

    private static Dictionary<int, int> s_locationIdMappings = new()
    {
        { 256 + 230, 256 + 196 }, // GanonsTowerRandomizerRoomTopLeft
        { 256 + 231, 256 + 197 }, // GanonsTowerRandomizerRoomTopRight
        { 256 + 232, 256 + 198 }, // GanonsTowerRandomizerRoomBottomLeft
        { 256 + 233, 256 + 199 }, // GanonsTowerRandomizerRoomBottomRight
        { 256 + 234, 256 + 200 }, // GanonsTowerHopeRoomLeft
        { 256 + 235, 256 + 201 }, // GanonsTowerHopeRoomRight
        { 256 + 236, 256 + 202 }, // GanonsTowerTileRoom
    };

    private static ParsedRomLocationDetails GetParsedLocationDetails(byte[] rom, Location location, bool isSuperMetroidLocation, List<string> playerNames, bool isMultiworldEnabled, List<int> smz3ItemTypes, bool isCasRom)
    {
        var isLocal = false;
        ItemType itemType;
        var playerName = "";
        var isProgression = false;
        var itemName = "";

        var id = (int)location.Id;
        if (!isCasRom && s_locationIdMappings.TryGetValue(id, out var newId))
        {
            id = newId;
        }

        if (isMultiworldEnabled)
        {
            var address = 0x386000 + id * 8;
            var bytes = rom.Skip(address).Take(8).ToArray();
            isLocal = BitConverter.ToInt16(bytes, 0) == 0;
            var itemNumber = BitConverter.ToInt16(bytes, 2);
            var ownerPlayerId = BitConverter.ToInt16(bytes, 4);
            var archipelagoFlags = BitConverter.ToInt16(bytes, 6);
            if (ownerPlayerId < playerNames.Count)
            {
                playerName = playerNames[ownerPlayerId];
            }
            else
            {
                playerName = $"Player {ownerPlayerId}";
            }
            isProgression = archipelagoFlags > 0;

            if (!isLocal)
            {
                if (!smz3ItemTypes.Contains(itemNumber))
                {
                    itemType = isProgression ? ItemType.OtherGameProgressionItem : ItemType.OtherGameItem;
                    itemName = isSuperMetroidLocation
                        ? GetSuperMetroidItemName(rom, archipelagoFlags, isProgression)
                        : GetZeldaItemName(rom, archipelagoFlags, isProgression);
                }
                else
                {
                    itemType = (ItemType)itemNumber;
                    itemName = itemType.GetDescription();
                }
            }
            else
            {
                itemType = (ItemType)itemNumber;
                itemName = itemType.GetDescription();
            }
        }
        else
        {
            itemType = isSuperMetroidLocation
                ? GetSuperMetroidLocationItemType(rom, location)
                : GetZeldaLocationItemType(rom, location);
            itemName = itemType.GetDescription();
            playerName = playerNames.First();
            isProgression = true;
        }

        if (itemType is ItemType.Key or ItemType.BigKey or ItemType.Compass or ItemType.Map)
        {
            var originalItemType = itemType;
            itemType = location.Region.ConvertToRegionItemType(originalItemType);
        }

        return new ParsedRomLocationDetails()
        {
            Location = location.Id,
            IsLocalPlayerItem = isLocal,
            ItemType = itemType,
            PlayerName = playerName,
            IsProgression = isProgression,
            ItemName = itemName
        };
    }

    private static ItemType GetSuperMetroidLocationItemType(byte[] rom, Location location)
    {
        var romValue = BitConverter.ToUInt16(rom.Skip(Snes(location.RomAddress)).Take(2).ToArray(), 0);

        // Regular Super Metroid item
        if (s_plmItemTypes.TryGetValue(romValue, out var itemType))
        {
            return itemType;
        }
        // Super Metroid item in a Chozo ball
        else if (s_plmItemTypes.TryGetValue(romValue - 0x54, out itemType))
        {
            return itemType;
        }
        // Super Metroid Item hidden in a block
        else if (s_plmItemTypes.TryGetValue(romValue - 0xA8, out itemType))
        {
            return itemType;
        }
        // Zelda item
        else
        {
            return (ItemType)rom[Snes(location.RomAddress+5)];
        }
    }

    private static ItemType GetZeldaLocationItemType(byte[] rom, Location location)
    {
        return (ItemType)rom[Snes(location.RomAddress)];
    }

    private static string GetSuperMetroidItemName(byte[] rom, short archipelagoFlags, bool isProgression)
    {
        var mod = isProgression ? 0 : 0x8000;
        var textLocation = 0x390000 + (archipelagoFlags - 1 + mod) * 64;
        var textBytes = rom.Skip(textLocation).Take(64).ToArray();

        var text = "";
        for (var i = 0; i < 64; i += 2)
        {
            var value = BitConverter.ToInt16(textBytes, i);
            if (s_smCharMap.TryGetValue(value, out var character))
            {
                text += character;
            }
        }

        return text.Replace("___", "").Trim();
    }

    private static string GetZeldaItemName(byte[] rom, short archipelagoFlags, bool isProgression)
    {
        var mod = isProgression ? 0 : 0x8000;
        var textLocation = 0x390000 + 100 * 64 + (archipelagoFlags - 1 + mod) * 20;
        var textBytes = rom.Skip(textLocation).Take(20).ToArray();
        var text = Encoding.UTF8.GetString(textBytes);
        return text.Replace("___", "").Replace("\0", "").Trim();
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
            /*else
            {
                var plmId = GetMetroidItemValue(data, location);
                yield return new GeneratedPatch(Snes(location.RomAddress), UshortBytes(plmId));
                if (plmId >= 0xEFE0)
                    yield return new GeneratedPatch(Snes(location.RomAddress + 5), new[] { GetZ3ItemId(data, location) });
            }*/
        }
    }

    private ushort GetMetroidItemValue(GetPatchesRequest data, Location location)
    {
        var plmId = 0xEFE0;

        /*var plmId = GetPatchesRequest.EnableMultiworld ?
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
            */

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
                yield return new GeneratedPatch(Snes(0x9E3BB), location.Item.Type == ItemType.KeyTH ? [0xE4] : [0xEB]);
            }

            if (GetPatchesRequest.EnableMultiworld)
            {
                yield return new GeneratedPatch(Snes(location.RomAddress), new[] { (byte)(location.Id - 256) });
                yield return ItemTablePatch(location, GetZ3ItemId(data, location));
            }
            /*
            else
            {
                yield return new GeneratedPatch(Snes(location.RomAddress), new[] { GetZ3ItemId(data, location) });
            }
            */
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
