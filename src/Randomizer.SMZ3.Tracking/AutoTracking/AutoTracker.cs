using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld.DeathMountain;
using Randomizer.SMZ3.Regions.Zelda.LightWorld;
using Randomizer.SMZ3.Regions.Zelda.LightWorld.DeathMountain;
using Randomizer.SMZ3.Tracking.Configuration;

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
        private int _currentIndex = 0;
        private Game _previousGame;
        private int _previousMetroidRegionValue = -1;
        private bool _hasStarted;
        private readonly HashSet<DungeonInfo> _enteredDungeons = new();
        private readonly HashSet<SchrodingersString> _statedMessages = new();
        private IEmulatorConnector? _connector;
        private readonly ILoggerFactory _loggerFactory;
        private bool _hasFairy;
        
        /// <summary>
        /// Constructor for Auto Tracker
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="loggerFactory"></param>
        public AutoTracker(ILogger<AutoTracker> logger, ILoggerFactory loggerFactory)
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

            // Check Zelda NPCs
            AddReadAction(new()
            {
                Type = EmulatorActionType.ReadBlock,
                Domain = MemoryDomain.WRAM,
                Address = 0x7ef410,
                Length = 0x2,
                Game = Game.Zelda,
                Action = CheckZeldaNPCs
            });

            // Check Zelda Overworld
            AddReadAction(new()
            {
                Type = EmulatorActionType.ReadBlock,
                Domain = MemoryDomain.WRAM,
                Address = 0x7ef280,
                Length = 0x82,
                Game = Game.Zelda,
                Action = CheckZeldaOverworld
            });

            // Check Zelda Inventory
            AddReadAction(new()
            {
                Type = EmulatorActionType.ReadBlock,
                Domain = MemoryDomain.WRAM,
                Address = 0x7ef300,
                Length = 0xD0,
                Game = Game.Zelda,
                Action = CheckZeldaInventory
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
        public AutoTrackerViewedAction LatestViewAction;

        /// <summary>
        /// If a connector is currently enabled
        /// </summary>
        public bool IsEnabled => _connector != null;

        /// <summary>
        /// If a connector is currently connected to the emulator
        /// </summary>
        public bool IsConnected => _connector != null && _connector.IsConnected();

        /// <summary>
        /// Called when the connector successfully established a connection with the emulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connector_Connected(object? sender, EventArgs e)
        {
            Tracker.Say(x => x.AutoTracker.WhenConnected);
            AutoTrackerConnected?.Invoke(this, new());
            SendMessages();
            _currentIndex = 0;
        }

        /// <summary>
        /// Called when a connector has temporarily lost connection with the emulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Connector_Disconnected(object? sender, EventArgs e)
        {
            Tracker.Say("Auto tracker disconnected");
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
            _readActionMap[e.Address].Invoke(e.Data);
        }

        /// <summary>
        /// Sends requests out to the connected lua script
        /// </summary>
        protected async void SendMessages()
        {
            while (_connector != null && _connector.IsConnected())
            {
                while (!_readActions[_currentIndex].ShouldSend(CurrentGame, _hasStarted))
                {
                    _currentIndex = (_currentIndex + 1) % _readActions.Count;
                }
                _connector.SendMessage(_readActions[_currentIndex]);
                _currentIndex = (_currentIndex + 1) % _readActions.Count;
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
                Tracker?.Say(x => x.AutoTracker.GameStarted, Tracker.Rom.Seed);
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
                _previousMetroidRegionValue = -1;
            }
            else if (value == 0xFF)
            {
                CurrentGame = Game.SM;
            }
            if (_previousGame != CurrentGame)
            {
                _logger.LogInformation($"Game changed to: {CurrentGame} {value}");
            }
        }

        /// <summary>
        /// Checks if the player has cleared Zelda room locations (cave, houses, dungeons)
        /// This also checks if the player has gotten the dungeon rewards
        /// </summary>
        /// <param name="action"></param>
        protected void CheckZeldaRooms(EmulatorAction action)
        {
            if (action.CurrentData != null && !action.HasDataChanged())
            {
                CheckLocations(action.CurrentData, LocationMemoryType.Default, true, Game.Zelda);
                CheckDungeons(action.CurrentData);
            }
        }

        /// <summary>
        /// Checks if the player has cleared Zelda NPC locations
        /// </summary>
        /// <param name="action"></param>
        protected void CheckZeldaNPCs(EmulatorAction action)
        {
            if (action.CurrentData != null && !action.HasDataChanged())
            {
                CheckLocations(action.CurrentData, LocationMemoryType.ZeldaNPC, false, Game.Zelda);
            }
        }

        /// <summary>
        /// Checks if the player has cleared Zelda Overworld locations
        /// </summary>
        /// <param name="action"></param>
        protected void CheckZeldaOverworld(EmulatorAction action)
        {
            if (action.CurrentData != null && !action.HasDataChanged())
            {
                CheckLocations(action.CurrentData, LocationMemoryType.ZeldaOverworld, false, Game.Zelda);
            }
        }

        /// <summary>
        /// Various checks for the Zelda inventory memory locations
        /// Currently this checks some Zelda locations that are oddly in the same memory
        /// block as the inventory, such as Link's uncle
        /// </summary>
        /// <param name="action"></param>
        protected void CheckZeldaInventory(EmulatorAction action)
        {
            if (action.CurrentData != null && !action.HasDataChanged())
            {
                // Check if the player has any bottled fairies for detecting deaths
                _hasFairy = false;
                for (var i = 0; i < 4; i++)
                {
                    _hasFairy |= action.CurrentData.ReadUInt8(0x5c + i) == 6;
                }
                CheckLocations(action.CurrentData, LocationMemoryType.ZeldaMisc, false, Game.Zelda);
            }
        }

        /// <summary>
        /// Checks to see if the player has cleared locations in Super Metroid
        /// </summary>
        /// <param name="action"></param>
        protected void CheckMetroidLocations(EmulatorAction action)
        {
            if (action.CurrentData != null && !action.HasDataChanged())
            {
                CheckLocations(action.CurrentData, LocationMemoryType.Default, false, Game.SM);
            }
        }

        /// <summary>
        ///  Checks if the player has defeated Super Metroid bosses
        /// </summary>
        /// <param name="action"></param>
        protected void CheckMetroidBosses(EmulatorAction action)
        {
            if (action.CurrentData != null && !action.HasDataChanged())
            {
                CheckSMBosses(action.CurrentData);
            }
        }

        /// <summary>
        /// Checks locations to see if they have accessed or not
        /// </summary>
        /// <param name="data">The memory returned from the emulator</param>
        /// <param name="type">The type of location to find the correct LocationInfo objects</param>
        /// <param name="is16Bit">Set to true if this is a 16 bit value or false for 8 bit</param>
        /// <param name="game">The game that is being checked</param>
        protected void CheckLocations(EmulatorMemoryData data, LocationMemoryType type, bool is16Bit, Game game)
        {
            foreach (var location in Tracker.World.Locations.Where(x => x.MemoryType == type && ((game == Game.SM && x.Id < 256) || (game == Game.Zelda && x.Id >= 256))))
            {
                try
                {
                    var loc = location.MemoryAddress ?? 0;
                    var flag = location.MemoryFlag ?? 0;
                    if (!location.Cleared && ((is16Bit && data.CheckUInt16(loc * 2, flag)) || (!is16Bit && data.CheckBinary8Bit(loc, flag))))
                    {
                        var item = Tracker.Items.SingleOrDefault(x => x.InternalItemType == location.Item.Type);
                        if (item != null)
                        {
                            Tracker.TrackItem(item, location, null, null, true);
                            _logger.LogInformation($"Auto tracked {location.Item.Name} from {location.Name} {loc} {flag} {is16Bit}");
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
        /// <param name="data">The memory returned from the emulator</param>
        protected void CheckDungeons(EmulatorMemoryData data)
        {
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
                    if (!dungeonInfo.Cleared && data.CheckUInt16(region.MemoryAddress * 2 ?? 0, region.MemoryFlag ?? 0))
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
        /// <param name="message">The response from the lua script</param>
        protected void CheckSMBosses(EmulatorMemoryData data)
        {
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
        /// <param name="message">The message from the emulator with the memory state</param>
        protected void CheckZeldaState(EmulatorAction action)
        {
            if (_previousGame != CurrentGame) return;
            var prevState = ZeldaState;
            ZeldaState = new(action.CurrentData);
            _logger.LogDebug(ZeldaState.ToString());
            if (prevState == null) return;

            // Falling down from Moldorm (detect if player was in Moldorm room and is now in the room below it)
            if (ZeldaState.CurrentRoom == 23 && ZeldaState.PreviousRoom == 7 && prevState.CurrentRoom == 7)
            {
                SayOnce(Tracker.Responses.AutoTracker.FallFromMoldorm);
            }
            // Falling down from Ganon (detect if player was in Ganon room and is now in the room below it)
            else if (ZeldaState.CurrentRoom == 16 && ZeldaState.PreviousRoom == 0 && prevState.CurrentRoom == 0)
            {
                SayOnce(Tracker.Responses.AutoTracker.FallFromGanon);
            }
            // Hera pot (player is in the pot room and did not get there from falling from the two rooms above it)
            else if (ZeldaState.CurrentRoom == 167 && prevState.CurrentRoom == 119 && prevState.PreviousRoom != 49)
            {
                _logger.LogInformation("Hera Pot detected");
                SayOnce(Tracker.Responses.AutoTracker.HeraPot);
            }
            // Ice breaker (player is on the right side of the wall but was previous in the room to the left)
            else if (ZeldaState.CurrentRoom == 31 && ZeldaState.PreviousRoom == 30 && ZeldaState.LinkX >= 7961 && prevState.LinkX < 7961 && ZeldaState.IsOnRightHalfOfRoom && prevState.IsOnRightHalfOfRoom)
            {
                _logger.LogInformation("Ice breaker detected");
                SayOnce(Tracker.Responses.AutoTracker.IceBreaker);
            }
            // Back Diver Down (player is now at the lower section and on the ground, but not from the ladder)
            else if (ZeldaState.CurrentRoom == 118 && ZeldaState.LinkX < 3474 && (ZeldaState.LinkX < 3400 || ZeldaState.LinkX > 3430) && ZeldaState.LinkY <= 3975 && prevState.LinkY > 3975 && (ZeldaState.LinkState is 0 or 6 or 3) && ZeldaState.IsOnBottomHalfOfroom && ZeldaState.IsOnRightHalfOfRoom)
            {
                _logger.LogInformation("Diver down detected");
                SayOnce(Tracker.Responses.AutoTracker.DiverDown);
            }
            // Left side diver down (player is now in the lower section to the right and on the ground, but not from the ladder)
            else if (ZeldaState.CurrentRoom == 53 && ZeldaState.PreviousRoom == 54 && ZeldaState.LinkX > 2800 && ZeldaState.LinkX < 2850 && ZeldaState.LinkY <= 1915 && prevState.LinkY > 1915 && (ZeldaState.LinkState is 0 or 6 or 3))
            {
                _logger.LogInformation("Diver down detected");
                SayOnce(Tracker.Responses.AutoTracker.DiverDown);
            }
            // Entered a dungeon (now in Dungeon state but was previously in Overworld or entering Dungeon state)
            else if (ZeldaState.State == 0x07 && (prevState.State == 0x06 || prevState.State == 0x09 || prevState.State == 0x0F || prevState.State == 0x10 || prevState.State == 0x11))
            {
                // Get the region for the room 
                var region = Tracker.World.Regions.Where(x => x is Z3Region)
                    .Select(x => x as Z3Region)
                    .FirstOrDefault(x => x != null && x.StartingRooms != null && x.StartingRooms.Contains(ZeldaState.CurrentRoom) && !x.IsOverworld);
                if (region == null) return;

                // Get the dungeon info for the room
                var dungeonInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(region));

                if (!_enteredDungeons.Contains(dungeonInfo) && (dungeonInfo.Reward == RewardItem.RedPendant || dungeonInfo.Reward == RewardItem.GreenPendant || dungeonInfo.Reward == RewardItem.BluePendant))
                {
                    Tracker.Say(Tracker.Responses.AutoTracker.EnterPendantDungeon, dungeonInfo.Name, dungeonInfo.Reward.GetName());
                }
                else if (region is CastleTower)
                {
                    SayOnce(Tracker.Responses.AutoTracker.EnterHyruleCastleTower);
                }
                else if (region is GanonsTower)
                {
                    var clearedCrystalDungeonCount = Tracker.WorldInfo.Dungeons
                                                        .Where(x => x.Cleared)
                                                        .Select(x => x.GetRegion(Tracker.World) as IHasReward)
                                                        .Count(x => x != null && x.Reward is Reward.CrystalBlue or Reward.CrystalRed);
                    if (clearedCrystalDungeonCount < 7)
                    {
                        SayOnce(Tracker.Responses.AutoTracker.EnteredGTEarly, clearedCrystalDungeonCount);
                    }
                }
                
                Tracker.UpdateRegion(region, Tracker.Options.AutoTrackerChangeMap);
                _enteredDungeons.Add(dungeonInfo);
            }
            // Changed overworld (either the state was changed to overworld or the overworld screen changed)
            else if (ZeldaState.State == 0x09 && (prevState.State != 0x09 || ZeldaState.OverworldScreen != prevState.OverworldScreen))
            {
                // Get the region for the room 
                var region = Tracker.World.Regions.Where(x => x is Z3Region)
                    .Select(x => x as Z3Region)
                    .FirstOrDefault(x => x != null && x.StartingRooms != null && x.StartingRooms.Contains(ZeldaState.OverworldScreen) && x.IsOverworld);
                if (region == null) return;

                Tracker.UpdateRegion(region, Tracker.Options.AutoTrackerChangeMap);
            }
            // Death (entered death state without a fairy)
            else if (ZeldaState.State == 0x12 && prevState.State != 0x12 && !_hasFairy)
            {
                _logger.LogInformation("Zelda death detected");

                // Say specific message for dying in the particular screen/room the player is in
                if (Tracker.CurrentRegion != null && Tracker.CurrentRegion.WhenDiedInRoom != null)
                {
                    var region = Tracker.CurrentRegion.GetRegion(Tracker.World) as Z3Region;
                    if (region != null && region.IsOverworld && Tracker.CurrentRegion.WhenDiedInRoom.ContainsKey(prevState.OverworldScreen.ToString()))
                    {
                        Tracker.Say(Tracker.CurrentRegion.WhenDiedInRoom[prevState.OverworldScreen.ToString()]);
                    }
                    else if (region != null && !region.IsOverworld && Tracker.CurrentRegion.WhenDiedInRoom.ContainsKey(prevState.CurrentRoom.ToString()))
                    {
                        Tracker.Say(Tracker.CurrentRegion.WhenDiedInRoom[prevState.CurrentRoom.ToString()]);
                    }
                }

                Tracker.TrackItem(Tracker.Items.First(x => x.ToString().Equals("Death", StringComparison.OrdinalIgnoreCase)));
            }
            // Swimming without flippers
            else if (ZeldaState.LinkState == 0x04 && prevState.LinkState == 0x04 && Tracker.Items.Any(x => x.InternalItemType == ItemType.Flippers && x.TrackingState == 0))
            {
                SayOnce(Tracker.Responses.AutoTracker.FakeFlippers);
            }
            // Looked at full map
            else if (ZeldaState.State == 14 && ZeldaState.Substate == 7 && ZeldaState.ReadUInt8(0xE0) == 0x80 && (LatestViewAction == null || !LatestViewAction.IsValid))
            {
                var currentRegion = Tracker?.CurrentRegion?.GetRegion(Tracker.World);
                if (currentRegion is LightWorldNorthWest or LightWorldNorthEast or LightWorldSouth or LightWorldDeathMountainEast or LightWorldDeathMountainWest)
                {
                    LatestViewAction = new AutoTrackerViewedAction(UpdateLightWorldRewards);
                }
                else if (currentRegion is DarkWorldNorthWest or DarkWorldNorthEast or DarkWorldSouth or DarkWorldMire or DarkWorldDeathMountainEast or DarkWorldDeathMountainWest)
                {
                    LatestViewAction = new AutoTrackerViewedAction(UpdateDarkWorldRewards);
                }
            }
        }

        /// <summary>
        /// Marks all of the rewards for the light world dungeons
        /// </summary>
        private void UpdateLightWorldRewards()
        {
            var rewards = new List<Reward>();

            var ep = Tracker.World.EasternPalace;
            var epInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(ep));
            rewards.Add(ep.Reward);

            var dp = Tracker.World.DesertPalace;
            var dpInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(dp));
            rewards.Add(dp.Reward);

            var toh = Tracker.World.TowerOfHera;
            var tohInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(toh));
            rewards.Add(toh.Reward);

            if (rewards.Count(x => x == Reward.CrystalRed || x == Reward.CrystalBlue) == 3)
            {
                SayOnce(Tracker.Responses.AutoTracker.LightWorldAllCrystals);
            }

            Tracker.SetDungeonReward(epInfo, ConvertReward(ep.Reward));
            Tracker.SetDungeonReward(dpInfo, ConvertReward(dp.Reward));
            Tracker.SetDungeonReward(tohInfo, ConvertReward(toh.Reward));
        }

        /// <summary>
        /// Marks all of the rewards for the dark world dungeons
        /// </summary>
        protected void UpdateDarkWorldRewards()
        {
            var pod = Tracker.World.PalaceOfDarkness;
            var podInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(pod));
            
            var sp = Tracker.World.SwampPalace;
            var spInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(sp));
            
            var sw = Tracker.World.SkullWoods;
            var swInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(sw));
            
            var tt = Tracker.World.ThievesTown;
            var ttInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(tt));
            
            var ip = Tracker.World.IcePalace;
            var ipInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(ip));
            
            var mm = Tracker.World.MiseryMire;
            var mmInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(mm));
            
            var tr = Tracker.World.TurtleRock;
            var trInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(tr));

            if (mm.Reward != Reward.CrystalRed && mm.Reward != Reward.CrystalBlue && tr.Reward != Reward.CrystalRed && tr.Reward != Reward.CrystalBlue)
            {
                SayOnce(Tracker.Responses.AutoTracker.DarkWorldNoMedallions);
            }

            Tracker.SetDungeonReward(podInfo, ConvertReward(pod.Reward));
            Tracker.SetDungeonReward(spInfo, ConvertReward(sp.Reward));
            Tracker.SetDungeonReward(swInfo, ConvertReward(sw.Reward));
            Tracker.SetDungeonReward(ttInfo, ConvertReward(tt.Reward));
            Tracker.SetDungeonReward(ipInfo, ConvertReward(ip.Reward));
            Tracker.SetDungeonReward(mmInfo, ConvertReward(mm.Reward));
            Tracker.SetDungeonReward(trInfo, ConvertReward(tr.Reward));
        }

        /// <summary>
        /// Tracks the current memory state of SM for Tracker voice lines
        /// </summary>
        /// <param name="message">The message from the emulator with the memory state</param>
        protected void CheckMetroidState(EmulatorAction action)
        {
            if (_previousGame != CurrentGame) return;
            var prevState = MetroidState;
            MetroidState = new(action.CurrentData);
            _logger.LogDebug(MetroidState.ToString());
            if (prevState == null) return;

            // Update the region that the player is currently in
            if (MetroidState.CurrentRegion != _previousMetroidRegionValue)
            {
                var newRegion = Tracker.World.Regions.Select(x => x as SMRegion).FirstOrDefault(x => x != null && x.MemoryRegionId == MetroidState.CurrentRegion);
                if (newRegion != null)
                {
                    Tracker.UpdateRegion(newRegion, Tracker.Options.AutoTrackerChangeMap);
                }
                _previousMetroidRegionValue = MetroidState.CurrentRegion;
            }

            // Approaching Kraid's Awful Son
            if (MetroidState.CurrentRegion == 1 && MetroidState.CurrentRoomInRegion == 45 && prevState.CurrentRoomInRegion == 44)
            {
                SayOnce(Tracker.Responses.AutoTracker.NearKraidsAwfulSon);
            }
            // Approaching Shaktool
            else if (MetroidState.CurrentRegion == 4 && MetroidState.CurrentRoomInRegion == 36 && prevState.CurrentRoomInRegion == 28)
            {
                SayOnce(Tracker.Responses.AutoTracker.NearShaktool);
            }
            // Approaching Crocomire
            else if (MetroidState.CurrentRegion == 2 && MetroidState.CurrentRoomInRegion == 9 && MetroidState.SamusX >= 3000 && MetroidState.SamusY > 500)// && !Tracker.WorldInfo.Bosses.First(x => "Crocomire".Equals(x.Name[0])).Defeated)
            {
                SayOnce(Tracker.Responses.AutoTracker.NearCrocomire, MetroidState.SuperMissiles, MetroidState.MaxSuperMissiles);
            }
            // Brinstar Mockball (got past gates without speed booster)
            else if (MetroidState.CurrentRegion == 1 && MetroidState.CurrentRoomInRegion == 3 && MetroidState.SamusX >= 560 && prevState.SamusX < 560 && MetroidState.SamusX < 800 && Tracker?.FindItemByType(ItemType.SpeedBooster)?.TrackingState == 0)
            {
                _logger.LogInformation("Mockball detected");
                SayOnce(Tracker.Responses.AutoTracker.MockBall);

            }
            // Norfair Mockball (got past gates without speed booster)
            else if (MetroidState.CurrentRegion == 2 && MetroidState.CurrentRoomInRegion == 4 && MetroidState.SamusX <= 1016 && prevState.SamusX > 1016 && MetroidState.SamusX > 800 && Tracker?.FindItemByType(ItemType.SpeedBooster)?.TrackingState == 0)
            {
                _logger.LogInformation("Mockball detected");
                SayOnce(Tracker.Responses.AutoTracker.MockBall);

            }
            // Skip spore spawn (entered spore spawn item room from tall brinstar room)
            else if (MetroidState.CurrentRegion == 1 && MetroidState.CurrentRoomInRegion == 22 && prevState.CurrentRoomInRegion == 9)
            {
                _logger.LogInformation("Spore spawn skip");
                SayOnce(Tracker.Responses.AutoTracker.SkipSporeSpawn);

            }
            // Ridley face
            else if (MetroidState.CurrentRegion == 2 && MetroidState.CurrentRoomInRegion == 37 && MetroidState.SamusX <= 375 && MetroidState.SamusX >= 100 && MetroidState.SamusY <= 200)
            {
                _logger.LogInformation("Greeting Ridley face");
                SayOnce(Tracker.Responses.AutoTracker.RidleyFace);

            }
            // Death (health and reserve tanks all 0 (have to check to make sure the player isn't warping between games)
            else if (MetroidState.Health == 0 && MetroidState.ReserveTanks == 0 && prevState.Health != 0 && !(MetroidState.CurrentRoom == 0 && MetroidState.CurrentRegion == 0 && MetroidState.SamusY == 0))
            {
                var region = Tracker.World.Regions.Select(x => x as SMRegion)
                    .Where(x => x != null && x.MemoryRegionId == MetroidState.CurrentRegion)
                    .Select(x => Tracker.WorldInfo.Regions.FirstOrDefault(y => y.GetRegion(Tracker.World) == x && y.WhenDiedInRoom != null))
                    .FirstOrDefault(x => x != null && x.WhenDiedInRoom != null && x.WhenDiedInRoom.ContainsKey(MetroidState.CurrentRoomInRegion.ToString()));
                if (region != null)
                {
                    SayOnce(region.WhenDiedInRoom[MetroidState.CurrentRoomInRegion.ToString()]);
                }
                Tracker.TrackItem(Tracker.Items.First(x => x.ToString().Equals("Death", StringComparison.OrdinalIgnoreCase)));
            }
        }

        /// <summary>
        /// Have Tracker say a message, but only one time
        /// </summary>
        /// <param name="statement">The response(s) to say</param>
        /// <param name="args">Arguments for the statement</param>
        protected void SayOnce(SchrodingersString statement, params object?[] args)
        {
            if (!_statedMessages.Contains(statement))
            {
                Tracker.Say(statement, args);
                _statedMessages.Add(statement);
            }
        }

        /// <summary>
        /// Converts Rewards to RewardItems
        /// TODO: Try to figure out how to determine between blue and red pendants
        /// </summary>
        /// <param name="reward"></param>
        /// <returns></returns>
        private RewardItem ConvertReward(Reward reward)
        {
            switch (reward)
            {
                case Reward.CrystalRed:
                    return RewardItem.RedCrystal;
                case Reward.CrystalBlue:
                    return RewardItem.Crystal;
                case Reward.PendantGreen:
                    return RewardItem.GreenPendant;
                case Reward.PendantNonGreen:
                    return RewardItem.RedPendant;
                default:
                    return RewardItem.Unknown;
            }
        }
    }
}
