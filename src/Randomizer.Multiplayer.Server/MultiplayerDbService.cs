using Microsoft.EntityFrameworkCore;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Server;

/// <summary>
/// Service to handle querying from and saving to the database via a DbContextFactory
/// </summary>
public class MultiplayerDbService
{

    private readonly bool _isDatabaseEnabled;
    private readonly IDbContextFactory<MultiplayerDbContext> _contextFactory;
    private readonly ILogger<MultiplayerDbService> _logger;

    public MultiplayerDbService(IDbContextFactory<MultiplayerDbContext> contextFactory, ILogger<MultiplayerDbService> logger)
    {
        _isDatabaseEnabled = true;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Adds a new game to the database
    /// </summary>
    /// <param name="state">The game state to save to the database</param>
    public async Task AddGameToDatabase(MultiplayerGameState state)
    {
        if (!_isDatabaseEnabled || !state.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        dbContext.MultiplayerGameStates.Add(state);
        await SaveChanges(dbContext, "Unable add game to the database");
    }

    /// <summary>
    /// Loads a multiplayer game from the database
    /// </summary>
    /// <param name="guid">The guid of the game to retrieve</param>
    /// <returns>The game object, if found</returns>
    public MultiplayerGame? LoadGameFromDatabase(string guid)
    {
        if (!_isDatabaseEnabled) return null;
        using var dbContext = _contextFactory.CreateDbContext();
        try
        {
            var gameState = dbContext.MultiplayerGameStates
                .Where(x => x.Guid == guid)
                .Include(x => x.Players)
                .FirstOrDefault();

            if (gameState == null) return null;

            // For some reason grabbing these via Include or ThenInclude
            // would sometimes hang, so grab them individually and manually
            // assign them
            var gameLocations = dbContext.MultiplayerLocationStates
                .Where(x => x.GameId == gameState.Id)
                .ToList();
            var gameItems = dbContext.MultiplayerItemStates
                .Where(x => x.GameId == gameState.Id)
                .ToList();
            var gameDungeons = dbContext.MultiplayerDungeonStates
                .Where(x => x.GameId == gameState.Id)
                .ToList();
            var gameBosses = dbContext.MultiplayerBossStates
                .Where(x => x.GameId == gameState.Id)
                .ToList();

            foreach (var player in gameState.Players)
            {
                player.Locations = gameLocations.Where(x => x.PlayerId == player.Id).ToList();
                player.Items = gameItems.Where(x => x.PlayerId == player.Id).ToList();
                player.Dungeons = gameDungeons.Where(x => x.PlayerId == player.Id).ToList();
                player.Bosses = gameBosses.Where(x => x.PlayerId == player.Id).ToList();
            }

            return MultiplayerGame.LoadGameFromState(gameState);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unable to load game from database");
            return null;
        }
    }

    /// <summary>
    /// Adds a new player state to the database
    /// </summary>
    /// <param name="gameState">The game state the player belongs to</param>
    /// <param name="playerState">The player state to add</param>
    public async Task AddPlayerToGame(MultiplayerGameState gameState, MultiplayerPlayerState playerState)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        dbContext.MultiplayerPlayerStates.Add(playerState);
        await SaveChanges(dbContext, $"Unable add player {playerState.Guid} to the database");
    }

    /// <summary>
    /// Saves the current status of a game state to the database
    /// </summary>
    /// <param name="gameState">The game state to save</param>
    public async Task SaveGameState(MultiplayerGameState gameState)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var dbState = dbContext.MultiplayerGameStates
            .FirstOrDefault(x => x.Id == gameState.Id);
        if (dbState == null) return;
        dbState.Copy(gameState);
        await SaveChanges(dbContext, $"Unable to save game state {gameState.Guid}");
    }

    /// <summary>
    /// Saves all of the provided game states to the database if they are set to be saved
    /// </summary>
    /// <param name="gameStates"></param>
    public async Task SaveGameStates(IEnumerable<MultiplayerGameState> gameStates)
    {
        if (!_isDatabaseEnabled) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var gameIds = gameStates.Where(x => x.SaveToDatabase).Select(x => x.Id).ToList();
        var dbStates = dbContext.MultiplayerGameStates
            .Where(x => gameIds.Contains(x.Id));
        foreach (var dbState in dbStates)
        {
            dbState.Copy(gameStates.First(x => x.Id == dbState.Id));
        }
        await SaveChanges(dbContext, "Unable to save game states");
    }

