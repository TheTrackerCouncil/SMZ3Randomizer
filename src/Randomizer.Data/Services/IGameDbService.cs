using System.Collections.Generic;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Services;

/// <summary>
/// Service for retrieving roms and multiplayer games from the database
/// </summary>
public interface IGameDbService
{
    /// <summary>
    /// Updates and saves values for a rom to the database
    /// </summary>
    /// <param name="rom">The GeneratedRom to update</param>
    /// <param name="label">The value to update for the label</param>
    /// <returns>True if the GeneratedRom was updated</returns>
    public bool UpdateGeneratedRom(GeneratedRom rom, string? label = null);

    /// <summary>
    /// Retrieves the list of roms from the database
    /// </summary>
    /// <returns>The list of roms</returns>
    public IEnumerable<GeneratedRom> GetGeneratedRomsList();

    /// <summary>
    /// Retrieves the list of multiplayer games from the database
    /// </summary>
    /// <returns>The list of multiplayer games</returns>
    public IEnumerable<MultiplayerGameDetails> GetMultiplayerGamesList();

    /// <summary>
    /// Retrieves the history of tracked events for a roms
    /// </summary>
    /// <param name="rom">The rom to retrieve the history for</param>
    /// <returns>The list of tracked events</returns>
    public ICollection<TrackerHistoryEvent> GetGameHistory(GeneratedRom rom);

    /// <summary>
    /// Deletes the generated rom from the file system and database
    /// </summary>
    /// <param name="rom">The rom to delete</param>
    /// <param name="error">Error message from trying to delete the rom</param>
    /// <returns>True if successfully deleted, false otherwise</returns>
    public bool DeleteGeneratedRom(GeneratedRom rom, out string error);

    /// <summary>
    /// Deletes the multiplayer game and rom from the file system and database
    /// </summary>
    /// <param name="details">The multiplayer game to delete</param>
    /// <param name="error">Error message from trying to delete the rom</param>
    /// <returns>True if successfully deleted, false otherwise</returns>
    public bool DeleteMultiplayerGame(MultiplayerGameDetails details, out string error);
}
