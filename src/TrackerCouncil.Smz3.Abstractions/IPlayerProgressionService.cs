using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

/// <summary>
/// Defines methods for retrieving the progression and individual tracking statuses
/// </summary>
public interface IPlayerProgressionService
{
    /// <summary>
    /// Enumarates all currently tracked items for the local player.
    /// </summary>
    /// <returns>
    /// A collection of items that have been tracked at least once.
    /// </returns>
    IEnumerable<Item> TrackedItems();

    /// <summary>
    /// Indicates whether an item of the specified type has been tracked
    /// for the local player.
    /// </summary>
    /// <param name="itemType">The type of item to check.</param>
    /// <returns>
    /// <see langword="true"/> if an item with the specified type has been
    /// tracked at least once; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsTracked(ItemType itemType);

    /// <summary>
    /// Enumarates all currently tracked rewards for the local player.
    /// </summary>
    /// <returns>
    /// A collection of reward that have been tracked.
    /// </returns>
    IEnumerable<Reward> TrackedRewards();

    /// <summary>
    /// Enumarates all currently tracked bosses for the local player.
    /// </summary>
    /// <returns>
    /// A collection of bosses that have been tracked.
    /// </returns>
    IEnumerable<Boss> TrackedBosses();

    /// <summary>
    /// Retrieves the progression containing all of the tracked items, rewards, and bosses
    /// for determining in logic locations
    /// </summary>
    /// <param name="assumeKeys">If it should be assumed that the player has all keys and keycards</param>
    /// <returns></returns>
    Progression GetProgression(bool assumeKeys);

    /// <summary>
    /// Retrieves the progression containing all of the tracked items, rewards, and bosses
    /// for determining in logic locations
    /// </summary>
    /// <param name="area">The area being looked at to see if keys/keycards should be assumed or not</param>
    /// <returns></returns>
    Progression GetProgression(IHasLocations area);

    /// <summary>
    /// Retrieves the progression containing all of the tracked items, rewards, and bosses
    /// for determining in logic locations
    /// </summary>
    /// <param name="location">The location being looked at to see if keys/keycards should be assumed or not</param>
    /// <returns></returns>
    Progression GetProgression(Location location);

    /// <summary>
    /// Clears cached progression
    /// </summary>
    void ResetProgression();
}