    /// <summary>
    /// Saves a current status of a player state to the database
    /// </summary>
    /// <param name="gameState">The game state the player belongs to</param>
    /// <param name="playerState">The player state being saved</param>
    public async Task SavePlayerState(MultiplayerGameState gameState, MultiplayerPlayerState playerState)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var dbState = await dbContext.MultiplayerPlayerStates.FindAsync(playerState.Id);
        if (dbState == null) return;
        dbState.Copy(playerState);
        await SaveChanges(dbContext, $"Unable to save player state for player {playerState.Guid}");
    }

    /// <summary>
    /// Saves the current status of multiplayer states to the database
    /// </summary>
    /// <param name="gameState">The game state the players belong to</param>
    /// <param name="playerStates">Collection of player states to save</param>
    public async Task SavePlayerStates(MultiplayerGameState gameState, IEnumerable<MultiplayerPlayerState> playerStates)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var playerIds = playerStates.Select(x => x.Id).ToList();
        var dbStates = dbContext.MultiplayerPlayerStates
            .Where(x => playerIds.Contains(x.Id));
        foreach (var dbState in dbStates)
        {
            dbState.Copy(playerStates.First(x => x.Id == dbState.Id));
        }
        await SaveChanges(dbContext, "Unable to save player states");
    }

    /// <summary>
    /// Saves the current status of a location state to the database
    /// </summary>
    /// <param name="gameState">The game state the location belongs to</param>
    /// <param name="locationState">The location state being saved</param>
    public async Task SaveLocationState(MultiplayerGameState gameState, MultiplayerLocationState locationState)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var dbState = await dbContext.MultiplayerLocationStates.FindAsync(locationState.Id);
        if (dbState == null) return;
        dbState.Tracked = locationState.Tracked;
        dbState.TrackedTime = locationState.TrackedTime;
        await SaveChanges(dbContext, "Unable to save location state");
    }

    /// <summary>
    /// Saves the current status of an item to the database
    /// </summary>
    /// <param name="gameState">The game state the item belongs to</param>
    /// <param name="itemState">The item state being saved</param>
    public async Task SaveItemState(MultiplayerGameState gameState, MultiplayerItemState itemState)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var dbState = await dbContext.MultiplayerItemStates.FindAsync(itemState.Id);
        if (dbState == null) return;
        dbState.TrackingValue = itemState.TrackingValue;
        dbState.TrackedTime = itemState.TrackedTime;
        await SaveChanges(dbContext, "Unable to save item state");
    }

    /// <summary>
    /// Saves the current status of a dungeon to the database
    /// </summary>
    /// <param name="gameState">The game state the dungeon belongs to</param>
    /// <param name="dungeonState">The dungeon state being saved</param>
    public async Task SaveDungeonState(MultiplayerGameState gameState, MultiplayerDungeonState dungeonState)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var dbState = await dbContext.MultiplayerDungeonStates.FindAsync(dungeonState.Id);
        if (dbState == null) return;
        dbState.Tracked = dungeonState.Tracked;
        dbState.TrackedTime = dungeonState.TrackedTime;
        await SaveChanges(dbContext, "Unable to save dungeon state");
    }

    /// <summary>
    /// Saves the current status of a boss to the database
    /// </summary>
    /// <param name="gameState">The game state the boss belongs to</param>
    /// <param name="bossState">The boss state being saved</param>
    public async Task SaveBossState(MultiplayerGameState gameState, MultiplayerBossState bossState)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var dbState = await dbContext.MultiplayerBossStates.FindAsync(bossState.Id);
        if (dbState == null) return;
        dbState.Tracked = bossState.Tracked;
        dbState.TrackedTime = bossState.TrackedTime;
        await SaveChanges(dbContext, $"Unable to save boss state");
    }

    /// <summary>
    /// Saves a player's entire world to the dabase
    /// </summary>
    /// <param name="gameState">The game state the player belongs to</param>
    /// <param name="playerState">The player state being saved</param>
    /// <param name="updates">A collection of all updates that were made to the </param>
    public async Task SavePlayerWorld(MultiplayerGameState gameState, MultiplayerPlayerState playerState, PlayerWorldUpdates updates)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase || !updates.HasUpdates) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();

        // Update locations
        var updateIds = updates.Locations.Select(x => x.Id).ToList();
        var dbLocations = dbContext.MultiplayerLocationStates
            .Where(x => x.PlayerId == playerState.Id && updateIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);
        foreach (var updateData in updates.Locations)
        {
            if (dbLocations.ContainsKey(updateData.Id))
            {
                var dbData = dbLocations[updateData.Id];
                dbData.Tracked = updateData.Tracked;
                dbData.TrackedTime = updateData.TrackedTime;
            }
            else
            {
                dbContext.MultiplayerLocationStates.Add(updateData);
            }
        }

        // Update items
        updateIds = updates.Items.Select(x => x.Id).ToList();
        var dbItems = dbContext.MultiplayerItemStates
            .Where(x => x.PlayerId == playerState.Id && updateIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);
        foreach (var updateData in updates.Items)
        {
            if (dbItems.ContainsKey(updateData.Id))
            {
                var dbData = dbItems[updateData.Id];
                dbData.TrackingValue = updateData.TrackingValue;
                dbData.TrackedTime = updateData.TrackedTime;
            }
            else
            {
                dbContext.MultiplayerItemStates.Add(updateData);
            }
        }

        // Update dungeons
        updateIds = updates.Dungeons.Select(x => x.Id).ToList();
        var dbDungeons = dbContext.MultiplayerDungeonStates
            .Where(x => x.PlayerId == playerState.Id && updateIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);
        foreach (var updateData in updates.Dungeons)
        {
            if (dbDungeons.ContainsKey(updateData.Id))
            {
                var dbData = dbDungeons[updateData.Id];
                dbData.Tracked = updateData.Tracked;
                dbData.TrackedTime = updateData.TrackedTime;
            }
            else
            {
                dbContext.MultiplayerDungeonStates.Add(updateData);
            }
        }

        // Update bosses
        updateIds = updates.Bosses.Select(x => x.Id).ToList();
        var dbBosses = dbContext.MultiplayerBossStates
            .Where(x => x.PlayerId == playerState.Id && updateIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);
        foreach (var updateData in updates.Bosses)
        {
            if (dbBosses.ContainsKey(updateData.Id))
            {
                var dbData = dbBosses[updateData.Id];
                dbData.Tracked = updateData.Tracked;
                dbData.TrackedTime = updateData.TrackedTime;
            }
            else
            {
                dbContext.MultiplayerBossStates.Add(updateData);
            }
        }

        await SaveChanges(dbContext, $"Unable to save player world {playerState.Guid}");
    }

    /// <summary>
    /// Deletes a player entirely from the database. Called if a player forfeits before a game starts.
    /// </summary>
    /// <param name="gameState">The game state the player belongs to</param>
    /// <param name="playerState">The player to delete</param>
    public async Task DeletePlayerState(MultiplayerGameState gameState, MultiplayerPlayerState playerState)
    {
        if (!_isDatabaseEnabled || !gameState.SaveToDatabase) return;
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var dbState = await dbContext.MultiplayerPlayerStates.FindAsync(playerState.Id);
        if (dbState == null) return;
        dbContext.MultiplayerPlayerStates.Remove(dbState);
        await SaveChanges(dbContext, $"Unable to delete player state {playerState.Guid}");
    }

    /// <summary>
    /// Deletes games that haven't been updated in a certain period of time
    /// </summary>
    /// <param name="expirationDays">The number of days before an inactive game is deleted</param>
    /// <returns>The number of deleted games</returns>
    public async Task<List<string>> DeleteOldGameStates(int expirationDays)
    {
        if (!_isDatabaseEnabled) return new List<string>();
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var expirationDate = DateTimeOffset.Now - TimeSpan.FromDays(expirationDays);
        var dbStates = dbContext.MultiplayerGameStates.Where(x => x.LastMessage.CompareTo(expirationDate) < 0);
        var deletedGuids = new List<string>();
        foreach (var dbState in dbStates)
        {
            dbContext.Remove(dbState);
            deletedGuids.Add(dbState.Guid);
        }

        await SaveChanges(dbContext, "Unable to delete old game saves");
        return deletedGuids;
    }

    private async Task SaveChanges(MultiplayerDbContext dbContext, string errorMessage)
    {
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "{Message}", errorMessage);
        }
    }
}
