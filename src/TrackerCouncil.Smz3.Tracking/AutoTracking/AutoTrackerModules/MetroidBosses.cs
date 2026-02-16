using System.Linq;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class MetroidBosses(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<MetroidBosses> logger) : AutoTrackerModule(tracker, snesConnector, logger)
{
    public override void Initialize()
    {
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7ed828,
            Length = 0x10,
            FrequencySeconds = 1,
            OnResponse = CheckMetroidBosses,
            Filter = () => IsInMetroid
        });

        if (Tracker.Options.AutoTrackingMode is AutoTrackingMode.Inventory)
        {
            SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0xA16078,
                Length = 0x08,
                FrequencySeconds = 4,
                OnResponse = ManageBosses,
                Filter = () => IsInZelda
            });

            if (Tracker.World.Config.MetroidKeysanity && Tracker.Options.AutoTrackingMode == AutoTrackingMode.Inventory)
            {
                SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
                {
                    MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                    SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
                    AddressFormat = AddressFormat.Snes9x,
                    SniMemoryMapping = MemoryMapping.ExHiRom,
                    Address = 0xa17970,
                    Length = 0x02,
                    FrequencySeconds = 2,
                    OnResponse = CheckMetroidKeycardsFromZelda,
                    Filter = () => IsInZelda
                });
            }
        }
    }

    private void CheckMetroidBosses(SnesData data, SnesData? prevData)
    {
        if (prevData == null || !HasValidState)
        {
            return;
        }

        ManageBosses(data, prevData);

        if (Tracker.Options.AutoTrackingMode == AutoTrackingMode.Inventory && Tracker.World.Config.MetroidKeysanity)
        {
            ManageKeycards(data, prevData, -0x7ed828);
        }
    }

    private void CheckMetroidKeycardsFromZelda(SnesData data, SnesData? prevData)
    {
        if (prevData == null || !HasValidState)
        {
            return;
        }

        ManageKeycards(data, prevData, -0x7ed830);
    }

    private void ManageBosses(SnesData data, SnesData? prevData)
    {
        if (prevData == null || !HasValidState)
        {
            return;
        }

        foreach (var boss in Tracker.World.AllBosses.Where(x => x.Metadata.MemoryAddress != null && x.Metadata.MemoryFlag > 0 && !x.State.AutoTracked))
        {
            if (data.CheckUInt8Flag(boss.Metadata.MemoryAddress ?? 0, boss.Metadata.MemoryFlag ?? 100))
            {
                Tracker.BossTracker.MarkBossAsDefeated(boss, true, null, true);
                Logger.LogInformation("Auto tracked {BossName} as defeated", boss.Name);
            }
        }
    }

    private void ManageKeycards(SnesData data, SnesData prevData, int offset)
    {
        var tracker = Tracker.ItemTracker;

        UpdateItem(ItemType.CardCrateriaL1, ItemSnesMemoryType.ByteFlag, 0x7ed830, 0x01);
        UpdateItem(ItemType.CardCrateriaL2, ItemSnesMemoryType.ByteFlag, 0x7ed830, 0x02);
        UpdateItem(ItemType.CardCrateriaBoss, ItemSnesMemoryType.ByteFlag, 0x7ed830, 0x04);
        UpdateItem(ItemType.CardBrinstarL1, ItemSnesMemoryType.ByteFlag, 0x7ed830, 0x08);
        UpdateItem(ItemType.CardBrinstarL2, ItemSnesMemoryType.ByteFlag, 0x7ed830, 0x10);
        UpdateItem(ItemType.CardBrinstarBoss, ItemSnesMemoryType.ByteFlag, 0x7ed830, 0x20);
        UpdateItem(ItemType.CardNorfairL1, ItemSnesMemoryType.ByteFlag, 0x7ed830, 0x40);
        UpdateItem(ItemType.CardNorfairL2, ItemSnesMemoryType.ByteFlag, 0x7ed830, 0x80);
        UpdateItem(ItemType.CardNorfairBoss, ItemSnesMemoryType.ByteFlag, 0x7ed831, 0x01);
        UpdateItem(ItemType.CardMaridiaL1, ItemSnesMemoryType.ByteFlag, 0x7ed831, 0x02);
        UpdateItem(ItemType.CardMaridiaL2, ItemSnesMemoryType.ByteFlag, 0x7ed831, 0x04);
        UpdateItem(ItemType.CardMaridiaBoss, ItemSnesMemoryType.ByteFlag, 0x7ed831, 0x08);
        UpdateItem(ItemType.CardWreckedShipL1, ItemSnesMemoryType.ByteFlag, 0x7ed831, 0x10);
        UpdateItem(ItemType.CardWreckedShipBoss, ItemSnesMemoryType.ByteFlag, 0x7ed831, 0x20);
        UpdateItem(ItemType.CardLowerNorfairL1, ItemSnesMemoryType.ByteFlag, 0x7ed831, 0x40);
        UpdateItem(ItemType.CardLowerNorfairBoss, ItemSnesMemoryType.ByteFlag, 0x7ed831, 0x80);

        return;

        void UpdateItem(ItemType item, ItemSnesMemoryType memoryType, int memoryLocation, int flagPosition = 0)
        {
            tracker.UpdateItemFromSnesMemory(item, memoryType, data, prevData, offset + memoryLocation, flagPosition);
        }
    }
}
