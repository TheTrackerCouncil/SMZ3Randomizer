using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks;
using Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Manages the automated checking of the emulator memory for purposes of auto tracking
    /// and other things using the appropriate connector (USB2SNES or Lura) based on user
    /// preferences.
    /// </summary>
    public class AutoTracker
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
        private int _currentIndex = 0;
        private Game _previousGame;
        private bool _hasStarted;
        private IEmulatorConnector? _connector;
        private readonly Queue<EmulatorAction> _sendActions = new();
        private CancellationTokenSource? _stopSendingMessages;
        private int _numGTItems = 0;
        private bool _seenGTTorch = false;
        private bool _foundGTKey = false;
        private bool _beatBothBosses = false;
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
        /// <param name="tracker"></param>
        public AutoTracker(ILogger<AutoTracker> logger,
            ILoggerFactory loggerFactory,
            IItemService itemService,
            IEnumerable<IZeldaStateCheck> zeldaStateChecks,
            IEnumerable<IMetroidStateCheck> metroidStateChecks,
            TrackerModuleFactory trackerModuleFactory,
            IRandomizerConfigService randomizerConfigService,
            IWorldService worldService,
            Tracker tracker
        )
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _itemService = itemService;
            _trackerModuleFactory = trackerModuleFactory;
            _config = randomizerConfigService;
            _worldService = worldService;
            Tracker = tracker;

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

            // Get the number of items given to the player via the interactor
            AddReadAction(new()
            {
                Type = EmulatorActionType.ReadBlock,
                Domain = MemoryDomain.CartRAM,
                Address = 0xA26602,
                Length = 0x2,
                Game = Game.Both,
                Action = (EmulatorAction test) =>
                {
                    if (Tracker?.GameService != null)
                    {
                        Tracker.GameService.ItemCounter = test.CurrentData?.ReadUInt16(0) ?? 0;
                    }
                }
            });

            _zeldaStateChecks = zeldaStateChecks;
            _metroidStateChecks = metroidStateChecks;
            _logger.LogInformation($"Zelda state checks: {_zeldaStateChecks.Count()}");
            _logger.LogInformation($"Metroid state checks: {_metroidStateChecks.Count()}");
        }

        /// <summary>
        /// The tracker associated with this auto tracker
        /// </summary>
        public Tracker Tracker { get; set; }

        /// <summary>
        /// The type of connector that the auto tracker is currently using
        /// </summary>
        public EmulatorConnectorType ConnectorType { get; protected set; }

        /// <summary>
        /// The game that the player is currently in
        /// </summary>
        public Game CurrentGame { get; protected set; } = Game.Neither;

        /// <summary>
        /// The latest state that the player in LTTP (location, health, etc.)
        /// </summary>
        public AutoTrackerZeldaState? ZeldaState { get; protected set; }

        /// <summary>
        /// The latest state that the player in Super Metroid (location, health, etc.)
        /// </summary>
        public AutoTrackerMetroidState? MetroidState { get; protected set; }

        /// <summary>
        /// Disables the current connector and creates the requested type
        /// </summary>
        public void SetConnector(EmulatorConnectorType type)
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
                    _connector = new USB2SNESConnector(_loggerFactory.CreateLogger<USB2SNESConnector>());
                }
                else
                {
                    _connector = new LuaConnector(_loggerFactory.CreateLogger<LuaConnector>());
                }
                ConnectorType = type;
                _connector.OnConnected += Connector_Connected;
                _connector.OnDisconnected += Connector_Disconnected;
                _connector.MessageReceived += Connector_MessageReceived;
                AutoTrackerEnabled?.Invoke(this, new());
            }
            else
            {
                ConnectorType = EmulatorConnectorType.None;
                AutoTrackerDisabled?.Invoke(this, new());
            }
        }

        /// <summary>
        /// Occurs when the tracker's auto tracker is enabled
        /// </summary>
        public event EventHandler? AutoTrackerEnabled;

        /// <summary>
        /// Occurs when the tracker's auto tracker is disabled
        /// </summary>
        public event EventHandler? AutoTrackerDisabled;

        /// <summary>
        /// Occurs when the tracker's auto tracker is connected
        /// </summary>
        public event EventHandler? AutoTrackerConnected;

        /// <summary>
        /// Occurs when the tracker's auto tracker is disconnected
        /// </summary>
        public event EventHandler? AutoTrackerDisconnected;

        /// <summary>
        /// The action to run when the player asks Tracker to look at the game
        /// </summary>
        public AutoTrackerViewedAction? LatestViewAction;

        /// <summary>
        /// If a connector is currently enabled
        /// </summary>
        public bool IsEnabled => _connector != null;

        /// <summary>
        /// If a connector is currently connected to the emulator
        /// </summary>
        public bool IsConnected => _connector != null && _connector.IsConnected();

        /// <summary>
        /// If the auto tracker is currently sending messages
        /// </summary>
        public bool IsSendingMessages { get; set; }

        /// <summary>
        /// If the player currently has a fairy
        /// </summary>
        public bool PlayerHasFairy { get; protected set; }

        /// <summary>
        /// If the user is activately in an SMZ3 rom
        /// </summary>
        public bool IsInSMZ3 => string.IsNullOrEmpty(_previousRom) || _previousRom.StartsWith("SMZ3 Cas");

        /// <summary>
        /// Called when the connector successfully established a connection with the emulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void Connector_Connected(object? sender, EventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.1f));
            if (!IsSendingMessages)
            {
                Tracker?.Say(x => x.AutoTracker.WhenConnected);
                AutoTrackerConnected?.Invoke(this, new());
                _stopSendingMessages = new CancellationTokenSource();
                _ = SendMessagesAsync(_stopSendingMessages.Token);
                _currentIndex = 0;
            }
        }

        /// <summary>
        /// Writes a particular action to the emulator memory
        /// </summary>
        /// <param name="action">The action to write to memory</param>
        public void WriteToMemory(EmulatorAction action)
        {
            _sendActions.Enqueue(action);
        }

        /// <summary>
        /// Called when a connector has temporarily lost connection with the emulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connector_Disconnected(object? sender, EventArgs e)
        {
            Tracker?.Say(x => x.AutoTracker.WhenDisconnected);
            _logger.LogInformation("Disconnected");
            AutoTrackerDisconnected?.Invoke(this, new());
            _stopSendingMessages?.Cancel();
            IsSendingMessages = false;
        }

        /// <summary>
        /// The connector has received memory from the emulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connector_MessageReceived(object? sender, EmulatorDataReceivedEventArgs e)
        {
            // If the user is playing SMZ3 (if we don't have the name, assume that they are)
            if (string.IsNullOrEmpty(e.RomName) || e.RomName.StartsWith("SMZ3 Cas"))
            {
                if (!string.IsNullOrEmpty(_previousRom) && e.RomName != _previousRom)
                {
                    _logger.LogInformation($"Changed to SMZ3 Rom {e.RomName} ({e.RomHash})");
                    Tracker.Say(x => x.AutoTracker.SwitchedToSMZ3Rom);
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
                _logger.LogInformation($"Ignoring rom {e.RomName} ({e.RomHash})");

                var key = "Unknown";
                if (Tracker.Responses.AutoTracker.SwitchedToOtherRom.ContainsKey(e.RomHash!))
                {
                    key = e.RomHash!;
                }

                Tracker.Say(x => x.AutoTracker.SwitchedToOtherRom[key]);
            }

            _previousRom = e.RomName;
        }

        /// <summary>
        /// Sends requests out to the connected lua script
        /// </summary>
        protected async Task SendMessagesAsync(CancellationToken cancellationToken)
        {
            Thread.CurrentThread.Name = DateTime.Now.ToString();
            _logger.LogInformation("Start sending messages " + Thread.CurrentThread.Name);
            IsSendingMessages = true;
            while (_connector != null && _connector.IsConnected() && !cancellationToken.IsCancellationRequested)
            {
                if (_connector.CanSendMessage())
                {
                    if (_sendActions.Count > 0)
                    {
                        var nextAction = _sendActions.Dequeue();

                        if (nextAction != null && nextAction.ShouldProcess(CurrentGame, _hasStarted))
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
            _logger.LogInformation("Stop sending messages " + Thread.CurrentThread.Name);
        }

        /// <summary>
        /// Adds a read action to repeatedly call out to the emulator
        /// </summary>
        /// <param name="action"></param>
        protected void AddReadAction(EmulatorAction action)
        {
            _readActions.Add(action);
            _readActionMap.Add(action.Address, action);
        }

        /// <summary>
        /// Check if the player has started playing the game
        /// </summary>
        /// <param name="action"></param>
        protected void CheckStarted(EmulatorAction action)
        {
            if (action.CurrentData == null) return;
            var value = action.CurrentData.ReadUInt8(0);
            if (value != 0 && !_hasStarted)
            {
                _logger.LogInformation("Game started");
                _hasStarted = true;
                Tracker?.Say(x => x.AutoTracker.GameStarted, Tracker.Rom?.Seed);
            }
        }

        /// <summary>
        /// Checks which game the player is currently in
        /// </summary>
        /// <param name="action"></param>
        protected void CheckGame(EmulatorAction action)
        {
            if (action.CurrentData == null) return;
            _previousGame = CurrentGame;
            var value = action.CurrentData.ReadUInt8(0);
            if (value == 0x00)
            {
                CurrentGame = Game.Zelda;
            }
            else if (value == 0xFF)
            {
                CurrentGame = Game.SM;
            }
            if (_previousGame != CurrentGame)
            {
                _logger.LogInformation($"Game changed to: {CurrentGame}");
            }
        }

        /// <summary>
        /// Checks if the player has cleared Zelda room locations (cave, houses, dungeons)
        /// This also checks if the player has gotten the dungeon rewards
        /// </summary>
        /// <param name="action"></param>
        protected void CheckZeldaRooms(EmulatorAction action)
        {
            if (action.CurrentData != null && action.PreviousData != null)
            {
                CheckLocations(action, LocationMemoryType.Default, true, Game.Zelda);
                CheckDungeons(action.CurrentData, action.PreviousData);
            }
        }

        /// <summary>
        /// Checks if the player has cleared misc Zelda locations (overworld, NPCs)
        /// Also where you can check inventory (inventory starts at an offset of 0x80)
        /// </summary>
        /// <param name="action"></param>
        protected void CheckZeldaMisc(EmulatorAction action)
        {
            if (action.CurrentData != null && action.PreviousData != null)
            {
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
                    if (duckItem != null && duckItem.State.TrackingState == 0)
                    {
                        Tracker?.TrackItem(duckItem, null, null, false, true, null, false);
                    }
                }

                // Check if the player cleared Aga
                if (action.CurrentData?.ReadUInt8(0x145) >= 3 && Tracker != null)
                {
                    var castleTower = Tracker.World.CastleTower;
                    if (!castleTower.DungeonState.Cleared)
                    {
                        Tracker.MarkDungeonAsCleared(castleTower, null, autoTracked: true);
                        _logger.LogInformation($"Auto tracked {castleTower.DungeonMetadata.Name} as cleared");
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the player has cleared locations in Super Metroid
        /// </summary>
        /// <param name="action"></param>
        protected void CheckMetroidLocations(EmulatorAction action)
        {
            if (action.CurrentData != null && action.PreviousData != null)
            {
                CheckLocations(action, LocationMemoryType.Default, false, Game.SM);
            }
        }

        /// <summary>
        ///  Checks if the player has defeated Super Metroid bosses
        /// </summary>
        /// <param name="action"></param>
        protected void CheckMetroidBosses(EmulatorAction action)
        {
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
        protected void CheckLocations(EmulatorAction action, LocationMemoryType type, bool is16Bit, Game game)
        {
            var currentData = action.CurrentData;
            var prevData = action.PreviousData;

            if (Tracker == null || currentData == null || prevData == null) return;

            // Store the locations for this action so that we don't need to grab them each time every half a second or so
            if (action.Locations == null)
            {
                action.Locations = _worldService.AllLocations().Where(x => x.MemoryType == type && ((game == Game.SM && x.Id < 256) || (game == Game.Zelda && x.Id >= 256))).ToList();
            }

            foreach (var location in action.Locations)
            {
                try
                {
                    var loc = location.MemoryAddress ?? 0;
                    var flag = location.MemoryFlag ?? 0;
                    var currentCleared = (is16Bit && currentData.CheckUInt16(loc * 2, flag)) || (!is16Bit && currentData.CheckBinary8Bit(loc, flag));
                    var prevCleared = (is16Bit && prevData.CheckUInt16(loc * 2, flag)) || (!is16Bit && prevData.CheckBinary8Bit(loc, flag));
                    if (!location.State.Cleared && currentCleared && prevCleared)
                    {
                        if (location.Region is GanonsTower gt && location != gt.BobsTorch)
                        {
                            IncrementGTItems(location);
                        }

                        var item = location.Item;
                        if (item != null)
                        {
                            Tracker.TrackItem(item: item, trackedAs: null, confidence: null, tryClear: true, autoTracked: true, location: location);
                            _logger.LogInformation($"Auto tracked {location.Item.Name} from {location.Name}");
                        }
                        else
                        {
                            Tracker.Clear(location, null, true);
                            _logger.LogInformation($"Auto tracked {location.Name} as cleared");
                        }
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to auto track location: " + location.Name);
                    Tracker.Error();
                }
            }
        }

        /// <summary>
        /// Checks the status of if dungeons have been cleared
        /// </summary>
        /// <param name="currentData">The latest memory data returned from the emulator</param>
        /// <param name="prevData">The previous memory data returned from the emulator</param>
        protected void CheckDungeons(EmulatorMemoryData currentData, EmulatorMemoryData prevData)
        {
            if (Tracker == null) return;

            foreach (var dungeon in Tracker.World.Dungeons)
            {
                var region = (Z3Region)dungeon;

                // Skip if we don't have any memory addresses saved for this dungeon
                if (region.MemoryAddress == null || region.MemoryFlag == null)
                {
                    continue;
                }

                try
                {
                    var prevValue = prevData.CheckUInt16(region.MemoryAddress * 2 ?? 0, region.MemoryFlag ?? 0);
                    var currentValue = currentData.CheckUInt16(region.MemoryAddress * 2 ?? 0, region.MemoryFlag ?? 0);
                    if (!dungeon.DungeonState.Cleared && prevValue && currentValue)
                    {
                        Tracker.MarkDungeonAsCleared(dungeon, autoTracked: true);
                        _logger.LogInformation($"Auto tracked {dungeon.DungeonName} as cleared");
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to auto track Dungeon: " + dungeon.DungeonName);
                    Tracker.Error();
                }
            }
        }

        /// <summary>
        /// Checks the status of if the Super Metroid bosses have been defeated
        /// </summary>
        /// <param name="data">The response from the lua script</param>
        protected void CheckSMBosses(EmulatorMemoryData data)
        {
            if (Tracker == null) return;

            foreach (var boss in Tracker.World.AllBosses.Where(x => x.Metadata?.MemoryAddress != null && x.Metadata?.MemoryFlag > 0 && x.State?.Defeated != true))
            {
                if (data.CheckBinary8Bit(boss.Metadata?.MemoryAddress ?? 0, boss.Metadata?.MemoryFlag ?? 100))
                {
                    Tracker.MarkBossAsDefeated(boss, true, null, true);
                    _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
                }
            }
        }

        /// <summary>
        /// Tracks the current memory state of LttP for Tracker voice lines
        /// </summary>
        /// <param name="action">The message from the emulator with the memory state</param>
        protected void CheckZeldaState(EmulatorAction action)
        {
            if (_previousGame != CurrentGame || action.CurrentData == null || Tracker == null) return;
            var prevState = ZeldaState;
            ZeldaState = new(action.CurrentData);
            _logger.LogDebug(ZeldaState.ToString());
            if (prevState == null) return;

            if (!_seenGTTorch
                && ZeldaState.CurrentRoom == 140
                && !ZeldaState.IsOnBottomHalfOfRoom
                && !ZeldaState.IsOnRightHalfOfRoom
                && ZeldaState.Substate != 14)
            {
                _seenGTTorch = true;
                IncrementGTItems(Tracker.World.GanonsTower.BobsTorch);
            }

            // Entered the triforce room
            if (ZeldaState.State == 0x19)
            {
                if (_beatBothBosses)
                {
                    Tracker.GameBeaten(true);
                }
            }

            foreach (var check in _zeldaStateChecks)
            {
                if (check != null && check.ExecuteCheck(Tracker, ZeldaState, prevState))
                {
                    _logger.LogInformation($"{check.GetType().Name} detected");
                }
            }
        }

        /// <summary>
        /// Checks if the final bosses of both games are defeated
        /// It appears as if 0x2 represents the first boss defeated and 0x106 is the second,
        /// no matter what order the bosses were defeated in
        /// </summary>
        /// <param name="action">The message from the emulator with the memory state</param>
        protected void CheckBeatFinalBosses(EmulatorAction action)
        {
            if (_previousGame != CurrentGame || action.CurrentData == null || Tracker == null) return;

            if (action.PreviousData?.ReadUInt8(0x2) == 0 && action.CurrentData.ReadUInt8(0x2) > 0)
            {
                if (CurrentGame == Game.Zelda)
                {
                    var gt = Tracker.World.GanonsTower;
                    if (!gt.DungeonState.Cleared)
                    {
                        Tracker.MarkDungeonAsCleared(gt, confidence: null, autoTracked: true);
                    }
                }
                else if (CurrentGame == Game.SM)
                {
                    var motherBrain = Tracker.World.AllBosses.First(x => x.Name == "Mother Brain");
                    if (motherBrain.State?.Defeated != true)
                    {
                        Tracker.MarkBossAsDefeated(motherBrain, admittedGuilt: true, confidence: null, autoTracked: true);
                    }
                }
            }

            if (action.PreviousData?.ReadUInt8(0x106) == 0 && action.CurrentData.ReadUInt8(0x106) > 0)
            {
                if (CurrentGame == Game.Zelda)
                {
                    var gt = Tracker.World.GanonsTower;
                    if (!gt.DungeonState.Cleared)
                    {
                        Tracker.MarkDungeonAsCleared(gt, confidence: null, autoTracked: true);
                    }
                }
                else if (CurrentGame == Game.SM)
                {
                    var motherBrain = Tracker.World.AllBosses.First(x => x.Name == "Mother Brain");
                    if (motherBrain.State?.Defeated != true)
                    {
                        Tracker.MarkBossAsDefeated(motherBrain, admittedGuilt: true, confidence: null, autoTracked: true);
                    }
                }
            }

            if (action.CurrentData.ReadUInt8(0x2) > 0 && action.CurrentData.ReadUInt8(0x106) > 0)
            {
                _beatBothBosses = true;
            }

        }

        /// <summary>
        /// Tracks the current memory state of SM for Tracker voice lines
        /// </summary>
        /// <param name="action">The message from the emulator with the memory state</param>
        protected void CheckMetroidState(EmulatorAction action)
        {
            if (_previousGame != CurrentGame || action.CurrentData == null || Tracker == null) return;
            var prevState = MetroidState;
            MetroidState = new(action.CurrentData);
            _logger.LogDebug(MetroidState.ToString());
            if (prevState == null) return;

            foreach (var check in _metroidStateChecks)
            {
                if (check != null && check.ExecuteCheck(Tracker, MetroidState, prevState))
                {
                    _logger.LogInformation($"{check.GetType().Name} detected");
                }
            }
        }

        /// <summary>
        /// Checks if the player has entered the ship
        /// </summary>
        /// <param name="action"></param>
        protected void CheckShip(EmulatorAction action)
        {
            if (_previousGame != CurrentGame || action.CurrentData == null || action.PreviousData == null || Tracker == null) return;
            var currentInShip = action.CurrentData.ReadUInt16(0) == 0xAA4F;
            if (currentInShip && _beatBothBosses)
            {
                Tracker?.GameBeaten(true);
            }
        }

        private void IncrementGTItems(Location location)
        {
            if (_foundGTKey || _config.Config.ZeldaKeysanity == true) return;

            var chatIntegrationModule = _trackerModuleFactory.GetModule<ChatIntegrationModule>();
            _numGTItems++;
            Tracker?.Say(_numGTItems.ToString());
            if (location.Item.Type == ItemType.BigKeyGT)
            {
                var responseIndex = 1;
                for (var i = 1; i <= _numGTItems; i++)
                {
                    if (Tracker?.Responses.AutoTracker.GTKeyResponses.ContainsKey(i) == true)
                    {
                        responseIndex = i;
                    }
                }
                Tracker?.Say(x => x.AutoTracker.GTKeyResponses[responseIndex], _numGTItems);
                chatIntegrationModule?.GTItemTracked(_numGTItems, true);
                _foundGTKey = true;
            }
            else
            {
                chatIntegrationModule?.GTItemTracked(_numGTItems, false);
            }
        }
    }
}
