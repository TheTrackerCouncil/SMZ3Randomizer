using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking;

/// <summary>
/// Service that handles interacting with the game via
/// auto tracker
/// </summary>
public class GameService : TrackerModule, IGameService
{
    private AutoTrackerBase? AutoTracker => TrackerBase.AutoTracker;
    private ISnesConnectorService _snesConnectorService;
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
    /// <param name="snesConnectorService"></param>
    public GameService(TrackerBase tracker, IItemService itemService, IWorldService worldService, ILogger<GameService> logger, IWorldAccessor worldAccessor, ISnesConnectorService snesConnectorService)
        : base(tracker, itemService, worldService, logger)
    {
        TrackerBase.GameService = this;
        _logger = logger;
        _snesConnectorService = snesConnectorService;
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
            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7E033A,
                Data = new List<byte>() { 0, 0, 0, 0 }
            });
        }

        if (IsInGame(Game.Zelda))
        {
            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7E1E6B,
                Data = new List<byte>() { 0, 0, 0, 0 }
            });
        }
    }

    /// <summary>
    /// Gives an item to the player
    /// </summary>
    /// <param name="item">The item to give</param>
    /// <param name="fromPlayerId">The id of the player giving the item to the player (null for tracker)</param>
    /// <returns>False if it is currently unable to give an item to the player</returns>
    public async Task<bool> TryGiveItemAsync(Item item, int? fromPlayerId)
    {
        return await TryGiveItemsAsync(new List<Item>() { item }, fromPlayerId ?? _trackerPlayerId);
    }

    /// <summary>
    /// Gives a series of items to the player
    /// </summary>
    /// <param name="items">The list of items to give to the player</param>
    /// <param name="fromPlayerId">The id of the player giving the item to the player</param>
    /// <returns>False if it is currently unable to give an item to the player</returns>
    public async Task<bool> TryGiveItemsAsync(List<Item> items, int fromPlayerId)
    {
        if (!IsInGame())
        {
            return false;
        }

        TrackerBase.ItemTracker.TrackItems(items, true, true);

        return await TryGiveItemTypesAsync(items.Select(x => (x.Type, fromPlayerId)).ToList());
    }

    /// <summary>
    /// Gives a series of item types from particular players
    /// </summary>
    /// <param name="items">The list of item types and the players that are giving the item to the player</param>
    /// <returns>False if it is currently unable to give the items to the player</returns>
    public async Task<bool> TryGiveItemTypesAsync(List<(ItemType type, int fromPlayerId)> items)
    {
        if (!IsInGame())
        {
            return false;
        }

        // Get the first block of memory
        var firstBlockResponse = await _snesConnectorService.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA26000,
            Length = 0x300
        });

        var secondBlockResponse = await _snesConnectorService.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA26300,
            Length = 0x300
        });

        if (!firstBlockResponse.Successful || !firstBlockResponse.HasData || !secondBlockResponse.Successful ||
            !secondBlockResponse.HasData)
        {
            return false;
        }

        var firstDataSet = firstBlockResponse.Data;
        var secondDataSet = secondBlockResponse.Data;

        // Determine number of gifted items by looking at both sets of data.
        // Each item takes up two words and we're interested in the second word in each pair.
        var data = firstDataSet.Raw.Concat(secondDataSet.Raw).ToArray();
        var itemCounter = 0;
        for (var i = 2; i < 0x600; i += 4)
        {
            var item = (ItemType)BitConverter.ToUInt16(data.AsSpan(i, 2));
            if (item != ItemType.Nothing)
            {
                itemCounter++;
            }
        }

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

            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0xA26000 + (itemCounter * 4),
                Data = bytes
            });

            itemCounter += batch.Length;
        }

        // Up the item counter to have them actually pick it up
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA26602,
            Data = Int16ToBytes(itemCounter)
        });

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
            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7EF372,
                Data = new List<byte>() { 0xA0 }
            });

            return true;
        }
        else if (AutoTracker.CurrentGame == Game.SM && AutoTracker.MetroidState != null)
        {
            var maxHealth = AutoTracker.MetroidState.MaxEnergy ?? 100;
            var maxReserves = AutoTracker.MetroidState.MaxReserveTanks ?? 0;

            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7E09C2,
                Data = Int16ToBytes(maxHealth)
            });

            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7E09D6,
                Data = Int16ToBytes(maxReserves)
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

        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7EF373,
            Data = new List<byte>() { 0x80 }
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

        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7EF375,
            Data = new List<byte>() { 0xFF }
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

        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7EF376,
            Data = new List<byte>() { 0x80 }
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
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7EF360,
            Data = bytes.Concat(bytes).ToList()
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
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E09C6,
            Data = Int16ToBytes(maxMissiles)
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
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E09CA,
            Data = Int16ToBytes(maxSuperMissiles)
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
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E09CE,
            Data = Int16ToBytes(maxPowerBombs)
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
            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7EF36D,
                Data = new List<byte>() { 0x0 }
            });

            // Deal 1 heart of damage
            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7E0373,
                Data = new List<byte>() { 0x8 }
            });

            return true;
        }
        else if (AutoTracker.CurrentGame == Game.SM)
        {
            MarkRecentlyKilled();

            // Empty reserves
            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7E09D6,
                Data = new List<byte>() { 0x0, 0x0 }
            });

            // Set HP to 1 (to prevent saving with 0 energy)
            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7E09C2,
                Data = new List<byte>() { 0x1, 0x0 }
            });

            // Deal 255 damage to player
            _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.ExHiRom,
                Address = 0x7E0A50,
                Data = new List<byte>() { 0xFF }
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
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E09C2,
            Data = new List<byte>() { 0x32, 0x0 }
        });

        // Empty reserves
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E09D6,
            Data = new List<byte>() { 0x0, 0x0 }
        });

        // Fill missiles
        var maxMissiles = AutoTracker?.MetroidState?.MaxMissiles ?? 0;
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E09C6,
            Data = Int16ToBytes(maxMissiles)
        });

        // Fill super missiles
        var maxSuperMissiles = AutoTracker?.MetroidState?.MaxSuperMissiles ?? 0;
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E09CA,
            Data = Int16ToBytes(maxSuperMissiles)
        });

        // Fill power bombs
        var maxPowerBombs = AutoTracker?.MetroidState?.MaxPowerBombs ?? 0;
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E09CE,
            Data = Int16ToBytes(maxPowerBombs)
        });

        return true;
    }

    /// <summary>
    /// Sets the player to have a charged shinespark
    /// </summary>
    /// <returns>True if successful</returns>
    public bool TryChargeShinespark()
    {
        if (!IsInGame(Game.SM))
        {
            return false;
        }

        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E0A68, // Special Samus palette timer
            Data = Int16ToBytes(600) // Ten seconds
        });
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E0ACC, // Samus palette type
            Data = Int16ToBytes(1) // Speed booster shine
        });
        _snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.UpdateMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E0ACE, // Special Samus palette frame
            Data = Int16ToBytes(0)
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
            .Where(x => x.State.ItemWorldId == TrackerBase.World.Id && x.State.WorldId != TrackerBase.World.Id &&
                        x.State.Autotracked && (!x.World.HasCompleted || !x.Item.Type.IsInCategory(ItemCategory.IgnoreOnMultiplayerCompletion)))
            .Select(x => (x.State.Item, x.State.WorldId)).ToList();

        foreach (var item in previouslyGiftedItems)
        {
            otherCollectedItems.Remove(item);
        }

        if (otherCollectedItems.Any())
        {
            _logger.LogInformation("Giving player {ItemCount} missing items", otherCollectedItems.Count);
            _ = TryGiveItemTypesAsync(otherCollectedItems);
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

    public override void AddCommands()
    {

    }
}
