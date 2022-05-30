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
        private readonly Tracker _tracker;
        private readonly ILogger<AutoTrackerModule> _logger;
        private readonly List<AutoTrackerMessage> _requestMessages = new();
        private readonly Dictionary<int, Action<AutoTrackerMessage>> _responseActions = new();
        private readonly Dictionary<int, int[]> _previousResponses = new();
        private int _currentRequestIndex = 0;
        private Game _currentGame;
        private TcpListener? _tcpListener = null;
        private bool _hasStarted;
        private Socket? _socket = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BossTrackingModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to write logging information.</param>
        public AutoTrackerModule(Tracker tracker, ILogger<AutoTrackerModule> logger) : base(tracker, logger)
        {
            _tracker = tracker;
            _logger = logger;
            _tracker.AutoTracker = this;

            // 7e0010 - Can tell if in OW or Dungeon
            // 7e00A0 - Can tell the room, which can probably be used to determine dungeon

            // Check if the game has started. SM locations start as cleared in memory until you get to the title screen.
            AddEvent("read_block", 0x7e0020, 0x1, Game.Neither, (AutoTrackerMessage message) =>
            {
                var value = ReadUInt8(message.Bytes, 0);
                if (value != 0)
                {
                    _logger.LogInformation("Game started");
                    _hasStarted = true;
                    Tracker.Say(x => x.Autotracker.GameStarted, Tracker.Rom.Seed);
                }
                _previousResponses[message.Address] = message.Bytes;
            });

            // Active game
            AddEvent("read_block", 0xa173fe, 0x1, Game.Both, (AutoTrackerMessage message) =>
            {
                var value = ReadUInt8(message.Bytes, 0);
                _currentGame = value == 0x00 ? Game.Zelda : Game.SM;
                _logger.LogInformation($"Game changed to: {_currentGame}");
                _previousResponses[message.Address] = message.Bytes;
            });

            // Zelda Room Locations
            AddEvent("read_block", 0x7ef000, 0x250, Game.Zelda, (AutoTrackerMessage message) =>
            {
                CheckLocations(message, LocationMemoryType.ZeldaRoom, true);
                CheckDungeons(message);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Zelda NPC Locations
            AddEvent("read_block", 0x7ef410, 0x2, Game.Zelda, (AutoTrackerMessage message) =>
            {
                CheckLocations(message, LocationMemoryType.ZeldaNPC, false);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Zelda Overworld Locations
            AddEvent("read_block", 0x7ef280, 0x82, Game.Zelda, (AutoTrackerMessage message) =>
            {
                CheckLocations(message, LocationMemoryType.ZeldaOverworld, false);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Zelda items while playing Zelda
            AddEvent("read_block", 0x7ef300, 0xD0, Game.Zelda, (AutoTrackerMessage message) =>
            {
                // CheckZeldaItemMemory
                CheckLocations(message, LocationMemoryType.ZeldaMisc, false);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Super Metroid locations
            AddEvent("read_block", 0x7ed870, 0x20, Game.SM, (AutoTrackerMessage message) =>
            {
                CheckLocations(message, LocationMemoryType.SMLocation, false);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Super Metroid bosses
            AddEvent("read_block", 0x7ed828, 0x08, Game.SM, (AutoTrackerMessage message) =>
            {
                CheckSMBosses(message);
                _previousResponses[message.Address] = message.Bytes;
            });


            // SM items while playing Zelda
            /*AddEvent("read_block", CorrectSRAMAddress(0xa17900), 0x10, Game.Zelda, (AutoTrackerMessage message) =>
            {
                var speed = Check8(message.Bytes, 0x03, 0x20);
                var plasma = Check8(message.Bytes, 0x06, 0x08);
                var gravity = Check8(message.Bytes, 0x02, 0x20);
                var bombs = Check8(message.Bytes, 0x03, 0x10);
                _logger.LogInformation($"speed {speed} | plasma {plasma} | Gravity suit {gravity} | morph bombs {bombs}");
                _previousResponses[message.Address] = message.Bytes;
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
            _logger.LogInformation("Autotracker Enabled");
            IsEnabled = true;
            _ = Task.Factory.StartNew(() => StartServer());
            AutoTrackerEnabled?.Invoke(this, new());
        }

        /// <summary>
        /// Disconnects current connections and prevents autotracking
        /// </summary>
        public void Disable()
        {
            _logger.LogInformation("Autotracker Disabled");
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
        /// <param name="address">The initial memory address</param>
        /// <param name="length">The number of bytes to obtain</param>
        /// <param name="game">Which game(s) this should be executed over</param>
        /// <param name="response">The action to perform upon receiving a response</param>
        protected void AddEvent(string action, int address, int length, Game game, Action<AutoTrackerMessage> response)
        {
            var request = new AutoTrackerMessage()
            {
                Action = action,
                Address = address,
                Length = length,
                Game = game
            };
            _requestMessages.Add(request);
            _responseActions.Add(address, response);
            _previousResponses.Add(address, new int[0]);
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
                                Tracker.Say(x => x.Autotracker.WhenConnected);
                                AutoTrackerConnected?.Invoke(this, new());
                                _ = Task.Factory.StartNew(() => SendMessages());
                                var line = reader.ReadLine();
                                while (line != null && _socket.Connected)
                                {
                                    var message = JsonSerializer.Deserialize<AutoTrackerMessage>(line);
                                    if (message != null && _responseActions.ContainsKey(message.Address) && !Enumerable.SequenceEqual(_previousResponses[message.Address], message.Bytes))
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

        /// <summary>
        /// Tracks changes to the Zelda inventory in memory
        /// </summary>
        /// <param name="message"></param>
        protected void CheckZeldaInventory(AutoTrackerMessage message)
        {
            foreach (var item in _tracker.Items.Where(x => x.InternalItemType.IsInCategory(ItemCategory.Zelda) && x.MemoryAddress != null))
            {
                var location = item.MemoryAddress ?? 0;
                var giveItem = false;

                if (item.InternalItemType == ItemType.Bottle)
                {
                    int numBottles = 0;
                    for (var i = 0; i < 4; i++)
                    {
                        if (CompareUInt8(message.Bytes, message.Address, location + i, item.MemoryFlag ?? 0))
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
                    var valueChanged = CompareUInt8(message.Bytes, message.Address, location, item.MemoryFlag);
                    var value = ReadUInt8(message.Bytes, location);
                    if (valueChanged && (value == 1 || value == 2) && item.TrackingState == 0)
                    {
                        giveItem = true;
                    }
                }
                else if (item.Multiple && item.HasStages)
                {
                    var valueChanged = CompareUInt8(message.Bytes, message.Address, location, item.MemoryFlag);
                    var value = ReadUInt8(message.Bytes, location);
                    if (valueChanged && item.TrackingState < value)
                    {
                        giveItem = true;
                    }
                }
                else
                {
                    giveItem = CompareUInt8(message.Bytes, message.Address, location, item.MemoryFlag) && item.TrackingState == 0;
                }

                if (giveItem)
                {
                    _tracker.TrackItem(item);
                }
            }
        }

        /// <summary>
        /// Checks locations to see if they have accessed or not
        /// </summary>
        /// <param name="message">The response from the lua script</param>
        /// <param name="type">The type of location to find the correct LocationInfo objects</param>
        /// <param name="is16Bit">Set to true if this is a 16 bit value or false for 8 bit</param>
        protected void CheckLocations(AutoTrackerMessage message, LocationMemoryType type, bool is16Bit)
        {
            foreach (var locationInfo in _tracker.WorldInfo.Locations.Where(x => x.MemoryType == type))
            {
                try
                {
                    var loc = locationInfo.MemoryAddress ?? 0;
                    var flag = locationInfo.MemoryFlag ?? 0;
                    var location = _tracker.World.Locations.Where(x => x.Id == locationInfo.Id).First();
                    if (!location.Cleared && ((is16Bit && CheckUInt16(message.Bytes, loc, flag)) || (!is16Bit && CheckUInt8(message.Bytes, loc, flag))))
                    {
                        if (!location.Item.Type.IsInAnyCategory(ItemCategory.Map, ItemCategory.Compass))
                        {
                            _tracker.TrackItem(_tracker.Items.Where(x => x.InternalItemType == location.Item.Type).First(), location);
                            _logger.LogInformation($"Auto tracked {location.Item.Name} from {location.Name}");
                        }
                        else
                        {
                            _tracker.Clear(location);
                            _logger.LogInformation($"Auto tracked {location.Name} as cleared");
                        }
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError("Unable to auto track Zelda Room: " + locationInfo.Name);
                    _logger.LogError(e.Message);
                    _logger.LogTrace(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Checks the status of if dungeons have been cleared
        /// </summary>
        /// <param name="message">The response from the lua script</param>
        protected void CheckDungeons(AutoTrackerMessage message)
        {
            foreach (var dungeonInfo in _tracker.WorldInfo.Dungeons)
            {
                try
                {
                    if (!dungeonInfo.Cleared && CheckUInt16(message.Bytes, dungeonInfo.MemoryAddress ?? 0, dungeonInfo.MemoryFlag ?? 0))
                    {
                        _tracker.MarkDungeonAsCleared(dungeonInfo);
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
            foreach (var bossInfo in _tracker.WorldInfo.Bosses.Where(x => x.MemoryAddress != null))
            {
                try
                {
                    if (!bossInfo.Defeated && CheckUInt8(message.Bytes, bossInfo.MemoryAddress ?? 0, bossInfo.MemoryFlag ?? 0))
                    {
                        _tracker.MarkBossAsDefeated(bossInfo);
                        _logger.LogInformation($"Auto tracked {bossInfo.Name} as defeated");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("Unable to mark boss as defated: " + bossInfo.Name);
                    _logger.LogError(e.Message);
                    _logger.LogTrace(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Returns the memory value at a location
        /// </summary>
        /// <param name="bytes">All of the bytes to check</param>
        /// <param name="location">The offset location to check</param>
        /// <returns>The value from the byte array at that location</returns>
        protected static int ReadUInt8(int[] bytes, int location)
        {
            return bytes[location];
        }

        /// <summary>
        /// Gets the memory value for a location and returns if it matches a given flag
        /// </summary>
        /// <param name="bytes">All of the bytes to check</param>
        /// <param name="location">The offset location to check</param>
        /// <param name="flag">The flag to check against</param>
        /// <returns>True if the flag is set for the memory location.</returns>
        protected static bool CheckUInt8(int[] bytes, int location, int flag)
        {
            return (ReadUInt8(bytes, location) & flag) == flag;
        }

        /// <summary>
        /// Checks if a value in memory matches a flag or has been increased to denote obtaining an item
        /// </summary>
        /// <param name="bytes">All of the bytes to check</param>
        /// <param name="address">The origin location in memory</param>
        /// <param name="location">The offset location to check</param>
        /// <param name="flag">The flag to check against</param>
        /// <returns>True if the value in memory was set or increased</returns>
        protected bool CompareUInt8(int[] bytes, int address, int location, int? flag)
        {
            var prevValue = _previousResponses[address].Length > location ? ReadUInt8(_previousResponses[address], location) : -1;
            var newValue = ReadUInt8(bytes, location);

            if (newValue > prevValue)
            {
                if (flag != null)
                {
                    if ((newValue & flag) == flag)
                    {
                        return true;
                    }
                }
                else if(newValue > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the memory value at a location
        /// </summary>
        /// <param name="bytes">All of the bytes to check</param>
        /// <param name="location">The offset location to check</param>
        /// <returns>The value from the byte array at that location</returns>
        protected static int ReadUInt16(int[] bytes, int location)
        {
            return bytes[location * 2 + 1] * 256 + bytes[location * 2];
        }

        /// <summary>
        /// Gets the memory value for a location and returns if it matches a given flag
        /// </summary>
        /// <param name="bytes">All of the bytes to check</param>
        /// <param name="location">The offset location to check</param>
        /// <param name="flag">The flag to check against</param>
        /// <returns>True if the flag is set for the memory location.</returns>
        protected static bool CheckUInt16(int[] bytes, int location, int flag)
        {
            var data = ReadUInt16(bytes, location);
            var adjustedFlag = 1 << flag;
            var temp = data & adjustedFlag;
            return temp == adjustedFlag;
        }

        /// <summary>
        /// Called when the module is destroyed
        /// </summary>
        public void Dispose() {
            Disable();
        }
    }
}
