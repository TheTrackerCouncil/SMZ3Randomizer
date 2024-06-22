using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Shared;

namespace Randomizer.Abstractions;

public interface IGameService
{
    /// <summary>
    /// Updates memory values so both SM and Z3 will cancel any pending MSU resumes and play
    /// all tracks from the start until new resume points have been stored.
    /// </summary>
    /// <returns>True, even if it didn't do anything</returns>
    public void TryCancelMsuResume();

    /// <summary>
    /// Gives an item to the player
    /// </summary>
    /// <param name="item">The item to give</param>
    /// <param name="fromPlayerId">The id of the player giving the item to the player (null for tracker)</param>
    /// <returns>False if it is currently unable to give an item to the player</returns>
    public Task<bool> TryGiveItemAsync(Item item, int? fromPlayerId);

    /// <summary>
    /// Gives a series of items to the player
    /// </summary>
    /// <param name="items">The list of items to give to the player</param>
    /// <param name="fromPlayerId">The id of the player giving the item to the player</param>
    /// <returns>False if it is currently unable to give an item to the player</returns>
    public Task<bool> TryGiveItemsAsync(List<Item> items, int fromPlayerId);

    /// <summary>
    /// Gives a series of item types from particular players
    /// </summary>
    /// <param name="items">The list of item types and the players that are giving the item to the player</param>
    /// <returns>False if it is currently unable to give the items to the player</returns>
    public Task<bool> TryGiveItemTypesAsync(List<(ItemType type, int fromPlayerId)> items);

    /// <summary>
    /// Restores the player to max health
    /// </summary>
    /// <returns>False if it is currently unable to give an item to the player</returns>
    public bool TryHealPlayer();

    /// <summary>
    /// Fully fills the player's magic
    /// </summary>
    /// <returns>False if it is currently unable to give magic to the player</returns>
    public bool TryFillMagic();

    /// <summary>
    /// Fully fills the player's bombs to capacity
    /// </summary>
    /// <returns>False if it is currently unable to give bombs to the player</returns>
    public bool TryFillZeldaBombs();

    /// <summary>
    /// Fully fills the player's arrows
    /// </summary>
    /// <returns>False if it is currently unable to give arrows to the player</returns>
    public bool TryFillArrows();

    /// <summary>
    /// Fully fills the player's rupees (sets to 2000)
    /// </summary>
    /// <returns>False if it is currently unable to give rupees to the player</returns>
    public bool TryFillRupees();

    /// <summary>
    /// Fully fills the player's missiles
    /// </summary>
    /// <returns>False if it is currently unable to give missiles to the player</returns>
    public bool TryFillMissiles();

    /// <summary>
    /// Fully fills the player's super missiles
    /// </summary>
    /// <returns>False if it is currently unable to give super missiles to the player</returns>
    public bool TryFillSuperMissiles();

    /// <summary>
    /// Fully fills the player's power bombs
    /// </summary>
    /// <returns>False if it is currently unable to give power bombs to the player</returns>
    public bool TryFillPowerBombs();

    /// <summary>
    /// Kills the player by removing their health and dealing damage to them
    /// </summary>
    /// <returns>True if successful</returns>
    public bool TryKillPlayer();

    /// <summary>
    /// Sets the player to have the requirements for a crystal flash
    /// </summary>
    /// <returns>True if successful</returns>
    public bool TrySetupCrystalFlash();

    /// <summary>
    /// Gives the player any items that tracker thinks they should have but are not in memory as having been gifted
    /// </summary>
    /// <param name="action"></param>
    public void SyncItems(EmulatorAction action);

    /// <summary>
    /// If the player was recently killed by the game service
    /// </summary>
    public bool PlayerRecentlyKilled { get; }

}
