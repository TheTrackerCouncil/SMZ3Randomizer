using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks;
using Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

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
        private readonly IEnumerable<IZeldaStateCheck?> _zeldaStateChecks;
        private readonly IEnumerable<IMetroidStateCheck?> _metroidStateChecks;
        private int _currentIndex = 0;
        private Game _previousGame;
        private bool _hasStarted;
        private IEmulatorConnector? _connector;

        /// <summary>
        /// Constructor for Auto Tracker
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="zeldaStateChecks"></param>
        /// <param name="metroidStateChecks"></param>
        public AutoTracker(ILogger<AutoTracker> logger, ILoggerFactory loggerFactory, IEnumerable<IZeldaStateCheck> zeldaStateChecks, IEnumerable<IMetroidStateCheck> metroidStateChecks)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;

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
                Address = 0x7033fe,
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

            _zeldaStateChecks = zeldaStateChecks;
            _metroidStateChecks = metroidStateChecks;
            _logger.LogInformation($"Zelda state checks: {_zeldaStateChecks.Count()}");
            _logger.LogInformation($"Metroid state checks: {_metroidStateChecks.Count()}");
        }

        /// <summary>
        /// The tracker associated with this auto tracker
        /// </summary>
        public Tracker? Tracker { get; set; }

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
        /// If the player currently has a fairy
        /// </summary>
        public bool PlayerHasFairy { get; protected set; }

        /// <summary>
        /// Called when the connector successfully established a connection with the emulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connector_Connected(object? sender, EventArgs e)
        {
            Tracker?.Say(x => x.AutoTracker.WhenConnected);
            AutoTrackerConnected?.Invoke(this, new());
            _ = SendMessagesAsync();
            _currentIndex = 0;
        }

        /// <summary>
        /// Called when a connector has temporarily lost connection with the emulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connector_Disconnected(object? sender, EventArgs e)
        {
            Tracker?.Say("Auto tracker disconnected");
            _logger.LogInformation("Disconnected");
            AutoTrackerDisconnected?.Invoke(this, new());
        }

        /// <summary>
        /// The connector has received memory from the emulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connector_MessageReceived(object? sender, EmulatorDataReceivedEventArgs e)
        {
            // Verify that message we received is still valid
            if (_readActionMap[e.Address].ShouldProcess(CurrentGame, _hasStarted))
            {
                _readActionMap[e.Address].Invoke(e.Data);
            }
        }

        /// <summary>
        /// Sends requests out to the connected lua script
        /// </summary>
        protected async Task SendMessagesAsync()
        {
            while (_connector != null && _connector.IsConnected())
            {
                if (_connector.CanSendMessage())
                {
                    while (!_readActions[_currentIndex].ShouldProcess(CurrentGame, _hasStarted))
                    {
                        _currentIndex = (_currentIndex + 1) % _readActions.Count;
                    }
                    _connector.SendMessage(_readActions[_currentIndex]);
                    _currentIndex = (_currentIndex + 1) % _readActions.Count;
                }

                await Task.Delay(TimeSpan.FromSeconds(0.1f));
            }
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
                action.Locations = Tracker.World.Locations.Where(x => x.MemoryType == type && ((game == Game.SM && x.Id < 256) || (game == Game.Zelda && x.Id >= 256))).ToList();
            }

            foreach (var location in action.Locations)
            {
                try
                {
                    var loc = location.MemoryAddress ?? 0;
                    var flag = location.MemoryFlag ?? 0;
                    var currentCleared = (is16Bit && currentData.CheckUInt16(loc * 2, flag)) || (!is16Bit && currentData.CheckBinary8Bit(loc, flag));
                    var prevCleared = (is16Bit && prevData.CheckUInt16(loc * 2, flag)) || (!is16Bit && prevData.CheckBinary8Bit(loc, flag));
                    if (!location.Cleared && currentCleared && prevCleared)
                    {
                        var item = Tracker.Items.SingleOrDefault(x => x.InternalItemType == location.Item.Type);
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

            foreach (var dungeonInfo in Tracker.WorldInfo.Dungeons)
            {
                var region = Tracker.World.Regions.First(x => dungeonInfo.Is(x) && x is Z3Region) as Z3Region;
                if (region == null)
                {
                    _logger.LogError($"Could not find region for {dungeonInfo.Name}");
                    Tracker.Error();
                    continue;
                }

                try
                {
                    var prevValue = prevData.CheckUInt16(region.MemoryAddress * 2 ?? 0, region.MemoryFlag ?? 0);
                    var currentValue = currentData.CheckUInt16(region.MemoryAddress * 2 ?? 0, region.MemoryFlag ?? 0);
                    if (!dungeonInfo.Cleared && prevValue && currentValue)
                    {
                        Tracker.MarkDungeonAsCleared(dungeonInfo);
                        _logger.LogInformation($"Auto tracked {dungeonInfo.Name} as cleared");
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to auto track Dungeon: " + dungeonInfo.Name);
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
            var boss = Tracker.WorldInfo.Bosses.First(x => "Kraid".Equals(x.Name[0], StringComparison.OrdinalIgnoreCase));
            if (!boss.Defeated && data.CheckBinary8Bit(0x1, 0x1))
            {
                Tracker.MarkBossAsDefeated(boss);
                _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
            }

            boss = Tracker.WorldInfo.Bosses.First(x => "Ridley".Equals(x.Name[0], StringComparison.OrdinalIgnoreCase));
            if (!boss.Defeated && data.CheckBinary8Bit(0x2, 0x1))
            {
                Tracker.MarkBossAsDefeated(boss);
                _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
            }

            boss = Tracker.WorldInfo.Bosses.First(x => "Phantoon".Equals(x.Name[0], StringComparison.OrdinalIgnoreCase));
            if (!boss.Defeated && data.CheckBinary8Bit(0x3, 0x1))
            {
                Tracker.MarkBossAsDefeated(boss);
                _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
            }

            boss = Tracker.WorldInfo.Bosses.First(x => "Draygon".Equals(x.Name[0], StringComparison.OrdinalIgnoreCase));
            if (!boss.Defeated && data.CheckBinary8Bit(0x4, 0x1))
            {
                Tracker.MarkBossAsDefeated(boss);
                _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
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

            foreach (var check in _zeldaStateChecks)
            {
                if (check != null && check.ExecuteCheck(Tracker, ZeldaState, prevState))
                {
                    _logger.LogInformation($"{check.GetType().Name} detected");
                }
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
    }
}
