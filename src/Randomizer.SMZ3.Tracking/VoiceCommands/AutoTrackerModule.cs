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
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class AutoTrackerModule : TrackerModule
    {
        private static readonly JsonSerializerOptions s_serializerOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };


        private Tracker _tracker;
        private ILogger<AutoTrackerModule> _logger;
        private bool keepListening = true;
        List<AutoTrackerMessage> _requestMessages = new List<AutoTrackerMessage>();
        Dictionary<int, Action<AutoTrackerMessage>> _responseActions = new Dictionary<int, Action<AutoTrackerMessage>>();
        Dictionary<int, int[]> _previousResponses = new Dictionary<int, int[]>();
        int currentRequestIndex = 0;
        bool isInSM = false;
        bool isInLttP = false;
        Game currentGame;

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
            logger.LogInformation("Auto Tracker Started");
            _ = Task.Factory.StartNew(() => StartServer());

            // Active game
            AddEvent("read_block", CorrectSRAMAddress(0xa173fe), 0x1, Game.Both, (AutoTrackerMessage message) =>
            {
                var value = Read8(message.Bytes, 0);
                isInLttP = value == 0x00;
                isInSM = value == 0xff;
                currentGame = isInLttP ? Game.Zelda : Game.SM;
                _logger.LogInformation("IsInLttp: " + isInLttP + " | IsInSM: " + isInSM);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Zelda Room Locations
            AddEvent("read_block", CorrectSRAMAddress(0x7ef000), 0x250, Game.Zelda, (AutoTrackerMessage message) =>
            {
                CheckLocations(message, LocationMemoryType.ZeldaRoom, true, true);
                CheckDungeons(message);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Zelda NPC Locations
            AddEvent("read_block", 0x7ef410, 0x2, Game.Zelda, (AutoTrackerMessage message) =>
            {
                CheckLocations(message, LocationMemoryType.ZeldaNPC, false, true);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Zelda Overworld Locations
            AddEvent("read_block", 0x7ef280, 0x82, Game.Zelda, (AutoTrackerMessage message) =>
            {
                CheckLocations(message, LocationMemoryType.ZeldaOverworld, false, true);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Zelda items while playing Zelda
            AddEvent("read_block", 0x7ef300, 0xD0, Game.Zelda, (AutoTrackerMessage message) =>
            {
                // CheckZeldaItemMemory
                CheckLocations(message, LocationMemoryType.ZeldaMisc, false, true);
                _previousResponses[message.Address] = message.Bytes;
            });

            // Super Metroid locations
            AddEvent("read_block", 0x7ed870, 0x20, Game.SM, (AutoTrackerMessage message) =>
            {
                CheckLocations(message, LocationMemoryType.SMLocation, false, true);
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

        protected void StartServer()
        {
            var listener = new TcpListener(IPAddress.Loopback, 43884);
            listener.Start();
            while (keepListening)
            {
                var socket = listener.AcceptSocket();
                if (socket.Connected)
                {
                    using (var stream = new NetworkStream(socket))
                    using (var writer = new StreamWriter(stream))
                    using (var reader = new StreamReader(stream))
                    {
                        try
                        {
                            _ = Task.Factory.StartNew(() => SendMessages(socket));
                            var line = reader.ReadLine();
                            while (line != null && socket.Connected)
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
                            _logger.LogError(DateTime.Now.ToString() + " !! -- " + ex.Message);
                        }
                    }

                }
            }
        }

        protected void SendMessages(Socket socket)
        {
            _logger.LogInformation("Start sending");
            while (socket.Connected)
            {
                try
                {
                    while (_requestMessages[currentRequestIndex].Game != Game.Both && _requestMessages[currentRequestIndex].Game != currentGame)
                    {
                        currentRequestIndex = (currentRequestIndex + 1) % _requestMessages.Count;
                    }

                    var message = JsonSerializer.Serialize(_requestMessages[currentRequestIndex]) + "\0";
                    socket.Send(Encoding.ASCII.GetBytes(message));
                    currentRequestIndex = (currentRequestIndex + 1) % _requestMessages.Count;
                    Task.Delay(TimeSpan.FromSeconds(0.25f)).Wait();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogTrace(e.StackTrace);
                    break;
                }
            }
        }

        protected void CheckZeldaInventory(AutoTrackerMessage message)
        {
            foreach (ItemData item in _tracker.Items.Where(x => x.InternalItemType.IsInCategory(ItemCategory.Zelda) && x.MemoryAddress != null))
            {
                var location = Convert.ToInt32(item.MemoryAddress, 16);
                var giveItem = false;

                if (item.InternalItemType == ItemType.Bottle)
                {
                    int numBottles = 0;
                    for (var i = 0; i < 4; i++)
                    {
                        if (Compare8(message.Bytes, message.Address, location + i, item.MemoryFlag))
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
                    var valueChanged = Compare8(message.Bytes, message.Address, location, item.MemoryFlag);
                    var value = Read8(message.Bytes, location);
                    if (valueChanged && (value == 1 || value == 2) && item.TrackingState == 0)
                    {
                        giveItem = true;
                    }
                }
                else if (item.Multiple && item.HasStages)
                {
                    var valueChanged = Compare8(message.Bytes, message.Address, location, item.MemoryFlag);
                    var value = Read8(message.Bytes, location);
                    if (valueChanged && item.TrackingState < value)
                    {
                        giveItem = true;
                    }
                }
                else
                {
                    giveItem = Compare8(message.Bytes, message.Address, location, item.MemoryFlag) && item.TrackingState == 0;
                }

                if (giveItem)
                {
                    _tracker.TrackItem(item);
                }
            }
        }

        protected void CheckLocations(AutoTrackerMessage message, LocationMemoryType type, bool is16, bool compareValue)
        {
            foreach (var locationInfo in _tracker.WorldInfo.Locations.Where(x => x.MemoryType == type))
            {
                try
                {
                    var memLocation = Convert.ToInt32(locationInfo.MemoryAddress, 16);
                    var memFlag = Convert.ToInt32(locationInfo.MemoryFlag, 16);
                    var location = _tracker.World.Locations.Where(x => x.Id == locationInfo.Id).First();
                    if (!location.Cleared && ((is16 && Check16(message.Bytes, memLocation, memFlag) == compareValue) || (!is16 && Check8(message.Bytes, memLocation, memFlag) == compareValue)))
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

        protected void CheckDungeons(AutoTrackerMessage message)
        {
            foreach (var dungeonInfo in _tracker.WorldInfo.Dungeons)
            {
                try
                {
                    var memLocation = Convert.ToInt32(dungeonInfo.MemoryAddress, 16);
                    var memFlag = Convert.ToInt32(dungeonInfo.MemoryFlag, 16);
                    if (!dungeonInfo.Cleared && Check16(message.Bytes, memLocation, memFlag))
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

        protected void CheckSMBosses(AutoTrackerMessage message)
        {
            foreach (var bossInfo in _tracker.WorldInfo.Bosses)
            {
                try
                {
                    var memLocation = Convert.ToInt32(bossInfo.MemoryAddress, 16);
                    var memFlag = Convert.ToInt32(bossInfo.MemoryFlag, 16);
                    if (!bossInfo.Defeated && Check8(message.Bytes, memLocation, memFlag))
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

        protected int Read8(int[] bytes, int location)
        {
            return bytes[location];
        }

        protected bool Check8(int[] bytes, int location, int flag)
        {
            return (Read8(bytes, location) & flag) == flag;
        }

        protected bool Compare8(int[] bytes, int address, int location, string? flag)
        {
            var prevValue = _previousResponses[address].Length > location ? Read8(_previousResponses[address], location) : -1;
            var newValue = Read8(bytes, location);

            if (newValue > prevValue)
            {
                if (flag != null)
                {
                    var flagVal = Convert.ToInt32(flag, 16);
                    if ((newValue & flagVal) == flagVal)
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
        
        protected int Read16(int[] bytes, int location)
        {
            return bytes[location * 2 + 1] * 256 + bytes[location * 2];
        }

        protected bool Check16(int[] bytes, int location, int flag)
        {
            var data = Read16(bytes, location);
            var adjustedFlag = 1 << flag;
            var temp = data & adjustedFlag;
            return temp == adjustedFlag;
        }

        protected int CorrectSRAMAddress(int address)
        {
            return address;
        }
    }
}
