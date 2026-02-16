using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class ZeldaMiscData(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<ZeldaMiscData> logger, IWorldQueryService worldQueryService) : AutoTrackerModule(tracker, snesConnector, logger)
{
    private List<Location> _locations = [];

    public override void Initialize()
    {
        _locations = worldQueryService.AllLocations().Where(x =>
            x.MemoryType == LocationMemoryType.ZeldaMisc && (int)x.Id >= 256).ToList();

        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7ef280,
            Length = 0x280,
            FrequencySeconds = 1,
            OnResponse = CheckZeldaMisc,
            Filter = () => IsInZelda
        });

        if (Tracker.Options.AutoTrackingMode is AutoTrackingMode.Inventory)
        {
            SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0xa06410,
                Length = 0x2,
                FrequencySeconds = Tracker.Options.AutoTrackingMode == AutoTrackingMode.Inventory ? 4 : 2,
                OnResponse = CheckZeldaNpcsFromMetroid,
                Filter = () => IsInMetroid
            });

            SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0xa17b40,
                Length = 0x180,
                FrequencySeconds = 2,
                OnResponse = CheckZeldaInventoryAndMiscLocationsFromMetroid,
                Filter = () => IsInMetroid
            });

            if (Tracker.Options.AutoTrackingMode == AutoTrackingMode.Inventory && Tracker.World.Config.ZeldaKeysanity)
            {
                SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
                {
                    MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                    SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
                    AddressFormat = AddressFormat.Snes9x,
                    SniMemoryMapping = MemoryMapping.ExHiRom,
                    Address = 0xa17f50,
                    Length = 0x10,
                    FrequencySeconds = 2,
                    OnResponse = CheckZeldaKeysFromMetroid,
                    Filter = () => IsInMetroid
                });
            }
        }
    }

    private void CheckZeldaMisc(SnesData data, SnesData? prevData)
    {
        if (!HasValidState || prevData == null) return;

        // Failsafe to prevent incorrect checking
        if (data.ReadUInt8(0x190) == 0xFF && data.ReadUInt8(0x191) == 0xFF)
        {
            Logger.LogInformation("Ignoring due to transition");
            return;
        }

        CheckLocations(data, prevData);

        var hasFairy = false;
        for (var i = 0; i < 4; i++)
        {
            hasFairy |= data.ReadUInt8(0xDC + i) == 6;
        }

        AutoTracker.PlayerHasFairy = hasFairy;

        // Activated flute
        if (data.CheckUInt8Flag(0x10C, 0x01) && !prevData.CheckUInt8Flag(0x10C, 0x01))
        {
            var duckItem = worldQueryService.FirstOrDefault("Duck");
            if (duckItem?.TrackingState == 0)
            {
                Tracker.ItemTracker.TrackItem(duckItem, null, null, false, true);
            }
        }

        // Check if the player cleared Aga
        if (data.ReadUInt8(0x145) >= 3)
        {
            var castleTower = Tracker.World.CastleTower;
            if (!castleTower.Boss.State.Defeated)
            {
                Tracker.BossTracker.MarkBossAsDefeated(castleTower, autoTracked: true);
                Logger.LogInformation("Auto tracked {Name} as cleared", castleTower.Name);
            }
        }

        if (Tracker.Options.AutoTrackingMode == AutoTrackingMode.Inventory)
        {
            ManageZeldaInventory(data, prevData, -0x7ef280);
            ManageZeldaKeys(data, prevData, -0x7ef280);
        }
    }

    private void CheckZeldaNpcsFromMetroid(SnesData data, SnesData? prevData)
    {
        if (!HasValidState || prevData == null) return;
        CheckLocations(data, prevData, 0x190, i => i is 0x190 or 0x191);
    }

    private void CheckZeldaInventoryAndMiscLocationsFromMetroid(SnesData data, SnesData? prevData)
    {
        if (!HasValidState || prevData == null) return;

        if (Tracker.Options.AutoTrackingMode == AutoTrackingMode.Inventory)
        {
            ManageZeldaInventory(data, prevData, -0x7ef340);
        }

        // Check if Aga was deleted
        if (data.ReadUInt8(0x85) >= 3)
        {
            var castleTower = Tracker.World.CastleTower;
            if (!castleTower.Boss.State.Defeated)
            {
                Tracker.BossTracker.MarkBossAsDefeated(castleTower, autoTracked: true);
                Logger.LogInformation("Auto tracked {Name} as cleared", castleTower.Name);
            }
        }

        CheckLocations(data, prevData, 0xC0, i => i is 0x149 or 0x146);
    }

    private void CheckZeldaKeysFromMetroid(SnesData data, SnesData? prevData)
    {
        if (!HasValidState || prevData == null) return;
        ManageZeldaKeys(data, prevData, -0x7ef4E0);
    }

    private void CheckLocations(SnesData data, SnesData prevData, int offset = 0, Func<int, bool>? filter = null)
    {
        var locations = filter == null ? _locations : _locations.Where(l => filter(l.MemoryAddress ?? 0));
        foreach (var location in locations)
        {
            try
            {
                var loc = (location.MemoryAddress ?? 0) - offset;
                var flag = location.MemoryFlag ?? 0;
                var currentCleared = data.CheckUInt8Flag(loc, flag);
                var prevCleared = prevData.CheckUInt8Flag(loc, flag);
                if (location.Autotracked == false && currentCleared && prevCleared)
                {
                    TrackLocation(location);
                }

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to auto track location: {LocationName}", location.Name);
                Tracker.Error();
            }
        }
    }

    private void ManageZeldaInventory(SnesData data, SnesData prevData, int offset)
    {
        var tracker = Tracker.ItemTracker;

        UpdateItem(ItemType.ProgressiveGlove, ItemSnesMemoryType.Byte, 0x7ef354);
        UpdateItem(ItemType.ProgressiveSword, ItemSnesMemoryType.Byte, 0x7ef359);
        UpdateItem(ItemType.ProgressiveShield, ItemSnesMemoryType.Byte, 0x7ef35A);
        UpdateItem(ItemType.ProgressiveTunic, ItemSnesMemoryType.Byte, 0x7ef35B);

        UpdateItem(ItemType.Bow, ItemSnesMemoryType.ByteSingleItemPositive, 0x7ef340);

        UpdateItem(ItemType.Hookshot, ItemSnesMemoryType.ByteSingleItem1, 0x7ef342);
        UpdateItem(ItemType.Firerod, ItemSnesMemoryType.ByteSingleItem1, 0x7ef345);
        UpdateItem(ItemType.Icerod, ItemSnesMemoryType.ByteSingleItem1, 0x7ef346);
        UpdateItem(ItemType.Bombos, ItemSnesMemoryType.ByteSingleItem1, 0x7ef347);
        UpdateItem(ItemType.Ether, ItemSnesMemoryType.ByteSingleItem1, 0x7ef348);
        UpdateItem(ItemType.Quake, ItemSnesMemoryType.ByteSingleItem1, 0x7ef349);
        UpdateItem(ItemType.Lamp, ItemSnesMemoryType.ByteSingleItem1, 0x7ef34a);
        UpdateItem(ItemType.Hammer, ItemSnesMemoryType.ByteSingleItem1, 0x7ef34b);
        UpdateItem(ItemType.Bugnet, ItemSnesMemoryType.ByteSingleItem1, 0x7ef34d);
        UpdateItem(ItemType.Book, ItemSnesMemoryType.ByteSingleItem1, 0x7ef34e);
        UpdateItem(ItemType.Somaria, ItemSnesMemoryType.ByteSingleItem1, 0x7ef350);
        UpdateItem(ItemType.Byrna, ItemSnesMemoryType.ByteSingleItem1, 0x7ef351);
        UpdateItem(ItemType.Cape, ItemSnesMemoryType.ByteSingleItem1, 0x7ef352);

        UpdateItem(ItemType.Mirror, ItemSnesMemoryType.ByteSingleItem12, 0x7ef353);
        UpdateItem(ItemType.Boots, ItemSnesMemoryType.ByteSingleItem12, 0x7ef355);
        UpdateItem(ItemType.Flippers, ItemSnesMemoryType.ByteSingleItem12, 0x7ef356);
        UpdateItem(ItemType.MoonPearl, ItemSnesMemoryType.ByteSingleItem12, 0x7ef357);

        UpdateItem(ItemType.HalfMagic, ItemSnesMemoryType.Byte, 0x7ef37b);
        UpdateItem(ItemType.BlueBoomerang, ItemSnesMemoryType.ByteFlag, 0x7ef38c, 0x80);
        UpdateItem(ItemType.RedBoomerang, ItemSnesMemoryType.ByteFlag, 0x7ef38c, 0x40);
        UpdateItem(ItemType.Powder, ItemSnesMemoryType.ByteFlag, 0x7ef38c, 0x10);
        UpdateItem(ItemType.SilverArrows, ItemSnesMemoryType.ByteFlag, 0x7ef38e, 0x40);
        UpdateItem(ItemType.Mushroom, ItemSnesMemoryType.ByteFlag, 0x7ef38c, 0x20);
        UpdateItem(ItemType.Shovel, ItemSnesMemoryType.ByteFlag, 0x7ef38c, 0x4);

        UpdateItem(ItemType.Bottle, ItemSnesMemoryType.Bottle, 0x7ef35c);
        UpdateItem(ItemType.Flute, ItemSnesMemoryType.Flute, 0x7ef38c);

        UpdateItem(ItemType.BigKeyGT, ItemSnesMemoryType.ByteFlag, 0x7ef366, 0x04);
        UpdateItem(ItemType.BigKeyTR, ItemSnesMemoryType.ByteFlag, 0x7ef366, 0x08);
        UpdateItem(ItemType.BigKeyTT, ItemSnesMemoryType.ByteFlag, 0x7ef366, 0x10);
        UpdateItem(ItemType.BigKeyTH, ItemSnesMemoryType.ByteFlag, 0x7ef366, 0x20);
        UpdateItem(ItemType.BigKeyIP, ItemSnesMemoryType.ByteFlag, 0x7ef366, 0x40);
        UpdateItem(ItemType.BigKeySW, ItemSnesMemoryType.ByteFlag, 0x7ef366, 0x80);
        UpdateItem(ItemType.BigKeyMM, ItemSnesMemoryType.ByteFlag, 0x7ef367, 0x01);
        UpdateItem(ItemType.BigKeyPD, ItemSnesMemoryType.ByteFlag, 0x7ef367, 0x02);
        UpdateItem(ItemType.BigKeySP, ItemSnesMemoryType.ByteFlag, 0x7ef367, 0x04);
        UpdateItem(ItemType.BigKeyDP, ItemSnesMemoryType.ByteFlag, 0x7ef367, 0x10);
        UpdateItem(ItemType.BigKeyEP, ItemSnesMemoryType.ByteFlag, 0x7ef367, 0x20);

        UpdateItem(ItemType.MapGT, ItemSnesMemoryType.ByteFlag, 0x7ef368, 0x04);
        UpdateItem(ItemType.MapTR, ItemSnesMemoryType.ByteFlag, 0x7ef368, 0x08);
        UpdateItem(ItemType.MapTT, ItemSnesMemoryType.ByteFlag, 0x7ef368, 0x10);
        UpdateItem(ItemType.MapTH, ItemSnesMemoryType.ByteFlag, 0x7ef368, 0x20);
        UpdateItem(ItemType.MapIP, ItemSnesMemoryType.ByteFlag, 0x7ef368, 0x40);
        UpdateItem(ItemType.MapSW, ItemSnesMemoryType.ByteFlag, 0x7ef368, 0x80);
        UpdateItem(ItemType.MapMM, ItemSnesMemoryType.ByteFlag, 0x7ef369, 0x01);
        UpdateItem(ItemType.MapPD, ItemSnesMemoryType.ByteFlag, 0x7ef369, 0x02);
        UpdateItem(ItemType.MapSP, ItemSnesMemoryType.ByteFlag, 0x7ef369, 0x04);
        UpdateItem(ItemType.MapDP, ItemSnesMemoryType.ByteFlag, 0x7ef369, 0x10);
        UpdateItem(ItemType.MapEP, ItemSnesMemoryType.ByteFlag, 0x7ef369, 0x20);
        UpdateItem(ItemType.MapHC, ItemSnesMemoryType.HyruleCastleMap, 0x7ef369);

        UpdateItem(ItemType.HeartContainer, ItemSnesMemoryType.HeartContainers, 0x7ef36C);
        UpdateItem(ItemType.HeartPiece, ItemSnesMemoryType.Byte, 0x7ef36B);
        UpdateItem(ItemType.ThreeHundredRupees, ItemSnesMemoryType.Rupees, 0x7ef362);

        return;

        void UpdateItem(ItemType item, ItemSnesMemoryType memoryType, int memoryLocation, int flagPosition = 0)
        {
            tracker.UpdateItemFromSnesMemory(item, memoryType, data, prevData, offset + memoryLocation, flagPosition);
        }
    }

    private void ManageZeldaKeys(SnesData data, SnesData prevData, int offset)
    {
        var tracker = Tracker.ItemTracker;

        UpdateItem(ItemType.KeyHC, ItemSnesMemoryType.Sum2Bytes, 0x7ef4E0);
        UpdateItem(ItemType.KeyDP, ItemSnesMemoryType.Byte, 0x7ef4E3);
        UpdateItem(ItemType.KeyCT, ItemSnesMemoryType.Byte, 0x7ef4E4);
        UpdateItem(ItemType.KeySP, ItemSnesMemoryType.Byte, 0x7ef4E5);
        UpdateItem(ItemType.KeyPD, ItemSnesMemoryType.Byte, 0x7ef4E6);
        UpdateItem(ItemType.KeyMM, ItemSnesMemoryType.Byte, 0x7ef4E7);
        UpdateItem(ItemType.KeySW, ItemSnesMemoryType.Byte, 0x7ef4E8);
        UpdateItem(ItemType.KeyIP, ItemSnesMemoryType.Byte, 0x7ef4E9);
        UpdateItem(ItemType.KeyTH, ItemSnesMemoryType.Byte, 0x7ef4EA);
        UpdateItem(ItemType.KeyTT, ItemSnesMemoryType.Byte, 0x7ef4EB);
        UpdateItem(ItemType.KeyTR, ItemSnesMemoryType.Byte, 0x7ef4EC);
        UpdateItem(ItemType.KeyGT, ItemSnesMemoryType.Byte, 0x7ef4ED);

        return;

        void UpdateItem(ItemType item, ItemSnesMemoryType memoryType, int memoryLocation, int flagPosition = 0)
        {
            tracker.UpdateItemFromSnesMemory(item, memoryType, data, prevData, offset + memoryLocation, flagPosition);
        }
    }
}
