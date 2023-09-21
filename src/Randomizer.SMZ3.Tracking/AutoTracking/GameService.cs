using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Service that handles interacting with the game via
    /// auto tracker
    /// </summary>
    public class GameService : TrackerModule
    {
        private AutoTracker? AutoTracker => Tracker.AutoTracker;
        private readonly ILogger<GameService> _logger;
        private readonly int _trackerPlayerId;
        private int _itemCounter;
        private readonly Dictionary<int, EmulatorAction> _emulatorActions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">The logger to associate with this module</param>
        /// <param name="worldAccessor">The accesor to determine the tracker player id</param>
        public GameService(Tracker tracker, IItemService itemService, IWorldService worldService, ILogger<GameService> logger, IWorldAccessor worldAccessor)
            : base(tracker, itemService, worldService, logger)
        {
            Tracker.GameService = this;
            _logger = logger;
            _trackerPlayerId = worldAccessor.Worlds.Count > 0 ? worldAccessor.Worlds.Count : 0;
        }

        /// <summary>
        /// Updates memory values so both SM and Z3 will cancel any pending MSU resumes and play
        /// all tracks from the start until new resume points have been stored.
        /// </summary>
        /// <returns>True, even if it didn't do anything</returns>
        public void TryCancelMsuResume()
        {
            if (IsInGame(Game.SM))
            {
                // Zero out SM's NO_RESUME_AFTER_LO and NO_RESUME_AFTER_HI variables
                AutoTracker?.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E033A, // As declared in sm/msu.asm
                    WriteValues = new List<byte>() { 0, 0, 0, 0 }
                });
            }

            if (IsInGame(Game.Zelda))
            {
                // Zero out Z3's MSUResumeTime variable
                AutoTracker?.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E1E6B, // As declared in z3/randomizer/ram.asm
                    WriteValues = new List<byte>() { 0, 0, 0, 0 }
                });
            }
        }

        /// <summary>
        /// Gives an item to the player
        /// </summary>
        /// <param name="item">The item to give</param>
        /// <param name="fromPlayerId">The id of the player giving the item to the player (null for tracker)</param>
        /// <returns>False if it is currently unable to give an item to the player</returns>
        public bool TryGiveItem(Item item, int? fromPlayerId)
        {
            return TryGiveItems(new List<Item>() { item }, fromPlayerId ?? _trackerPlayerId);
        }

        /// <summary>
        /// Gives a series of items to the player
        /// </summary>
        /// <param name="items">The list of items to give to the player</param>
        /// <param name="fromPlayerId">The id of the player giving the item to the player</param>
        /// <returns>False if it is currently unable to give an item to the player</returns>
        public bool TryGiveItems(List<Item> items, int fromPlayerId)
        {
            if (!IsInGame())
            {
                return false;
            }

            Tracker.TrackItems(items, true, true);

            return TryGiveItemTypes(items.Select(x => (x.Type, fromPlayerId)).ToList());
        }

        /// <summary>
        /// Gives a series of item types from particular players
        /// </summary>
        /// <param name="items">The list of item types and the players that are giving the item to the player</param>
        /// <returns>False if it is currently unable to give the items to the player</returns>
        public bool TryGiveItemTypes(List<(ItemType type, int fromPlayerId)> items)
        {
            if (!IsInGame())
            {
                return false;
            }

            var tempItemCounter = _itemCounter;
            EmulatorAction action;

            // First give the player all of the requested items
            // Batch them into chunks of 50 due to byte limit for QUSB2SNES
            foreach (var batch in items.Chunk(50))
            {
                var bytes = new List<byte>();
                foreach (var item in batch)
                {
                    bytes.AddRange(Int16ToBytes(item.fromPlayerId));
                    bytes.AddRange(Int16ToBytes((int)item.type));
                }

                action = new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.CartRAM,
                    Address = 0xA26000 + (tempItemCounter * 4),
                    WriteValues = bytes
                };
                AutoTracker!.WriteToMemory(action);

                tempItemCounter += batch.Length;
            }

            // Up the item counter to have them actually pick it up
            action = new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.CartRAM,
                Address = 0xA26602,
                WriteValues = Int16ToBytes(tempItemCounter)
            };
            AutoTracker!.WriteToMemory(action);

            _itemCounter = tempItemCounter;

            return true;
        }

        /// <summary>
        /// Restores the player to max health
        /// </summary>
        /// <returns>False if it is currently unable to give an item to the player</returns>
        public bool TryHealPlayer()
        {
            if (!IsInGame())
            {
                return false;
            }

            if (AutoTracker!.CurrentGame == Game.Zelda)
            {
                AutoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7EF372,
                    WriteValues = new List<byte>() { 0xA0 }
                });

                return true;
            }
            else if (AutoTracker.CurrentGame == Game.SM && AutoTracker.MetroidState != null)
            {
                var maxHealth = AutoTracker.MetroidState.MaxEnergy;
                var maxReserves = AutoTracker.MetroidState.MaxReserveTanks;
                AutoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E09C2,
                    WriteValues = Int16ToBytes(maxHealth)
                });

                AutoTracker.WriteToMemory(new EmulatorAction()
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
            if (!IsInGame(Game.Zelda))
            {
                return false;
            }

            AutoTracker!.WriteToMemory(new EmulatorAction()
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
            if (!IsInGame(Game.Zelda))
            {
                return false;
            }

            AutoTracker!.WriteToMemory(new EmulatorAction()
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
            if (!IsInGame(Game.Zelda))
            {
                return false;
            }

            AutoTracker!.WriteToMemory(new EmulatorAction()
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
            if (!IsInGame(Game.Zelda))
            {
                return false;
            }

            var bytes = Int16ToBytes(2000);

            // Writing the target value to $7EF360 makes the rupee count start counting toward it.
            // Writing the target value to $7EF362 immediately sets the rupee count, but then it starts counting back toward where it was.
            // Writing the target value to both locations immediately sets the rupee count and keeps it there.
            AutoTracker!.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7EF360,
                WriteValues = bytes.Concat(bytes).ToList()
            });

            return true;
        }

        /// <summary>
        /// Fully fills the player's missiles
        /// </summary>
        /// <returns>False if it is currently unable to give missiles to the player</returns>
        public bool TryFillMissiles()
        {
            if (!IsInGame(Game.SM))
            {
                return false;
            }

            var maxMissiles = AutoTracker!.MetroidState?.MaxMissiles ?? 0;
            AutoTracker.WriteToMemory(new EmulatorAction()
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
            if (!IsInGame(Game.SM))
            {
                return false;
            }

            var maxSuperMissiles = AutoTracker!.MetroidState?.MaxSuperMissiles ?? 0;
            AutoTracker.WriteToMemory(new EmulatorAction()
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
            if (!IsInGame(Game.SM))
            {
                return false;
            }

            var maxPowerBombs = AutoTracker!.MetroidState?.MaxPowerBombs ?? 0;
            AutoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09CE,
                WriteValues = Int16ToBytes(maxPowerBombs)
            });

            return true;
        }

        /// <summary>
        /// Kills the player by removing their health and dealing damage to them
        /// </summary>
        /// <returns>True if successful</returns>
        public bool TryKillPlayer()
        {
            if (!IsInGame())
            {
                _logger.LogWarning("Could not kill player as they are not in game");
                return false;
            }

            if (AutoTracker!.CurrentGame == Game.Zelda)
            {
                MarkRecentlyKilled();

                // Set health to 0
                AutoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7EF36D,
                    WriteValues = new List<byte>() { 0x0 }
                });

                // Deal 1 heart of damage
                AutoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E0373,
                    WriteValues = new List<byte>() { 0x8 }
                });

                return true;
            }
            else if (AutoTracker.CurrentGame == Game.SM)
            {
                MarkRecentlyKilled();

                // Empty reserves
                AutoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E09D6,
                    WriteValues = new List<byte>() { 0x0, 0x0 }
                });

                // Set HP to 1 (to prevent saving with 0 energy)
                AutoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E09C2,
                    WriteValues = new List<byte>() { 0x1, 0x0 }
                });

                // Deal 255 damage to player
                AutoTracker.WriteToMemory(new EmulatorAction()
                {
                    Type = EmulatorActionType.WriteBytes,
                    Domain = MemoryDomain.WRAM,
                    Address = 0x7E0A50,
                    WriteValues = new List<byte>() { 0xFF }
                });

                return true;
            }

            _logger.LogWarning("Could not kill player as they are not in either Zelda or Metroid currently");
            return false;
        }

        /// <summary>
        /// Sets the player to have the requirements for a crystal flash
        /// </summary>
        /// <returns>True if successful</returns>
        public bool TrySetupCrystalFlash()
        {
            if (!IsInGame(Game.SM))
            {
                return false;
            }

            // Set HP to 50 health
            AutoTracker!.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09C2,
                WriteValues = new List<byte>() { 0x32, 0x0 }
            });

            // Empty reserves
            AutoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09D6,
                WriteValues = new List<byte>() { 0x0, 0x0 }
            });

            // Fill missiles
            var maxMissiles = AutoTracker.MetroidState?.MaxMissiles ?? 0;
            AutoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09C6,
                WriteValues = Int16ToBytes(maxMissiles)
            });

            // Fill super missiles
            var maxSuperMissiles = AutoTracker.MetroidState?.MaxSuperMissiles ?? 0;
            AutoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09CA,
                WriteValues = Int16ToBytes(maxSuperMissiles)
            });

            // Fill power bombs
            var maxPowerBombs = AutoTracker.MetroidState?.MaxPowerBombs ?? 0;
            AutoTracker.WriteToMemory(new EmulatorAction()
            {
                Type = EmulatorActionType.WriteBytes,
                Domain = MemoryDomain.WRAM,
                Address = 0x7E09CE,
                WriteValues = Int16ToBytes(maxPowerBombs)
            });

            return true;
        }

        /// <summary>
        /// Gives the player any items that tracker thinks they should have but are not in memory as having been gifted
        /// </summary>
        /// <param name="action"></param>
        public void SyncItems(EmulatorAction action)
        {
            if (AutoTracker?.HasValidState != true)
            {
                return;
            }

            _emulatorActions[action.Address] = action;

            if (!_emulatorActions.ContainsKey(0xA26000) || !_emulatorActions.ContainsKey(0xA26300) || !_emulatorActions.Values.All(x =>
                    x.LastRunTime != null && (DateTime.Now - x.LastRunTime.Value).TotalSeconds < 5))
            {
                return;
            }

            var data = _emulatorActions[0xA26000].CurrentData!.Raw.Concat(_emulatorActions[0xA26300].CurrentData!.Raw).ToArray();

            var previouslyGiftedItems = new List<(ItemType type, int fromPlayerId)>();
            for (var i = 0; i < 0x150; i++)
            {
                var item = (ItemType)BitConverter.ToUInt16(data.AsSpan(i * 4 + 2, 2));
                if (item == ItemType.Nothing)
                {
                    continue;
                }

                var playerId = BitConverter.ToUInt16(data.AsSpan(i * 4, 2));
                previouslyGiftedItems.Add((item, playerId));
            }

            _itemCounter = previouslyGiftedItems.Count;

            var otherCollectedItems = WorldService.Worlds.SelectMany(x => x.Locations)
                .Where(x => x.State.ItemWorldId == Tracker.World.Id && x.State.WorldId != Tracker.World.Id &&
                            x.State.Autotracked).Select(x => (x.State.Item, x.State.WorldId)).ToList();

            foreach (var item in previouslyGiftedItems)
            {
                otherCollectedItems.Remove(item);
            }

            if (otherCollectedItems.Any())
            {
                _logger.LogInformation("Giving player {ItemCount} missing items", otherCollectedItems.Count);
                TryGiveItemTypes(otherCollectedItems);
            }
        }

        /// <summary>
        /// If the player was recently killed by the game service
        /// </summary>
        public bool PlayerRecentlyKilled { get; private set; }

        private async void MarkRecentlyKilled()
        {
            PlayerRecentlyKilled = true;
            await Task.Delay(TimeSpan.FromSeconds(10));
            PlayerRecentlyKilled = false;
        }

        private static byte[] Int16ToBytes(int value)
        {
            var bytes = BitConverter.GetBytes((short)value).ToList();
            if (!BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            return bytes.ToArray();
        }

        private bool IsInGame(Game game = Game.Both)
        {
            if (AutoTracker is { IsConnected: true, IsInSMZ3: true, HasValidState: true })
            {
                return game == Game.Both || AutoTracker.CurrentGame == game;
            }
            return false;
        }
    }
}
