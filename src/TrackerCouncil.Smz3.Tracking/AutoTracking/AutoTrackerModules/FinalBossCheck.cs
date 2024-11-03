using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class FinalBossCheck(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<FinalBossCheck> logger) : AutoTrackerModule(tracker, snesConnector, logger)
{
    public override void Initialize()
    {
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA17400,
            Length = 0x120,
            FrequencySeconds = 1,
            OnResponse = CheckBeatFinalBosses,
            Filter = () => IsInGame
        });
    }

    private void CheckBeatFinalBosses(SnesData data, SnesData? prevData)
    {
        if (prevData == null) return;
        var didUpdate = false;

        var hasNewlyDefeatedFinalBoss = (prevData.ReadUInt8(0x2) == 0 && data.ReadUInt8(0x2) > 0) ||
                                   (prevData.ReadUInt8(0x106) == 0 && data.ReadUInt8(0x106) > 0);

        if (!hasNewlyDefeatedFinalBoss)
        {
            return;
        }

        if (IsInZelda)
        {
            var ganon = Tracker.World.GetBossOfType(BossType.Ganon);
            if (!ganon.Defeated)
            {
                Logger.LogInformation("Auto tracked Ganon's Tower");
                Tracker.BossTracker.MarkBossAsDefeated(Tracker.World.GanonsTower, confidence: null, autoTracked: true);
                didUpdate = true;
            }
        }
        else if (IsInMetroid)
        {
            var motherBrain = Tracker.World.GetBossOfType(BossType.MotherBrain);
            if (!motherBrain.Defeated)
            {
                Logger.LogInformation("Auto tracked Mother Brain");
                var boss = Tracker.World.Bosses.First(x => x.Type == BossType.MotherBrain);
                Tracker.BossTracker.MarkBossAsDefeated(boss, admittedGuilt: true, confidence: null, autoTracked: true);
                _ = CountHyperBeamShots();
                didUpdate = true;
            }
        }

        if (didUpdate && data.ReadUInt8(0x2) > 0 && data.ReadUInt8(0x106) > 0)
        {
            AutoTracker.HasDefeatedBothBosses = true;
        }
    }

    private async Task CountHyperBeamShots()
    {
        try
        {
            var response = await SnesConnector.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7e033e,
                Length = 0x02
            });

            if (!response.Successful || !response.HasData) return;

            var health = response.Data.ReadUInt16(0);

            if (health != null)
            {
                // Each Hyper Beam shot does 1000 damage
                var count = (int)health / 1000;

                if (health % 1000 > 0)
                {
                    count++;
                }

                Logger.LogInformation("Counted {Count} Hyper Beam shot(s) for {Health} health", count, health);
                Tracker.Say(responses: Tracker.Responses.CountHyperBeamShots, tieredKey: count, args: [count]);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when attempting to count the number of hyper beam shots");
        }
    }
}
