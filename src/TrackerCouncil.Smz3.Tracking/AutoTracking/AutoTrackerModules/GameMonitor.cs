using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class GameMonitor(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<GameMonitor> logger, IWorldService worldService) : AutoTrackerModule(tracker, snesConnector, logger)
{
    public override void Initialize()
    {
        // Check if the game has started or not
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7e0020,
            Length = 1,
            FrequencySeconds = 0.5,
            OnResponse = GameStart,
            Filter = () => !HasStartedGame
        });

        // Check if the player is in Zelda or Metroid
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA173FE,
            Length = 2,
            OnResponse = CheckGame,
            Filter = () => HasStartedGame
        });
    }

    private void GameStart(SnesData data, SnesData? prevData)
    {
        var value = data.ReadUInt8(0);
        if (value != 0 && prevData?.ReadUInt8(0) != 0 && !HasStartedGame)
        {
            Logger.LogInformation("Game started");
            AutoTracker.HasStarted = true;

            if (Tracker.World.Config.MultiWorld && worldService.Worlds.Count > 1)
            {
                var worldCount = worldService.Worlds.Count;
                var otherPlayerName = worldService.Worlds.Where(x => x != worldService.World).Random(new Random())!.Config.PhoneticName;
                Tracker.Say(x => x.AutoTracker.GameStartedMultiplayer, args: [worldCount, otherPlayerName]);
            }
            else
            {
                Tracker.Say(x => x.AutoTracker.GameStarted, args: [Tracker.Rom?.Seed]);
            }
        }
    }

    private void CheckGame(SnesData data, SnesData? prevData)
    {
        var game = Game.Neither;
        var value = data.ReadUInt8(0);
        if (value == 0x00)
        {
            game = Game.Zelda;
            AutoTracker.UpdateValidState(true);
        }
        else if (value == 0xFF)
        {
            game = Game.SM;
        }
        else if (value == 0x11)
        {
            game = Game.Credits;
            Tracker.UpdateTrackNumber(99);
        }

        if (game != AutoTracker.CurrentGame)
        {
            AutoTracker.UpdateGame(game);
            Logger.LogInformation("Game changed to: {CurrentGame}", game);
        }
    }
}
