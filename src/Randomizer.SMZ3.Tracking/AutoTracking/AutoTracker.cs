using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks;
using Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.Shared.Enums;

namespace Randomizer.SMZ3.Tracking.AutoTracking;

/// <summary>
/// Manages the automated checking of the emulator memory for purposes of auto tracking
/// and other things using the appropriate connector (USB2SNES or Lura) based on user
/// preferences.
/// </summary>
public class AutoTracker : AutoTrackerBase
{
    private readonly ILogger<AutoTracker> _logger;
    private readonly List<EmulatorAction> _readActions = new();
    private readonly Dictionary<int, EmulatorAction> _readActionMap = new();
    private readonly ILoggerFactory _loggerFactory;
    private readonly IItemService _itemService;
    private readonly IEnumerable<IZeldaStateCheck?> _zeldaStateChecks;
    private readonly IEnumerable<IMetroidStateCheck?> _metroidStateChecks;
    private readonly TrackerModuleFactory _trackerModuleFactory;
    private readonly IRandomizerConfigService _config;
    private readonly IWorldService _worldService;
    private readonly IGameModeService _gameModeService;
    private int _currentIndex;
    private Game _previousGame;
    private bool _hasStarted;
    private bool _hasValidState;
    private IEmulatorConnector? _connector;
    private readonly Queue<EmulatorAction> _sendActions = new();
    private CancellationTokenSource? _stopSendingMessages;
    private int _numGTItems;
    private bool _seenGTTorch;
    private bool _foundGTKey;
    private bool _beatBothBosses;
    private string? _previousRom;

    /// <summary>
    /// Constructor for Auto Tracker
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="itemService"></param>
    /// <param name="zeldaStateChecks"></param>
    /// <param name="metroidStateChecks"></param>
    /// <param name="trackerModuleFactory"></param>
    /// <param name="randomizerConfigService"></param>
    /// <param name="worldService"></param>
    /// <param name="trackerBase"></param>
    /// <param name="gameModeService"></param>
    public AutoTracker(ILogger<AutoTracker> logger,
        ILoggerFactory loggerFactory,
        IItemService itemService,
        IEnumerable<IZeldaStateCheck> zeldaStateChecks,
        IEnumerable<IMetroidStateCheck> metroidStateChecks,
        TrackerModuleFactory trackerModuleFactory,
        IRandomizerConfigService randomizerConfigService,
        IWorldService worldService,
        TrackerBase trackerBase,
        IGameModeService gameModeService)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _itemService = itemService;
        _trackerModuleFactory = trackerModuleFactory;
        _config = randomizerConfigService;
        _worldService = worldService;
        _gameModeService = gameModeService;
        TrackerBase = trackerBase;

