using System.Linq;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;

namespace Randomizer.SMZ3.Tracking.AutoTracking.AutoTrackerModules;

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

        if (prevData.ReadUInt8(0x2) == 0 && data.ReadUInt8(0x2) > 0)
        {
            if (IsInZelda)
            {
                var gt = Tracker.World.GanonsTower;
                if (gt.DungeonState.Cleared == false)
                {
                    Logger.LogInformation("Auto tracked Ganon's Tower");
                    Tracker.MarkDungeonAsCleared(gt, confidence: null, autoTracked: true);
                    didUpdate = true;
                }
            }
            else if (IsInMetroid)
            {
                var motherBrain = Tracker.World.AllBosses.First(x => x.Name == "Mother Brain");
                if (motherBrain.State.Defeated != true)
                {
                    Logger.LogInformation("Auto tracked Mother Brain");
                    Tracker.MarkBossAsDefeated(motherBrain, admittedGuilt: true, confidence: null, autoTracked: true);
                    didUpdate = true;
                }
            }
        }

        if (prevData.ReadUInt8(0x106) == 0 && data.ReadUInt8(0x106) > 0)
        {
            if (IsInZelda)
            {
                var gt = Tracker.World.GanonsTower;
                if (gt.DungeonState.Cleared == false)
                {
                    Logger.LogInformation("Auto tracked Ganon's Tower");
                    Tracker.MarkDungeonAsCleared(gt, confidence: null, autoTracked: true);
                    didUpdate = true;
                }
            }
            else if (IsInMetroid)
            {
                var motherBrain = Tracker.World.AllBosses.First(x => x.Name == "Mother Brain");
                if (motherBrain.State.Defeated != true)
                {
                    Logger.LogInformation("Auto tracked Mother Brain");
                    Tracker.MarkBossAsDefeated(motherBrain, admittedGuilt: true, confidence: null, autoTracked: true);
                    didUpdate = true;
                }
            }
        }

        if (didUpdate && data.ReadUInt8(0x2) > 0 && data.ReadUInt8(0x106) > 0)
        {
            AutoTracker.HasDefeatedBothBosses = true;
        }

    }
}
