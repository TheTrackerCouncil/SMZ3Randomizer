using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class MetroidStateChecks(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<MetroidStateChecks> logger, IEnumerable<IMetroidStateCheck> metroidStateChecks) : AutoTrackerModule(tracker, snesConnector, logger)
{
    private DateTime? previousInventoryCheckTime;
    private SnesData? previousInventorySnesData;

    public override void Initialize()
    {
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7e0750,
            Length = 0x400,
            OnResponse = CheckMetroidState,
            Filter = () => IsInMetroid
        });

        if (Tracker.Options.AutoTrackingMode == AutoTrackingMode.Inventory)
        {
            SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0xa17902,
                Length = 0x80,
                FrequencySeconds = 2,
                OnResponse = CheckMetroidInventoryFromZelda,
                Filter = () =>
                {
                    if (!IsInMetroid)
                    {
                        previousInventoryCheckTime = null;
                        previousInventorySnesData = null;
                    }

                    return IsInZelda;
                }
            });
        }
    }

    private void CheckMetroidState(SnesData data, SnesData? prevData)
    {
        var prevState = AutoTracker.MetroidState;
        var MetroidState = AutoTracker.MetroidState = new AutoTrackerMetroidState(data);
        Logger.LogDebug("{StateDetails}", MetroidState.ToString());
        if (prevState == null) return;

        // If the game hasn't booted up, wait until we find valid data in the Metroid state before we start
        // checking locations
        if (AutoTracker.HasValidState != MetroidState.IsValid)
        {
            AutoTracker.UpdateValidState(MetroidState.IsValid);
            if (MetroidState.IsValid)
            {
                Logger.LogInformation("Valid game state detected");
            }
        }

        if (!MetroidState.IsValid) return;

        foreach (var check in metroidStateChecks)
        {
            if (check.ExecuteCheck(Tracker, MetroidState, prevState))
            {
                Logger.LogInformation("{StateName} detected", check.GetType().Name);
            }
        }

        if (Tracker.Options.AutoTrackingMode == AutoTrackingMode.Inventory && prevData != null)
        {
            // Because state checks are so frequent, it can lead to invalid tracking, so throttle
            // the inventory checks to only be every second
            if (previousInventoryCheckTime == null || previousInventorySnesData == null)
            {
                previousInventorySnesData = data;
                previousInventoryCheckTime = DateTime.Now;
                return;
            }
            else if ((DateTime.Now - previousInventoryCheckTime.Value).TotalSeconds < 1)
            {
                return;
            }

            ManageMetroidInventory(data, previousInventorySnesData, -0x7e0750);
            previousInventoryCheckTime = DateTime.Now;
            previousInventorySnesData = data;
        }
    }

    private void CheckMetroidInventoryFromZelda(SnesData data, SnesData? prevData)
    {
        if (!HasValidState || prevData == null) return;
        ManageMetroidInventory(data, prevData, -0x7e09A4);
    }

    private void ManageMetroidInventory(SnesData data, SnesData prevData, int offset)
    {
        var tracker = Tracker.ItemTracker;
        UpdateItem(ItemType.Varia, ItemSnesMemoryType.ByteFlag, 0x7E09A4, 0x01);
        UpdateItem(ItemType.SpringBall, ItemSnesMemoryType.ByteFlag, 0x7E09A4, 0x02);
        UpdateItem(ItemType.Morph, ItemSnesMemoryType.ByteFlag, 0x7E09A4, 0x04);
        UpdateItem(ItemType.ScrewAttack, ItemSnesMemoryType.ByteFlag, 0x7E09A4, 0x08);
        UpdateItem(ItemType.Gravity, ItemSnesMemoryType.ByteFlag, 0x7E09A4, 0x20);

        UpdateItem(ItemType.HiJump, ItemSnesMemoryType.ByteFlag, 0x7E09A5, 0x01);
        UpdateItem(ItemType.SpaceJump, ItemSnesMemoryType.ByteFlag, 0x7E09A5, 0x02);
        UpdateItem(ItemType.Bombs, ItemSnesMemoryType.ByteFlag, 0x7E09A5, 0x10);
        UpdateItem(ItemType.SpeedBooster, ItemSnesMemoryType.ByteFlag, 0x7E09A5, 0x20);
        UpdateItem(ItemType.Grapple, ItemSnesMemoryType.ByteFlag, 0x7E09A5, 0x40);
        UpdateItem(ItemType.XRay, ItemSnesMemoryType.ByteFlag, 0x7E09A5, 0x80);

        UpdateItem(ItemType.Wave, ItemSnesMemoryType.ByteFlag, 0x7E09A8, 0x01);
        UpdateItem(ItemType.Ice, ItemSnesMemoryType.ByteFlag, 0x7E09A8, 0x02);
        UpdateItem(ItemType.Spazer, ItemSnesMemoryType.ByteFlag, 0x7E09A8, 0x04);
        UpdateItem(ItemType.Plasma, ItemSnesMemoryType.ByteFlag, 0x7E09A8, 0x08);
        UpdateItem(ItemType.Charge, ItemSnesMemoryType.ByteFlag, 0x7E09A9, 0x10);

        UpdateItem(ItemType.ETank, ItemSnesMemoryType.WordEnergy, 0x7e09c4);
        UpdateItem(ItemType.Missile, ItemSnesMemoryType.WordAmmo, 0x7e09c8);
        UpdateItem(ItemType.Super, ItemSnesMemoryType.WordAmmo, 0x7e09cc);
        UpdateItem(ItemType.PowerBomb, ItemSnesMemoryType.WordAmmo, 0x7e09d0);
        UpdateItem(ItemType.ReserveTank, ItemSnesMemoryType.WordReserves, 0x7E09D4);

        return;

        void UpdateItem(ItemType item, ItemSnesMemoryType memoryType, int memoryLocation, int flagPosition = 0)
        {
            tracker.UpdateItemFromSnesMemory(item, memoryType, data, prevData, offset + memoryLocation, flagPosition);
        }
    }
}