        // Check if the game has started or not
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7e0020,
            Length = 0x1,
            Game = Game.Neither,
            Action = CheckStarted
        });

        // Check whether the player is in Zelda or Metroid
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.CartRAM,
            Address = 0xA173FE,
            Length = 0x2,
            Game = Game.Both,
            Action = CheckGame
        });

        // Check Zelda rooms (caves, houses, dungeons)
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7ef000,
            Length = 0x250,
            Game = Game.Zelda,
            FrequencySeconds = 1,
            Action = CheckZeldaRooms
        });

        // Check Zelda overworld and NPC locations and inventory
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7ef280,
            Length = 0x200,
            Game = Game.Zelda,
            FrequencySeconds = 1,
            Action = CheckZeldaMisc
        });

        // Check Zelda state
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7e0000,
            Length = 0x250,
            Game = Game.Zelda,
            Action = CheckZeldaState
        });

        // Check if Ganon is defeated
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.CartRAM,
            Address = 0xA17400,
            Length = 0x120,
            Game = Game.Both,
            FrequencySeconds = 1,
            Action = CheckBeatFinalBosses
        });

        // Check Metroid locations
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7ed870,
            Length = 0x20,
            Game = Game.SM,
            FrequencySeconds = 1,
            Action = CheckMetroidLocations
        });

        // Check Metroid bosses
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7ed828,
            Length = 0x08,
            Game = Game.SM,
            FrequencySeconds = 1,
            Action = CheckMetroidBosses
        });

        // Check state of if the player has entered the ship
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7e0FB2,
            Length = 0x2,
            Game = Game.SM,
            Action = CheckShip
        });

        // Check Metroid state
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7e0750,
            Length = 0x400,
            Game = Game.SM,
            Action = CheckMetroidState
        });

        // Check for current Zelda song
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7E010B,
            Length = 0x01,
            Game = Game.Zelda,
            FrequencySeconds = 2,
            Action = action => TrackerBase.UpdateTrackNumber(action.CurrentData!.ReadUInt8(0))
        });

        // Check for current Metroid song
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7E0332,
            Length = 0x01,
            Game = Game.SM,
            FrequencySeconds = 2,
            Action = action => TrackerBase.UpdateTrackNumber(action.CurrentData!.ReadUInt8(0))
        });

        // Check for current title screen song
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.WRAM,
            Address = 0x7E0331,
            Length = 0x02,
            Game = Game.Neither,
            FrequencySeconds = 2,
            Action = action => TrackerBase.UpdateTrackNumber(action.CurrentData!.ReadUInt8(1))
        });

        // Get the number of items given to the player via the interactor
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.CartRAM,
            Address = 0xA26000,
            Length = 0x300,
            Game = Game.Both,
            FrequencySeconds = 30,
            Action = action =>
            {
                TrackerBase.GameService?.SyncItems(action);
            }
        });

        // Get the number of items given to the player via the interactor
        AddReadAction(new()
        {
            Type = EmulatorActionType.ReadBlock,
            Domain = MemoryDomain.CartRAM,
            Address = 0xA26300,
            Length = 0x300,
            Game = Game.Both,
            FrequencySeconds = 30,
            Action = action =>
            {
                TrackerBase.GameService?.SyncItems(action);
            }
        });

        _zeldaStateChecks = zeldaStateChecks;
        _metroidStateChecks = metroidStateChecks;
        _logger.LogInformation("Zelda state checks: {ZeldaStateCount}", _zeldaStateChecks.Count());
        _logger.LogInformation("Metroid state checks: {MetroidStateCount}", _metroidStateChecks.Count());
    }

    /// <summary>
    /// Disables the current connector and creates the requested type
    /// </summary>
    public override void SetConnector(EmulatorConnectorType type, string? qusb2SnesIp)
    {
        if (_connector != null)
        {
            _connector.Dispose();
            _connector = null;
        }

        if (type != EmulatorConnectorType.None)
        {
            if (type == EmulatorConnectorType.USB2SNES)
            {
                _connector = new USB2SNESConnector(_loggerFactory.CreateLogger<USB2SNESConnector>(), qusb2SnesIp);
            }
            else
            {
                _connector = new LuaConnector(_loggerFactory.CreateLogger<LuaConnector>());
            }
            ConnectorType = type;
            _connector.OnConnected += Connector_Connected;
            _connector.OnDisconnected += Connector_Disconnected;
            _connector.MessageReceived += Connector_MessageReceived;
            OnAutoTrackerEnabled();
        }
        else
        {
            ConnectorType = EmulatorConnectorType.None;
            OnAutoTrackerDisabled();
        }
    }

    /// <summary>
    /// If a connector is currently enabled
    /// </summary>
    public override bool IsEnabled => _connector != null;

    /// <summary>
    /// If a connector is currently connected to the emulator
    /// </summary>
    public override bool IsConnected => _connector != null && _connector.IsConnected();

    /// <summary>
    /// If a connector is currently connected to the emulator and a valid game state is detected
    /// </summary>
    public override bool HasValidState => IsConnected && _hasValidState;

    /// <summary>
    /// If the user is activately in an SMZ3 rom
    /// </summary>
    public override bool IsInSMZ3 => string.IsNullOrEmpty(_previousRom) || _previousRom.StartsWith("SMZ3 Cas");

    /// <summary>
    /// Called when the connector successfully established a connection with the emulator
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Connector_Connected(object? sender, EventArgs e)
    {
        _logger.LogInformation("Connector Connected");
        await Task.Delay(TimeSpan.FromSeconds(0.1f));
        if (!IsSendingMessages)
        {
            _logger.LogInformation("Start sending messages");
            TrackerBase.Say(x => x.AutoTracker.WhenConnected);
            OnAutoTrackerConnected();
            _stopSendingMessages = new CancellationTokenSource();
            _ = SendMessagesAsync(_stopSendingMessages.Token);
            _currentIndex = 0;
        }
    }

    /// <summary>
    /// Writes a particular action to the emulator memory
    /// </summary>
    /// <param name="action">The action to write to memory</param>
    public override void WriteToMemory(EmulatorAction action)
    {
        _sendActions.Enqueue(action);
    }

    /// <summary>
    /// Called when a connector has temporarily lost connection with the emulator
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Connector_Disconnected(object? sender, EventArgs e)
    {
        TrackerBase.Say(x => x.AutoTracker.WhenDisconnected);
        _logger.LogInformation("Disconnected");
        OnAutoTrackerDisconnected();
        _stopSendingMessages?.Cancel();

        // Reset everything once
        IsSendingMessages = false;
        foreach (var action in _readActions)
        {
            action.ClearData();
        }
        _sendActions.Clear();
        CurrentGame = Game.Neither;
        _hasValidState = false;
    }

    /// <summary>
    /// The connector has received memory from the emulator
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Connector_MessageReceived(object? sender, EmulatorDataReceivedEventArgs e)
    {
        // If the user is playing SMZ3 (if we don't have the name, assume that they are)
        if (string.IsNullOrEmpty(e.RomName) || e.RomName.StartsWith("SMZ3 Cas"))
        {
            if (!string.IsNullOrEmpty(_previousRom) && e.RomName != _previousRom)
            {
                _logger.LogInformation("Changed to SMZ3 rom {RomName} ({RomHash})", e.RomName,e.RomHash);
                TrackerBase.Say(x => x.AutoTracker.SwitchedToSMZ3Rom);
            }

            // Verify that message we received is still valid, then execute
            if (_readActionMap[e.Address].ShouldProcess(CurrentGame, _hasStarted))
            {
                _readActionMap[e.Address].Invoke(e.Data);
            }
        }
        // If the user is switching to a non-SMZ3 rom
        else if (!string.IsNullOrEmpty(e.RomName) && e.RomName != _previousRom)
        {
            _logger.LogInformation("Ignoring rom {RomName} ({RomHash})", e.RomName,e.RomHash);

            var key = "Unknown";
            if (TrackerBase.Responses.AutoTracker.SwitchedToOtherRom.ContainsKey(e.RomHash!))
            {
                key = e.RomHash!;
            }

            TrackerBase.Say(x => x.AutoTracker.SwitchedToOtherRom[key]);
        }

        _previousRom = e.RomName;
    }

    /// <summary>
    /// Sends requests out to the connected lua script
    /// </summary>
    private async Task SendMessagesAsync(CancellationToken cancellationToken)
    {
        Thread.CurrentThread.Name = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        _logger.LogInformation("Start sending messages {ThreadName}", Thread.CurrentThread.Name);
        IsSendingMessages = true;
        while (_connector != null && _connector.IsConnected() && !cancellationToken.IsCancellationRequested)
        {
            if (_connector.CanSendMessage())
            {
                if (_sendActions.Count > 0)
                {
                    var nextAction = _sendActions.Dequeue();

                    if (nextAction.ShouldProcess(CurrentGame, _hasStarted))
                    {
                        _connector.SendMessage(nextAction);
                    }
                }
                else
                {
                    while (!_readActions[_currentIndex].ShouldProcess(CurrentGame, _hasStarted))
                    {
                        _currentIndex = (_currentIndex + 1) % _readActions.Count;
                    }
                    _connector.SendMessage(_readActions[_currentIndex]);
                    _currentIndex = (_currentIndex + 1) % _readActions.Count;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken);
        }
        IsSendingMessages = false;
        _logger.LogInformation("Stop sending messages {ThreadName}", Thread.CurrentThread.Name);
    }

    /// <summary>
    /// Adds a read action to repeatedly call out to the emulator
    /// </summary>
    /// <param name="action"></param>
    private void AddReadAction(EmulatorAction action)
    {
        _readActions.Add(action);
        _readActionMap.Add(action.Address, action);
    }

    /// <summary>
    /// Check if the player has started playing the game
    /// </summary>
    /// <param name="action"></param>
    private void CheckStarted(EmulatorAction action)
    {
        if (action.CurrentData == null) return;
        var value = action.CurrentData.ReadUInt8(0);
        if (value != 0 && !_hasStarted)
        {
            _logger.LogInformation("Game started");
            _hasStarted = true;

            if (TrackerBase.World.Config.MultiWorld && _worldService.Worlds.Count > 1)
            {
                var worldCount = _worldService.Worlds.Count;
                var otherPlayerName = _worldService.Worlds.Where(x => x != _worldService.World).Random(new Random())!.Config.PhoneticName;
                TrackerBase.Say(x => x.AutoTracker.GameStartedMultiplayer, worldCount, otherPlayerName);
            }
            else
            {
                TrackerBase.Say(x => x.AutoTracker.GameStarted, TrackerBase.Rom?.Seed);
            }
        }
    }

    /// <summary>
    /// Checks which game the player is currently in
    /// </summary>
    /// <param name="action"></param>
    private void CheckGame(EmulatorAction action)
    {
        if (action.CurrentData == null) return;
        _previousGame = CurrentGame;
        var value = action.CurrentData.ReadUInt8(0);
        if (value == 0x00)
        {
            CurrentGame = Game.Zelda;
            _hasValidState = true;
        }
        else if (value == 0xFF)
        {
            CurrentGame = Game.SM;
        }
        else if (value == 0x11)
        {
            CurrentGame = Game.Credits;
            TrackerBase.UpdateTrackNumber(99);
        }
        if (_previousGame != CurrentGame)
        {
            _logger.LogInformation("Game changed to: {CurrentGame}", CurrentGame);
        }
    }

    /// <summary>
    /// Checks if the player has cleared Zelda room locations (cave, houses, dungeons)
    /// This also checks if the player has gotten the dungeon rewards
    /// </summary>
    /// <param name="action"></param>
    private void CheckZeldaRooms(EmulatorAction action)
    {
        if (!_hasValidState) return;
        if (action.CurrentData == null || action.PreviousData == null) return;
        CheckLocations(action, LocationMemoryType.Default, true, Game.Zelda);
        CheckDungeons(action.CurrentData, action.PreviousData);
    }

    /// <summary>
    /// Checks if the player has cleared misc Zelda locations (overworld, NPCs)
    /// Also where you can check inventory (inventory starts at an offset of 0x80)
    /// </summary>
    /// <param name="action"></param>
    private void CheckZeldaMisc(EmulatorAction action)
    {
        if (!_hasValidState) return;
        if (action.CurrentData == null || action.PreviousData == null) return;
        // Failsafe to prevent incorrect checking
        if (action.CurrentData?.ReadUInt8(0x190) == 0xFF && action.CurrentData?.ReadUInt8(0x191) == 0xFF)
        {
            _logger.LogInformation("Ignoring due to transition");
            return;
        }

        CheckLocations(action, LocationMemoryType.ZeldaMisc, false, Game.Zelda);

        PlayerHasFairy = false;
        for (var i = 0; i < 4; i++)
        {
            PlayerHasFairy |= action.CurrentData?.ReadUInt8(0xDC + i) == 6;
        }

        // Activated flute
        if (action.CurrentData?.CheckBinary8Bit(0x10C, 0x01) == true && action.PreviousData?.CheckBinary8Bit(0x10C, 0x01) != true)
        {
            var duckItem = _itemService.FirstOrDefault("Duck");
            if (duckItem?.State.TrackingState == 0)
            {
                TrackerBase.TrackItem(duckItem, null, null, false, true);
            }
        }

        // Check if the player cleared Aga
        if (action.CurrentData?.ReadUInt8(0x145) >= 3)
        {
            var castleTower = TrackerBase.World.CastleTower;
            if (castleTower.DungeonState.Cleared == false)
            {
                TrackerBase.MarkDungeonAsCleared(castleTower, null, autoTracked: true);
                _logger.LogInformation("Auto tracked {Name} as cleared", castleTower.Name);
            }
        }
    }

    /// <summary>
    /// Checks to see if the player has cleared locations in Super Metroid
    /// </summary>
    /// <param name="action"></param>
    private void CheckMetroidLocations(EmulatorAction action)
    {
        if (!_hasValidState) return;
        if (action.CurrentData != null && action.PreviousData != null)
        {
            CheckLocations(action, LocationMemoryType.Default, false, Game.SM);
        }
    }

    /// <summary>
    ///  Checks if the player has defeated Super Metroid bosses
    /// </summary>
    /// <param name="action"></param>
    private void CheckMetroidBosses(EmulatorAction action)
    {
        if (!_hasValidState) return;
        if (action.CurrentData != null && action.PreviousData != null)
        {
            CheckSMBosses(action.CurrentData);
        }
    }

    /// <summary>
    /// Checks locations to see if they have accessed or not
    /// </summary>
    /// <param name="action">The emulator action with the emulator memory data</param>
    /// <param name="type">The type of location to find the correct LocationInfo objects</param>
    /// <param name="is16Bit">Set to true if this is a 16 bit value or false for 8 bit</param>
    /// <param name="game">The game that is being checked</param>
    private void CheckLocations(EmulatorAction action, LocationMemoryType type, bool is16Bit, Game game)
    {
        var currentData = action.CurrentData;
        var prevData = action.PreviousData;

        if (currentData == null || prevData == null) return;

        // Store the locations for this action so that we don't need to grab them each time every half a second or so
        action.Locations ??= _worldService.AllLocations().Where(x =>
            x.MemoryType == type && ((game == Game.SM && (int)x.Id < 256) || (game == Game.Zelda && (int)x.Id >= 256))).ToList();

        foreach (var location in action.Locations)
        {
            try
            {
                var loc = location.MemoryAddress ?? 0;
                var flag = location.MemoryFlag ?? 0;
                var currentCleared = (is16Bit && currentData.CheckUInt16(loc * 2, flag)) || (!is16Bit && currentData.CheckBinary8Bit(loc, flag));
                var prevCleared = (is16Bit && prevData.CheckUInt16(loc * 2, flag)) || (!is16Bit && prevData.CheckBinary8Bit(loc, flag));
                if (location.State.Autotracked == false && currentCleared && prevCleared)
                {
                    // Increment GT guessing game number
                    if (location.Region is GanonsTower gt && location != gt.BobsTorch)
                    {
                        IncrementGTItems(location);
                    }

                    var item = location.Item;
                    location.State.Autotracked = true;
                    TrackerBase.TrackItem(item: item, trackedAs: null, confidence: null, tryClear: true, autoTracked: true, location: location);
                    _logger.LogInformation("Auto tracked {ItemName} from {LocationName}", location.Item.Name, location.Name);

                    // Mark HC as cleared if this was Zelda's Cell
                    if (location.Id == LocationId.HyruleCastleZeldasCell && TrackerBase.World.HyruleCastle.DungeonState.Cleared == false)
                    {
                        TrackerBase.MarkDungeonAsCleared(TrackerBase.World.HyruleCastle, null, autoTracked: true);
                    }
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to auto track location: {LocationName}", location.Name);
                TrackerBase.Error();
            }
        }
    }

    /// <summary>
    /// Checks the status of if dungeons have been cleared
    /// </summary>
    /// <param name="currentData">The latest memory data returned from the emulator</param>
    /// <param name="prevData">The previous memory data returned from the emulator</param>
    private void CheckDungeons(EmulatorMemoryData currentData, EmulatorMemoryData prevData)
    {
        foreach (var dungeon in TrackerBase.World.Dungeons)
        {
            var region = (Z3Region)dungeon;

            // Skip if we don't have any memory addresses saved for this dungeon
            if (region.MemoryAddress == null || region.MemoryFlag == null)
            {
                continue;
            }

            try
            {
                var prevValue = prevData.CheckUInt16((int)(region.MemoryAddress * 2), region.MemoryFlag ?? 0);
                var currentValue = currentData.CheckUInt16((int)(region.MemoryAddress * 2), region.MemoryFlag ?? 0);
                if (dungeon.DungeonState.AutoTracked == false && prevValue && currentValue)
                {
                    dungeon.DungeonState.AutoTracked = true;
                    TrackerBase.MarkDungeonAsCleared(dungeon, autoTracked: true);
                    _logger.LogInformation("Auto tracked {DungeonName} as cleared", dungeon.DungeonName);
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to auto track Dungeon: {DungeonName}", dungeon.DungeonName);
                TrackerBase.Error();
            }
        }
    }

    /// <summary>
    /// Checks the status of if the Super Metroid bosses have been defeated
    /// </summary>
    /// <param name="data">The response from the lua script</param>
    private void CheckSMBosses(EmulatorMemoryData data)
    {
        foreach (var boss in TrackerBase.World.AllBosses.Where(x => x.Metadata.MemoryAddress != null && x.Metadata.MemoryFlag > 0 && !x.State.AutoTracked))
        {
            if (data.CheckBinary8Bit(boss.Metadata.MemoryAddress ?? 0, boss.Metadata.MemoryFlag ?? 100))
            {
                boss.State.AutoTracked = true;
                TrackerBase.MarkBossAsDefeated(boss, true, null, true);
                _logger.LogInformation("Auto tracked {BossName} as defeated", boss.Name);
            }
        }
    }

    /// <summary>
    /// Tracks the current memory state of LttP for Tracker voice lines
    /// </summary>
    /// <param name="action">The message from the emulator with the memory state</param>
    private void CheckZeldaState(EmulatorAction action)
    {
        if (_previousGame != CurrentGame || action.CurrentData == null) return;
        var prevState = ZeldaState;
        ZeldaState = new(action.CurrentData);
        _logger.LogDebug("{StateDetails}", ZeldaState.ToString());
        if (prevState == null) return;

        if (!_seenGTTorch
            && ZeldaState.CurrentRoom == 140
            && !ZeldaState.IsOnBottomHalfOfRoom
            && !ZeldaState.IsOnRightHalfOfRoom
            && ZeldaState.Substate != 14)
        {
            _seenGTTorch = true;
            IncrementGTItems(TrackerBase.World.GanonsTower.BobsTorch);
        }

        // Entered the triforce room
        if (ZeldaState.State == 0x19)
        {
            if (_beatBothBosses)
            {
                TrackerBase.GameBeaten(true);
            }
        }

        foreach (var check in _zeldaStateChecks)
        {
            if (check != null && check.ExecuteCheck(TrackerBase, ZeldaState, prevState))
            {
                _logger.LogInformation("{StateName} detected", check.GetType().Name);
            }
        }

        _gameModeService.ZeldaStateChanged(ZeldaState, prevState);
    }

    /// <summary>
    /// Checks if the final bosses of both games are defeated
    /// It appears as if 0x2 represents the first boss defeated and 0x106 is the second,
    /// no matter what order the bosses were defeated in
    /// </summary>
    /// <param name="action">The message from the emulator with the memory state</param>
    private void CheckBeatFinalBosses(EmulatorAction action)
    {
        if (_previousGame != CurrentGame || action.CurrentData == null) return;
        var didUpdate = false;

        if (action.PreviousData?.ReadUInt8(0x2) == 0 && action.CurrentData.ReadUInt8(0x2) > 0)
        {
            if (CurrentGame == Game.Zelda)
            {
                var gt = TrackerBase.World.GanonsTower;
                if (gt.DungeonState.Cleared == false)
                {
                    _logger.LogInformation("Auto tracked Ganon's Tower");
                    TrackerBase.MarkDungeonAsCleared(gt, confidence: null, autoTracked: true);
                    didUpdate = true;
                }
            }
            else if (CurrentGame == Game.SM)
            {
                var motherBrain = TrackerBase.World.AllBosses.First(x => x.Name == "Mother Brain");
                if (motherBrain.State.Defeated != true)
                {
                    _logger.LogInformation("Auto tracked Mother Brain");
                    TrackerBase.MarkBossAsDefeated(motherBrain, admittedGuilt: true, confidence: null, autoTracked: true);
                    didUpdate = true;
                }
            }
        }

        if (action.PreviousData?.ReadUInt8(0x106) == 0 && action.CurrentData.ReadUInt8(0x106) > 0)
        {
            if (CurrentGame == Game.Zelda)
            {
                var gt = TrackerBase.World.GanonsTower;
                if (gt.DungeonState.Cleared == false)
                {
                    _logger.LogInformation("Auto tracked Ganon's Tower");
                    TrackerBase.MarkDungeonAsCleared(gt, confidence: null, autoTracked: true);
                    didUpdate = true;
                }
            }
            else if (CurrentGame == Game.SM)
            {
                var motherBrain = TrackerBase.World.AllBosses.First(x => x.Name == "Mother Brain");
                if (motherBrain.State.Defeated != true)
                {
                    _logger.LogInformation("Auto tracked Mother Brain");
                    TrackerBase.MarkBossAsDefeated(motherBrain, admittedGuilt: true, confidence: null, autoTracked: true);
                    didUpdate = true;
                }
            }
        }

        if (didUpdate && action.CurrentData.ReadUInt8(0x2) > 0 && action.CurrentData.ReadUInt8(0x106) > 0)
        {
            _beatBothBosses = true;
        }

    }

    /// <summary>
    /// Tracks the current memory state of SM for Tracker voice lines
    /// </summary>
    /// <param name="action">The message from the emulator with the memory state</param>
    private void CheckMetroidState(EmulatorAction action)
    {
        if (_previousGame != CurrentGame || action.CurrentData == null) return;
        var prevState = MetroidState;
        MetroidState = new(action.CurrentData);
        _logger.LogDebug("{StateDetails}", MetroidState.ToString());
        if (prevState == null) return;

        // If the game hasn't booted up, wait until we find valid data in the Metroid state before we start
        // checking locations
        if (_hasValidState != MetroidState.IsValid)
        {
            _hasValidState = MetroidState.IsValid;
            if (_hasValidState)
            {
                _logger.LogInformation("Valid game state detected");
            }
        }

        if (!_hasValidState) return;

        foreach (var check in _metroidStateChecks)
        {
            if (check != null && check.ExecuteCheck(TrackerBase, MetroidState, prevState))
            {
                _logger.LogInformation("{StateName} detected", check.GetType().Name);
            }
        }

        _gameModeService.MetroidStateChanged(MetroidState, prevState);
    }

    /// <summary>
    /// Checks if the player has entered the ship
    /// </summary>
    /// <param name="action"></param>
    private void CheckShip(EmulatorAction action)
    {
        if (_previousGame != CurrentGame || action.CurrentData == null || action.PreviousData == null) return;
        var currentInShip = action.CurrentData.ReadUInt16(0) == 0xAA4F;
        if (currentInShip && _beatBothBosses)
        {
            TrackerBase.GameBeaten(true);
        }
    }

    private void IncrementGTItems(Location location)
    {
        if (_foundGTKey || _config.Config.GameModeConfigs.KeysanityConfig.ZeldaKeysanity) return;

        var chatIntegrationModule = _trackerModuleFactory.GetModule<ChatIntegrationModule>();
        _numGTItems++;
        TrackerBase.Say(_numGTItems.ToString());
        if (location.Item.Type == ItemType.BigKeyGT)
        {
            var responseIndex = 1;
            for (var i = 1; i <= _numGTItems; i++)
            {
                if (TrackerBase.Responses.AutoTracker.GTKeyResponses.ContainsKey(i))
                {
                    responseIndex = i;
                }
            }
            TrackerBase.Say(x => x.AutoTracker.GTKeyResponses[responseIndex], _numGTItems);
            chatIntegrationModule?.GTItemTracked(_numGTItems, true);
            _foundGTKey = true;
        }
        else
        {
            chatIntegrationModule?.GTItemTracked(_numGTItems, false);
        }
    }
}
