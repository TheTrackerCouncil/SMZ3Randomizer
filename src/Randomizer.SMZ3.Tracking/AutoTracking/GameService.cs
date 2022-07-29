﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Service that handles interacting with the game via
    /// auto tracker
    /// </summary>
    public class GameService : TrackerModule
    { 
        private AutoTracker? _autoTracker => Tracker.AutoTracker;
        private readonly ILogger<GameService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">The logger to associate with this module</param>
        public GameService(Tracker tracker, IItemService itemService, ILogger<GameService> logger)
            : base(tracker, itemService, logger)
        {
            Tracker.GameService = this;
            _logger = logger;
        }

        /// <summary>
        /// Gives an item to the player
        /// </summary>
        /// <param name="item">The item to give</param>
        /// <param name="fromPlayerId">The id of the player giving the item to the player (0 for tracker)</param>
        /// <returns>False if it is currently unable to give an item to the player</returns>
        public bool TryGiveItem(ItemData item, int fromPlayerId = 0)
        {
            if (_autoTracker == null || !_autoTracker.IsConnected)
            {
                return false;
            }

            // First give the player the item by the requested 
            var bytes = new List<byte>();
            bytes.AddRange(Int16ToBytes(fromPlayerId + 1));
            bytes.AddRange(Int16ToBytes((int)item.InternalItemType));
            var action = new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.CartRAM,
                Address = 0xA26000 + (ItemCounter * 4),
                WriteValues = bytes
            };
            _autoTracker.WriteToMemory(action);

            // Up the item counter to have them actually pick it up
            action = new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.CartRAM,
                Address = 0xA26602,
                WriteValues = Int16ToBytes(ItemCounter + 1)
            };
            _autoTracker.WriteToMemory(action);

            // Track the item
            Tracker.TrackItem(item,
                              trackedAs: null,
                              confidence: null,
                              tryClear: false,
                              autoTracked: true,
                              location: null,
                              giftedItem: true);

            ItemCounter++;

            return true;
        }

        /// <summary>
        /// Restores the player to max health
        /// </summary>
        /// <returns>False if it is currently unable to give an item to the player</returns>
        public bool TryHealPlayer()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected)
            {
                return false;
            }

            if (_autoTracker.CurrentGame == Game.Zelda)
            {
                _autoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7EF372,
                    WriteValues = new List<byte>() { 0xA0 }
                });

                return true;
            }
            else if (_autoTracker.CurrentGame == Game.SM && _autoTracker.MetroidState != null)
            {
                var maxHealth = _autoTracker.MetroidState.MaxEnergy;
                var maxReserves = _autoTracker.MetroidState.MaxReserveTanks;
                _autoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E09C2,
                    WriteValues = Int16ToBytes(maxHealth)
                });

                _autoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E09D6,
                    WriteValues = Int16ToBytes(maxReserves)
                });

                return true;
            }

            return false;
        }

        /// <summary>
        /// Fully fills the player's magic
        /// </summary>
        /// <returns>False if it is currently unable to give magic to the player</returns>
        public bool TryFillMagic()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected || _autoTracker.CurrentGame != Game.Zelda)
            {
                return false;
            }

            _autoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7EF373,
                WriteValues = new List<byte>() { 0x80 }
            });

            return true;
        }

        /// <summary>
        /// Fully fills the player's bombs to capacity
        /// </summary>
        /// <returns>False if it is currently unable to give bombs to the player</returns>
        public bool TryFillZeldaBombs()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected || _autoTracker.CurrentGame != Game.Zelda)
            {
                return false;
            }

            _autoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7EF375,
                WriteValues = new List<byte>() { 0xFF }
            });

            return true;
        }

        /// <summary>
        /// Fully fills the player's arrows
        /// </summary>
        /// <returns>False if it is currently unable to give arrows to the player</returns>
        public bool TryFillArrows()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected || _autoTracker.CurrentGame != Game.Zelda)
            {
                return false;
            }

            _autoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7EF376,
                WriteValues = new List<byte>() { 0x80 }
            });

            return true;
        }

        /// <summary>
        /// Fully fills the player's rupees (sets to 2000)
        /// </summary>
        /// <returns>False if it is currently unable to give rupees to the player</returns>
        public bool TryFillRupees()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected || _autoTracker.CurrentGame != Game.Zelda)
            {
                return false;
            }

            _autoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7EF360,
                WriteValues = new List<byte>() { 0x13, 0x88, 0x13, 0x88 }
            });

            return true;
        }

        /// <summary>
        /// Fully fills the player's missiles
        /// </summary>
        /// <returns>False if it is currently unable to give missiles to the player</returns>
        public bool TryFillMissiles()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected || _autoTracker.CurrentGame != Game.SM)
            {
                return false;
            }

            var maxMissiles = _autoTracker.MetroidState?.MaxMissiles ?? 0;
            _autoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09C6,
                WriteValues = Int16ToBytes(maxMissiles)
            });

            return true;
        }

        /// <summary>
        /// Fully fills the player's super missiles
        /// </summary>
        /// <returns>False if it is currently unable to give super missiles to the player</returns>
        public bool TryFillSuperMissiles()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected || _autoTracker.CurrentGame != Game.SM)
            {
                return false;
            }

            var maxSuperMissiles = _autoTracker.MetroidState?.MaxSuperMissiles ?? 0;
            _autoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09CA,
                WriteValues = Int16ToBytes(maxSuperMissiles)
            });

            return true;
        }

        /// <summary>
        /// Fully fills the player's power bombs
        /// </summary>
        /// <returns>False if it is currently unable to give power bombs to the player</returns>
        public bool TryFillPowerBombs()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected || _autoTracker.CurrentGame != Game.SM)
            {
                return false;
            }

            var maxPowerMissiles = _autoTracker.MetroidState?.MaxPowerBombs ?? 0;
            _autoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09CE,
                WriteValues = Int16ToBytes(maxPowerMissiles)
            });

            return true;
        }

        /// <summary>
        /// Kills the player by removing their health and dealing damage to them
        /// </summary>
        /// <returns>True if successful</returns>
        public bool TryKillPlayer()
        {
            if (_autoTracker == null || !_autoTracker.IsConnected)
            {
                return false;
            }

            if (_autoTracker.CurrentGame == Game.Zelda)
            {
                // Set health to 0
                _autoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7EF36D,
                    WriteValues = new List<byte>() { 0x0 }
                });

                // Deal 1 heart of damage
                _autoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E0373,
                    WriteValues = new List<byte>() { 0x8 }
                });

                return true;
            }
            else if (_autoTracker.CurrentGame == Game.SM)
            {
                // Set HP to 1 (to prevent saving with 0 energy)
                _autoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E09C2,
                    WriteValues = new List<byte>() { 0x1, 0x0 }
                });

                // Deal 255 damage to player
                _autoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E0A50,
                    WriteValues = new List<byte>() { 0xFF }
                });

                return true;
            }
            return false;
        }

        /// <summary>
        /// The number of items given to the player
        /// </summary>
        public int ItemCounter { get; set; }

        private static byte[] Int16ToBytes(int value)
        {
            return new byte[] { (byte)(value % 256), (byte)(value / 256) };
        }
    }
}
