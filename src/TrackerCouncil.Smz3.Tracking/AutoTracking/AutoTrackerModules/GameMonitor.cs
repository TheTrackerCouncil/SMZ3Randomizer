using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class GameMonitor(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<GameMonitor> logger, IWorldQueryService worldQueryService) : AutoTrackerModule(tracker, snesConnector, logger)
{
    private bool bIsCheckingGameStart;

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
        if (value != 0 && prevData?.ReadUInt8(0) != 0 && !HasStartedGame && !bIsCheckingGameStart)
        {
            bIsCheckingGameStart = true;
            _ = CheckValidGameStart();
        }
    }

    private async Task CheckValidGameStart()
    {
        var response = await SnesConnector.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA173FE,
            Length = 2
        });

        if (!response.Successful)
        {
            bIsCheckingGameStart = false;
            return;
        }

        var game = GetGame(response.Data);

        if (game is Game.Neither or Game.Credits)
        {
            bIsCheckingGameStart = false;
            return;
        }

        if (game == Game.Zelda)
        {
            response = await SnesConnector.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7e0000,
                Length = 0x250
            });

            if (!response.Successful)
            {
                bIsCheckingGameStart = false;
                return;
            }

            var zeldaState = new AutoTrackerZeldaState(response.Data);
            if (zeldaState.IsValid)
            {
                MarkAsStarted();
            }
            bIsCheckingGameStart = false;
        }
        else if (game == Game.SM)
        {
            response = await SnesConnector.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7e0750,
                Length = 0x400,
            });

            if (!response.Successful)
            {
                bIsCheckingGameStart = false;
                return;
            }

            var metroidState = new AutoTrackerMetroidState(response.Data);
            if (metroidState.IsValid)
            {
                MarkAsStarted();
            }
            bIsCheckingGameStart = false;
        }
    }

    private void MarkAsStarted()
    {
        Logger.LogInformation("Game started");

        AutoTracker.HasStarted = true;

        if (Tracker.World.Config.RomGenerator != RomGenerator.Cas)
        {
            Tracker.Say(x => x.AutoTracker.GameStartedNonCas);
        }
        else if (Tracker.World.Config.MultiWorld && worldQueryService.Worlds.Count > 1)
        {
            var worldCount = worldQueryService.Worlds.Count;
            var otherPlayerName = worldQueryService.Worlds.Where(x => x != worldQueryService.World).Random(new Random())!.Config.PhoneticName;
            Tracker.Say(x => x.AutoTracker.GameStartedMultiplayer, args: [worldCount, otherPlayerName]);
        }
        else
        {
            Tracker.Say(x => x.AutoTracker.GameStarted, args: [Tracker.Rom?.Seed]);
        }
    }

    private void CheckGame(SnesData data, SnesData? prevData)
    {
        var game = GetGame(data);

        if (game != AutoTracker.CurrentGame)
        {
            AutoTracker.UpdateGame(game);
            Logger.LogInformation("Game changed to: {CurrentGame}", game);
        }
    }

    private Game GetGame(SnesData data)
    {
        return data.ReadUInt8(0) switch
        {
            0x00 => Game.Zelda,
            0xFF => Game.SM,
            0x11 => Game.Credits,
            _ => Game.Neither
        };
    }
}
