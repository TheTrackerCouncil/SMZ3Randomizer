using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Module that handles the basics of reading the rom's memory while being played
    /// for autotracking purposes. It creates a listening tcp port for the lua script
    /// to connect to and then submits requests to the lua script and interprets the
    /// responses
    /// </summary>
    public class AutoTrackerModule : TrackerModule, IDisposable
    {
        private readonly ILogger<AutoTrackerModule> _logger;
        private readonly List<AutoTrackerMessage> _requestMessages = new();
        private readonly Dictionary<int, Action<AutoTrackerMessage>> _responseActions = new();
        private readonly Dictionary<int, AutoTrackerMessage> _previousResponses = new();
        private int _currentRequestIndex = 0;
        private Game _previousGame;
        private Game _currentGame;
        private TcpListener? _tcpListener = null;
        private bool _hasStarted;
        private Socket? _socket = null;
        private AutoTrackerZeldaState? _previousZeldaState;
        private AutoTrackerMetroidState? _previousMetroidState;
        private readonly HashSet<DungeonInfo> _enteredDungeons = new();
        private readonly HashSet<SchrodingersString> _statedMessages = new();
        private int _previousMetroidRegionValue = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTrackerModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to write logging information.</param>
        public AutoTrackerModule(Tracker tracker, ILogger<AutoTrackerModule> logger) : base(tracker, logger)
        {
            _logger = logger;
            Tracker.AutoTracker = this;

            // Check if the game has started. SM locations start as cleared in memory until you get to the title screen.
            AddEvent("read_block", "WRAM", 0x7e0020, 0x1, Game.Neither, (AutoTrackerMessage message) =>
            {
                var value = message.ReadUInt8(0);
                if (value != 0 && !_hasStarted)
                {
                    _logger.LogInformation("Game started");
                    _hasStarted = true;
                    Tracker.Say(x => x.AutoTracker.GameStarted, Tracker.Rom.Seed);
                }
                _previousResponses[message.Address] = message;
            });

            // Active game
            AddEvent("read_block", "CARTRAM", 0x7033fe, 0x2, Game.Both, (AutoTrackerMessage message) =>
            {
                _previousGame = _currentGame;
                var value = message.ReadUInt8(0);
                if (value == 0x00)
                {
                    
                    _currentGame = Game.Zelda;
                    _previousMetroidRegionValue = -1;
                }
                else if (value == 0xFF)
                {
                    _currentGame = Game.SM;
                }
                if (_previousGame != _currentGame)
                {
                    _logger.LogInformation($"Game changed to: {_currentGame} {value}");
                }
                _previousResponses[message.Address] = message;
            });

            // Zelda Room Locations
            AddEvent("read_block", "WRAM", 0x7ef000, 0x250, Game.Zelda, (AutoTrackerMessage message) =>
            {
                if (message.IsMemoryEqualTo(_previousResponses[message.Address]))
                {
                    CheckLocations(message, LocationMemoryType.Default, true, Game.Zelda);
                    CheckDungeons(message);
                }
                _previousResponses[message.Address] = message;
            });

            // Zelda NPC Locations
            AddEvent("read_block", "WRAM", 0x7ef410, 0x2, Game.Zelda, (AutoTrackerMessage message) =>
            {
                if (message.IsMemoryEqualTo(_previousResponses[message.Address]))
                {
                    CheckLocations(message, LocationMemoryType.ZeldaNPC, false, Game.Zelda);
                }
                _previousResponses[message.Address] = message;
            });

            // Zelda Overworld Locations
            AddEvent("read_block", "WRAM", 0x7ef280, 0x82, Game.Zelda, (AutoTrackerMessage message) =>
            {
                if (message.IsMemoryEqualTo(_previousResponses[message.Address]))
                {
                    CheckLocations(message, LocationMemoryType.ZeldaOverworld, false, Game.Zelda);
                }
                _previousResponses[message.Address] = message;
            });

            // Zelda items while playing Zelda
            AddEvent("read_block", "WRAM", 0x7ef300, 0xD0, Game.Zelda, (AutoTrackerMessage message) =>
            {
                if (message.IsMemoryEqualTo(_previousResponses[message.Address]))
                {
                    // CheckZeldaItemMemory
                    CheckLocations(message, LocationMemoryType.ZeldaMisc, false, Game.Zelda);
                }
                _previousResponses[message.Address] = message;
            });

            // Super Metroid locations
            AddEvent("read_block", "WRAM", 0x7ed870, 0x20, Game.SM, (AutoTrackerMessage message) =>
            {
                if (message.IsMemoryEqualTo(_previousResponses[message.Address]))
                {
                    CheckLocations(message, LocationMemoryType.Default, false, Game.SM);
                }
                _previousResponses[message.Address] = message;
            });

            // Super Metroid bosses
            AddEvent("read_block", "WRAM", 0x7ed828, 0x08, Game.SM, (AutoTrackerMessage message) =>
            {
                if (message.IsMemoryEqualTo(_previousResponses[message.Address]))
                {
                    CheckSMBosses(message);
                }
                _previousResponses[message.Address] = message;
            });

            // Zelda State Checks
            AddEvent("read_block", "WRAM", 0x7e0000, 0x250, Game.Zelda, (AutoTrackerMessage message) =>
            {
                if (_previousGame == _currentGame)
                {
                    ZeldaStateChecks(message);
                    _previousResponses[message.Address] = message;
                }
            });

            // Metroid State Checks
            AddEvent("read_block", "WRAM", 0x7e0750, 0x400, Game.SM, (AutoTrackerMessage message) =>
            {
                if (_previousGame == _currentGame)
                {
                    MetroidStateChecks(message);
                    _previousResponses[message.Address] = message;
                }
            });


            // SM items while playing Zelda
            /*AddEvent("read_block", CorrectSRAMAddress(0xa17900), 0x10, Game.Zelda, (AutoTrackerMessage message) =>
            {
                var speed = message.Check8(0x03, 0x20);
                var plasma = message.Check8(0x06, 0x08);
                var gravity = message.Check8( 0x02, 0x20);
                var bombs = message.Check8(0x03, 0x10);
                _logger.LogInformation($"speed {speed} | plasma {plasma} | Gravity suit {gravity} | morph bombs {bombs}");
                _previousResponses[message.Address] = message;
            });*/
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
        /// If autotracking is currently enabled
        /// </summary>
        public bool IsEnabled { get; private set; } = false;

        /// <summary>
        /// Enables autotracking and listening for connections
        /// </summary>
        public void Enable()
        {
            _logger.LogInformation("Auto tracker Enabled");
            IsEnabled = true;
            _ = Task.Factory.StartNew(() => StartServer());
            AutoTrackerEnabled?.Invoke(this, new());
        }

        /// <summary>
        /// Disconnects current connections and prevents autotracking
        /// </summary>
        public void Disable()
        {
            _logger.LogInformation("Auto tracker Disabled");
            IsEnabled = false;
            if (_tcpListener != null) {
                _tcpListener.Stop();
            }
            if (_socket != null && _socket.Connected)
            {
                _socket.Close();
            }
            AutoTrackerDisabled?.Invoke(this, new());
        }

        /// <summary>
        /// Adds a memory request/response to be processed
        /// </summary>
        /// <param name="action">The command for the lua script to execute</param>
        /// <param name="domain"></param>
        /// <param name="address">The initial memory address</param>
        /// <param name="length">The number of bytes to obtain</param>
        /// <param name="game">Which game(s) this should be executed over</param>
        /// <param name="response">The action to perform upon receiving a response</param>
        protected void AddEvent(string action, string domain, int address, int length, Game game, Action<AutoTrackerMessage> response)
        {
            var request = new AutoTrackerMessage()
            {
                Action = action,
                Domain = domain,
                Address = address,
                Length = length,
                Game = game
            };
            _requestMessages.Add(request);
            _responseActions.Add(address, response);
            _previousResponses.Add(address, new());
        }

        /// <summary>
        /// Starts a listen server for the lua script to connect to
        /// </summary>
        protected void StartServer()
        {
            _tcpListener = new TcpListener(IPAddress.Loopback, 6969);
            _tcpListener.Start();
            while (IsEnabled)
            {
                try
                {
                    _socket = _tcpListener.AcceptSocket();
                    if (_socket.Connected)
                    {
                        using (var stream = new NetworkStream(_socket))
                        using (var writer = new StreamWriter(stream))
                        using (var reader = new StreamReader(stream))
                        {
                            try
                            {
                                Tracker.Say(x => x.AutoTracker.WhenConnected);
                                AutoTrackerConnected?.Invoke(this, new());
                                _ = Task.Factory.StartNew(() => SendMessages());
                                var line = reader.ReadLine();
                                while (line != null && _socket.Connected)
                                {
                                    var message = JsonSerializer.Deserialize<AutoTrackerMessage>(line);
                                    if (message != null)
                                    {
                                        _responseActions[message.Address].Invoke(message);
                                    }
                                    line = reader.ReadLine();
                                }
                            }
                            catch (Exception ex)
                            {
                                AutoTrackerDisconnected?.Invoke(this, new());
                                _logger.LogError(ex, "Error sending message");
                            }
                        }

                    }
                }
                catch (SocketException se)
                {
                    _logger.LogError(se, "Error in accepting socket");
                }
            }
        }

        /// <summary>
        /// Sends requests out to the connected lua script
        /// </summary>
        protected void SendMessages()
        {
            _logger.LogInformation("Start sending");
            while (_socket != null && _socket.Connected)
            {
                try
                {
                    while (!_requestMessages[_currentRequestIndex].ShouldSend(_currentGame, _hasStarted))
                    {
                        _currentRequestIndex = (_currentRequestIndex + 1) % _requestMessages.Count;
                    }

                    var message = JsonSerializer.Serialize(_requestMessages[_currentRequestIndex]) + "\0";
                    _socket.Send(Encoding.ASCII.GetBytes(message));
                    _currentRequestIndex = (_currentRequestIndex + 1) % _requestMessages.Count;
                    Task.Delay(TimeSpan.FromSeconds(0.25f)).Wait();
                }
                catch (Exception e)
                {
                    AutoTrackerDisconnected?.Invoke(this, new());
                    _logger.LogError(e.StackTrace, "Error sending message");
                    break;
                }
            }
        }

        /*
        /// <summary>
        /// Tracks changes to the Zelda inventory in memory
        /// </summary>
        /// <param name="message"></param>
        ///
        protected void CheckZeldaInventory(AutoTrackerMessage message)
        {
            foreach (var item in Tracker.Items.Where(x => x.InternalItemType.IsInCategory(ItemCategory.Zelda) && x.MemoryAddress != null))
            {
                var location = item.MemoryAddress ?? 0;
                var giveItem = false;

                if (item.InternalItemType == ItemType.Bottle)
                {
                    int numBottles = 0;
                    for (var i = 0; i < 4; i++)
                    {
                        if (message.CompareUInt8(_previousResponses[message.Address], location + i, item.MemoryFlag ?? 0))
                        {
                            numBottles++;
                        }
                    }

                    if (numBottles > 0 && item.TrackingState < numBottles)
                    {
                        giveItem = true;
                    }
                }
                else if (item.InternalItemType == ItemType.Mirror)
                {
                    var valueChanged = message.CompareUInt8(_previousResponses[message.Address], location, item.MemoryFlag);
                    var value = message.ReadUInt8(location);
                    if (valueChanged && (value == 1 || value == 2) && item.TrackingState == 0)
                    {
                        giveItem = true;
                    }
                }
                else if (item.Multiple && item.HasStages)
                {
                    var valueChanged = message.CompareUInt8(_previousResponses[message.Address], location, item.MemoryFlag);
                    var value = message.ReadUInt8(location);
                    if (valueChanged && item.TrackingState < value)
                    {
                        giveItem = true;
                    }
                }
                else
                {
                    giveItem = message.CompareUInt8(_previousResponses[message.Address], location, item.MemoryFlag) && item.TrackingState == 0;
                }

                if (giveItem)
                {
                    Tracker.TrackItem(item);
                }
            }
        }
        */

        /// <summary>
        /// Checks locations to see if they have accessed or not
        /// </summary>
        /// <param name="message">The response from the lua script</param>
        /// <param name="type">The type of location to find the correct LocationInfo objects</param>
        /// <param name="is16Bit">Set to true if this is a 16 bit value or false for 8 bit</param>
        /// <param name="game">The game that is being checked</param>
        protected void CheckLocations(AutoTrackerMessage message, LocationMemoryType type, bool is16Bit, Game game)
        {
            foreach (var location in Tracker.World.Locations.Where(x => x.MemoryType == type && ((game == Game.SM && x.Id < 256) || (game == Game.Zelda && x.Id >= 256))))
            {
                try
                {
                    var loc = location.MemoryAddress ?? 0;
                    var flag = location.MemoryFlag ?? 0;
                    if (!location.Cleared && ((is16Bit && message.CheckUInt16(loc * 2, flag)) || (!is16Bit && message.CheckUInt8(loc, flag))))
                    {
                        var item = Tracker.Items.SingleOrDefault(x => x.InternalItemType == location.Item.Type);
                        if (item != null)
                        {
                            Tracker.TrackItem(item, location, null, null, true);
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
                    _logger.LogError("Unable to auto track location: " + location.Name);
                    _logger.LogError(e.Message);
                    _logger.LogTrace(e.StackTrace);
                    Tracker.Error();
                }
            }
        }

        /// <summary>
        /// Checks the status of if dungeons have been cleared
        /// </summary>
        /// <param name="message">The response from the lua script</param>
        protected void CheckDungeons(AutoTrackerMessage message)
        {
            foreach (var dungeonInfo in Tracker.WorldInfo.Dungeons)
            {
                var region = Tracker.World.Regions.First(x => dungeonInfo.Is(x) && x is Z3Region) as Z3Region;
                if (region == null)
                {
                    _logger.LogError($"Could not find region for {dungeonInfo.Name}");
                    continue;
                }

                try
                {
                    if (!dungeonInfo.Cleared && message.CheckUInt16(region.MemoryAddress * 2 ?? 0, region.MemoryFlag ?? 0))
                    {
                        Tracker.MarkDungeonAsCleared(dungeonInfo);
                        _logger.LogInformation($"Auto tracked {dungeonInfo.Name} as cleared");
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError("Unable to auto track Dungeon: " + dungeonInfo.Name);
                    _logger.LogError(e.Message);
                    _logger.LogTrace(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Checks the status of if the Super Metroid bosses have been defeated
        /// </summary>
        /// <param name="message">The response from the lua script</param>
        protected void CheckSMBosses(AutoTrackerMessage message)
        {
            var boss = Tracker.WorldInfo.Bosses.First(x => "Kraid".Equals(x.Name[0], StringComparison.OrdinalIgnoreCase));
            if (!boss.Defeated && message.CheckUInt8(0x1, 0x1))
            {
                Tracker.MarkBossAsDefeated(boss);
                _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
            }

            boss = Tracker.WorldInfo.Bosses.First(x => "Ridley".Equals(x.Name[0], StringComparison.OrdinalIgnoreCase));
            if (!boss.Defeated && message.CheckUInt8(0x2, 0x1))
            {
                Tracker.MarkBossAsDefeated(boss);
                _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
            }

            boss = Tracker.WorldInfo.Bosses.First(x => "Phantoon".Equals(x.Name[0], StringComparison.OrdinalIgnoreCase));
            if (!boss.Defeated && message.CheckUInt8(0x3, 0x1))
            {
                Tracker.MarkBossAsDefeated(boss);
                _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
            }

            boss = Tracker.WorldInfo.Bosses.First(x => "Draygon".Equals(x.Name[0], StringComparison.OrdinalIgnoreCase));
            if (!boss.Defeated && message.CheckUInt8(0x4, 0x1))
            {
                Tracker.MarkBossAsDefeated(boss);
                _logger.LogInformation($"Auto tracked {boss.Name} as defeated");
            }
        }

        /// <summary>
        /// Tracks the current memory state of LttP for Tracker voice lines
        /// </summary>
        /// <param name="message">The message from the emulator with the memory state</param>
        protected void ZeldaStateChecks(AutoTrackerMessage message)
        {
            AutoTrackerZeldaState state = new(message);
            _logger.LogDebug(state.ToString());
            if (_previousZeldaState == null)
            {
                _previousZeldaState = state;
                return;
            }

            // Changed overworld (commented out for now as apparently SMZ3 breaks the memory update when transitioning worlds)
            /* if (state.OverworldValue == _previousZeldaState.OverworldValue && (state.OverworldValue == 0x00 || state.OverworldValue == 0x40) && state.OverworldValue != _previousZeldaOverworldValue)
            {
                Tracker.UpdateRegion(state.OverworldValue == 0x40 ? Tracker.World.LightWorldSouth : Tracker.World.DarkWorldSouth);
                _previousZeldaOverworldValue = state.OverworldValue;
                _previousMetroidRegionValue = -1;
            }*/

            // Falling down from Moldorm (detect if player was in Moldorm room and is now in the room below it)
            if (state.CurrentRoom == 23 && state.PreviousRoom == 7 && _previousZeldaState.CurrentRoom == 7)
            {
                SayOnce(Tracker.Responses.AutoTracker.FallFromMoldorm);
            }
            // Falling down from Ganon (detect if player was in Ganon room and is now in the room below it)
            else if (state.CurrentRoom == 16 && state.PreviousRoom == 0 && _previousZeldaState.CurrentRoom == 0)
            {
                SayOnce(Tracker.Responses.AutoTracker.FallFromGanon);
            }
            // Hera pot (player is in the pot room but does not have the big key)
            else if (state.CurrentRoom == 167 && _previousZeldaState.CurrentRoom == 119 && Tracker.Items.First(x => x.InternalItemType == ItemType.BigKeyTH).TrackingState == 0)
            {
                SayOnce(Tracker.Responses.AutoTracker.HeraPot);
            }
            // Ice breaker (player is on the right side of the wall but was previous in the room to the left)
            else if (state.CurrentRoom == 31 && state.PreviousRoom == 30 && state.LinkX >= 0x48 && _previousZeldaState.LinkX < 0x48 && state.IsOnRightHalfOfRoom && _previousZeldaState.IsOnRightHalfOfRoom)
            {
                SayOnce(Tracker.Responses.AutoTracker.IceBreaker);
            }
            // Back Diver Down (player is now at the lower section and on the ground, but not from the ladder)
            else if (state.CurrentRoom == 118 && state.LinkX < 0x9F && state.LinkX != 0x68 && state.LinkY <= 0x98 && _previousZeldaState.LinkY > 0x98 && state.LinkState == 0 && state.IsOnBottomHalfOfroom)
            {
                SayOnce(Tracker.Responses.AutoTracker.DiverDown);
            }
            // Entered a dungeon (now in Dungeon state but was previously in Overworld or entering Dungeon state)
            else if (state.State == 0x07 && (_previousZeldaState.State == 0x06 || _previousZeldaState.State == 0x09 || _previousZeldaState.State == 0x0F || _previousZeldaState.State == 0x10 || _previousZeldaState.State == 0x11))
            {
                // Get the region for the room 
                var region = Tracker.World.Regions.Where(x => x is Z3Region)
                    .Select(x => x as Z3Region)
                    .FirstOrDefault(x => x != null && x.StartingRooms != null && x.StartingRooms.Contains(state.CurrentRoom) && !x.IsOverworld);
                if (region == null) return;

                // Get the dungeon info for the room
                var dungeonInfo = Tracker.WorldInfo.Dungeons.First(x => x.Is(region));

                if (!_enteredDungeons.Contains(dungeonInfo) && (dungeonInfo.Reward == RewardItem.RedPendant || dungeonInfo.Reward == RewardItem.GreenPendant || dungeonInfo.Reward == RewardItem.BluePendant))
                {
                    Tracker.Say(x => x.AutoTracker.EnterPendantDungeon, dungeonInfo.Name, dungeonInfo.Reward.GetName());
                }
                else if (region is CastleTower)
                {
                    SayOnce(Tracker.Responses.AutoTracker.EnterHyruleCastleTower);
                }

                Tracker.UpdateRegion(region, Tracker.Options.AutoTrackerChangeMap);
                _enteredDungeons.Add(dungeonInfo);
            }
            // Changed overworld (either the state was changed to overworld or the overworld screen changed)
            else if (state.State == 0x09 && (_previousZeldaState.State != 0x09 || state.OverworldScreen != _previousZeldaState.OverworldScreen))
            {
                // Get the region for the room 
                var region = Tracker.World.Regions.Where(x => x is Z3Region)
                    .Select(x => x as Z3Region)
                    .FirstOrDefault(x => x != null && x.StartingRooms != null && x.StartingRooms.Contains(state.OverworldScreen) && x.IsOverworld);
                if (region == null) return;

                Tracker.UpdateRegion(region, Tracker.Options.AutoTrackerChangeMap);
            }
            // Death
            else if (state.State is 0x5 or 0x0E or 0x1B && _previousZeldaState.State == 0x12)
            {
                // Say specific message for dying in the particular screen/room the player is in
                if (Tracker.CurrentRegion != null && Tracker.CurrentRegion.WhenDiedInRoom != null)
                {
                    var region = Tracker.CurrentRegion.GetRegion(Tracker.World) as Z3Region;
                    if (region != null && region.IsOverworld && Tracker.CurrentRegion.WhenDiedInRoom.ContainsKey(_previousZeldaState.OverworldScreen.ToString()))
                    {
                        Tracker.Say(Tracker.CurrentRegion.WhenDiedInRoom[_previousZeldaState.OverworldScreen.ToString()]);
                    }
                    else if (region != null && !region.IsOverworld && Tracker.CurrentRegion.WhenDiedInRoom.ContainsKey(_previousZeldaState.CurrentRoom.ToString()))
                    {
                        Tracker.Say(Tracker.CurrentRegion.WhenDiedInRoom[_previousZeldaState.CurrentRoom.ToString()]);
                    }
                }
                
                Tracker.TrackItem(Tracker.Items.First(x => x.ToString().Equals("Death", StringComparison.OrdinalIgnoreCase)));
            }
            // Swimming without flippers
            else if (state.LinkState == 0x04 && _previousZeldaState.LinkState == 0x04 && Tracker.Items.Any(x => x.InternalItemType == ItemType.Flippers && x.TrackingState == 0))
            {
                SayOnce(Tracker.Responses.AutoTracker.FakeFlippers);
            }

            _previousZeldaState = state;
        }

        /// <summary>
        /// Tracks the current memory state of SM for Tracker voice lines
        /// </summary>
        /// <param name="message">The message from the emulator with the memory state</param>
        protected void MetroidStateChecks(AutoTrackerMessage message)
        {
            AutoTrackerMetroidState state = new(message);
            _logger.LogDebug(state.ToString());
            if (_previousMetroidState == null)
            {
                _previousMetroidState = state;
                return;
            }

            // Update the region that the player is currently in
            if (state.CurrentRegion != _previousMetroidRegionValue)
            {
                var newRegion = Tracker.World.Regions.Select(x => x as SMRegion).FirstOrDefault(x => x != null && x.MemoryRegionId == state.CurrentRegion);
                if (newRegion != null)
                {
                    Tracker.UpdateRegion(newRegion, Tracker.Options.AutoTrackerChangeMap);
                }
                _previousMetroidRegionValue = state.CurrentRegion;
            }

            // Approaching Kraid's Awful Son
            if (state.CurrentRegion == 1 && state.CurrentRoomInRegion == 45 && _previousMetroidState.CurrentRoomInRegion == 44)
            {
                SayOnce(Tracker.Responses.AutoTracker.NearKraidsAwfulSon);
            }
            // Approaching Shaktool
            else if (state.CurrentRegion == 4 && state.CurrentRoomInRegion == 36 && _previousMetroidState.CurrentRoomInRegion == 28)
            {
                SayOnce(Tracker.Responses.AutoTracker.NearShaktool);
            }
            // Approaching Crocomire
            else if (state.CurrentRegion == 2 && state.CurrentRoomInRegion == 9 && state.SamusX >= 3000 && state.SamusY > 500)// && !Tracker.WorldInfo.Bosses.First(x => "Crocomire".Equals(x.Name[0])).Defeated)
            {
                SayOnce(Tracker.Responses.AutoTracker.NearCrocomire, state.SuperMissiles, state.MaxSuperMissiles);
            }
            // Death (health and reserve tanks all 0 (have to check to make sure the player isn't warping between games)
            else if (state.Health == 0 && state.ReserveTanks == 0 && _previousMetroidState.Health != 0 && !(state.CurrentRoom == 0 && state.CurrentRegion == 0 && state.SamusY == 0))
            {
                if (Tracker.CurrentRegion != null && Tracker.CurrentRegion.WhenDiedInRoom != null && Tracker.CurrentRegion.WhenDiedInRoom.ContainsKey(state.CurrentRoomInRegion.ToString()))
                {
                    Tracker.Say(Tracker.CurrentRegion.WhenDiedInRoom[state.CurrentRoomInRegion.ToString()]);
                }
                Tracker.TrackItem(Tracker.Items.First(x => x.ToString().Equals("Death", StringComparison.OrdinalIgnoreCase)));
            }
            
            _previousMetroidState = state;
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
        /// Called when the module is destroyed
        /// </summary>
        public void Dispose() {
            Disable();
        }
    }

}
