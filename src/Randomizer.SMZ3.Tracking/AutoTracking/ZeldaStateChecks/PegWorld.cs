using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SNI;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for tracking how many Peg World pegs have been hammered
/// </summary>
public class PegWorld(TrackerBase tracker, ISnesConnectorService snesConnector) : IZeldaStateCheck
{
    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (currentState.OverworldScreen == 0x62)
        {
            CountPegs();
            return true;
        }

        return false;
    }

    private void CountPegs()
    {
        snesConnector.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7e04c8,
            Length = 0x01, // This is actually a four-byte value, but practically, only the lowest byte is important
            OnResponse = (data, _) =>
            {
                var count = data.ReadUInt8(0);

                if (count != null)
                {
                    tracker.SetPegs((int)count);
                }
            }
        });
    }
}
