using System.Linq;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;

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
            Length = 0x08,
            FrequencySeconds = 1,
            OnResponse = CheckMetroidBosses,
            Filter = () => IsInMetroid
        });
    }

    private void CheckMetroidBosses(SnesData data, SnesData? prevData)
    {
        if (prevData == null)
        {
            return;
        }

        foreach (var boss in Tracker.World.AllBosses.Where(x => x.Metadata.MemoryAddress != null && x.Metadata.MemoryFlag > 0 && !x.State.AutoTracked))
        {
            if (data.CheckUInt8Flag(boss.Metadata.MemoryAddress ?? 0, boss.Metadata.MemoryFlag ?? 100))
            {
                boss.State.AutoTracked = true;
                Tracker.MarkBossAsDefeated(boss, true, null, true);
                Logger.LogInformation("Auto tracked {BossName} as defeated", boss.Name);
            }
        }
    }
}
