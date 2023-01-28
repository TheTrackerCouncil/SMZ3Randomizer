using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Multiplayer.Client.EventHandlers;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client.GameServices;

/// <summary>
/// Abstract class for all of the different multiplayer game types
/// Houses a lot of of the basic logic for generating seeds and
/// handling players tracking locations and items
/// </summary>
public abstract class MultiplayerGameTypeService : IDisposable
{
    protected MultiplayerGameTypeService(Smz3Randomizer randomizer, Smz3MultiplayerRomGenerator multiplayerRomGenerator, MultiplayerClientService client)
    {
        Randomizer = randomizer;
        Client = client;
        MultiplayerRomGenerator = multiplayerRomGenerator;
    }

    public TrackerState? TrackerState { get; set; }

    protected Smz3Randomizer Randomizer { get; init; }

    protected MultiplayerClientService Client { get; }

    protected Smz3MultiplayerRomGenerator MultiplayerRomGenerator { get; }

    protected int LocalPlayerId => Client.LocalPlayer?.WorldId ?? 0;

    /// <summary>
    /// Generates seed data for all of the players
    /// </summary>
    /// <param name="seed">The seed for the random generator</param>
    /// <param name="players">The list of player states</param>
    /// <param name="localPlayer">The local player's player state</param>
    /// <param name="error">Output of any error that happened</param>
    /// <returns>The seed data object with all of the world details</returns>
    public abstract SeedData? GenerateSeed(string seed, List<MultiplayerPlayerState> players,
        MultiplayerPlayerState localPlayer, out string error);

    /// <summary>
    /// Regenerates seed data given pregenerated data for each of the players
    /// </summary>
    /// <param name="seed">The seed for the random generator</param>
    /// <param name="playerGenerationData">The list of generation data for each of the players</param>
    /// <param name="players">The list of player states</param>
    /// <param name="localPlayer">The local player's player state</param>
    /// <param name="error">Output of any error that happened</param>
    /// <returns>The seed data object with all of the world details</returns>
    public abstract SeedData? RegenerateSeed(string seed, List<MultiplayerPlayerGenerationData> playerGenerationData,
        List<MultiplayerPlayerState> players, MultiplayerPlayerState localPlayer,
        out string error);

    /// <summary>
    /// Creates the default player state based on the player's world
    /// </summary>
    /// <param name="world">The world of the player</param>
    /// <param name="allWorlds"></param>
    /// <returns>The newly created multiplayer player state</returns>
    public MultiplayerWorldState GetPlayerDefaultState(World world, IEnumerable<World> allWorlds)
    {

        // Grab the item types that belong to this player, not the ones that are in their world
        var playerItemTypes = allWorlds
            .SelectMany(x => x.LocationItems)
            .Where(x => x.World.Id == world.Id)
            .Select(x => x.Type)
            .Distinct()
            .ToList();
        var locations = world.Locations.ToDictionary(x => x.Id, _ => false);
        var items = playerItemTypes.ToDictionary(x => x, _ => 0);
        var bosses = world.GoldenBosses.Select(x => x.Type).ToDictionary(x => x, _ => false);
        var dungeons = world.Dungeons.ToDictionary(x => x.GetType().Name, _ => false);
        var state = new MultiplayerWorldState(locations, items, bosses, dungeons);
        return state;
    }

    /// <summary>
    /// Generates seed data from a list of player configs
    /// </summary>
    /// <param name="configs">The list of player configs</param>
    /// <param name="seed">The seed number to use for random generation</param>
    /// <param name="error">Output of any error that happened</param>
    /// <returns>The generated seed data with all of the worlds</returns>
    protected SeedData? GenerateSeedInternal(List<Config> configs, string? seed, out string error)
    {
        SeedData? seedData = null;
        var validated = false;
        error = "";
        for (var i = 0; i < 3; i++)
        {
            try
            {
                seedData = Randomizer.GenerateSeed(configs, seed, CancellationToken.None);
                if (!Randomizer.ValidateSeedSettings(seedData))
                {
                    error = "";
                }
                else
                {
                    validated = true;
                    break;
                }
            }
            catch (RandomizerGenerationException e)
            {
                seedData = null;
                error = $"Error generating rom\n{e.Message}\nPlease try again. If it persists, try modifying your seed settings.";
            }
        }

        if (!validated)
        {
            error = $"Could not successfully generate a seed with requested settings.";
            seedData = null;
        }

        return seedData;
    }

    /// <summary>
    /// Regenerates seed data from a list of configs that have the pregenerated multiplayer world data
    /// </summary>
    /// <param name="configs">The list of configs with the player data</param>
    /// <param name="seed">The seed to use for the random generator</param>
    /// <param name="error">Output of any error that happened</param>
    /// <returns>The regenerated seed data</returns>
    protected SeedData? RegenerateSeedInternal(List<Config> configs, string? seed, out string error)
    {
        SeedData? seedData = null;
        var validated = false;
        error = "";
        for (var i = 0; i < 3; i++)
        {
            try
            {
                seedData = MultiplayerRomGenerator.GenerateSeed(configs, seed, CancellationToken.None);
                if (!Randomizer.ValidateSeedSettings(seedData))
                {
                    error = "";
                }
                else
                {
                    validated = true;
                    break;
                }
            }
            catch (RandomizerGenerationException e)
            {
                seedData = null;
                error = $"Error generating rom\n{e.Message}\nPlease try again. If it persists, try modifying your seed settings.";
            }
        }

        if (!validated)
        {
            error = $"Could not successfully generate a seed with requested settings.";
            seedData = null;
        }

        return seedData;
    }

    /// <summary>
    /// Creates a has for each of the worlds to use to confirm that seeds match
    /// </summary>
    /// <param name="worlds">The list of worlds to use to generate the hash</param>
    /// <returns>A unique hash for all of the location items and dungeon rewards</returns>
    public string GetValidationHash(IEnumerable<World> worlds)
    {
        var itemHashCode = string.Join(",",
            worlds.SelectMany(x => x.Locations).OrderBy(x => x.World.Id).ThenBy(x => x.Id)
                .Select(x => x.Item.Type.ToString()));
        var rewardHashCode = string.Join(",",
            worlds.SelectMany(x => x.Regions).OrderBy(x => x.World.Id)
                .ThenBy(x => x.Name)
                .OfType<IHasReward>().Select(x => x.RewardType.ToString()));
        return $"{NonCryptographicHash.Fnv1a(itemHashCode)}{NonCryptographicHash.Fnv1a(rewardHashCode)}";
    }

    /// <summary>
    /// Notifies the server that a location has been tracked by the local player
    /// </summary>
    /// <param name="location">The location that was tracked</param>
    public async Task TrackLocation(Location location)
    {
        await Client.TrackLocation(location.Id, location.World.Guid);
    }

    /// <summary>
    /// Notifies the server that an item belonging to the local player has been tracked
    /// </summary>
    /// <param name="item">The item that has been tracked</param>
    public async Task TrackItem(Item item)
    {
        await Client.TrackItem(item.Type, item.State.TrackingState, item.World.Guid);
    }

    /// <summary>
    /// Notifies the server that a dungeon has been tracked by the local player
    /// </summary>
    /// <param name="dungeon">The dungeon that has been tracked</param>
    public async Task TrackDungeon(IDungeon dungeon)
    {
        await Client.TrackDungeon(dungeon.DungeonState.Name, (dungeon as Region)!.World.Guid);
    }

    /// <summary>
    /// Notifies the server that a boss has been tracked by the local player
    /// </summary>
    /// <param name="boss">The boss that has been tracked</param>
    public async Task TrackBoss(Boss boss)
    {
        await Client.TrackBoss(boss.Type, boss.World.Guid);
    }

    /// <summary>
    /// Creates arguments to send to Tracker when a player tracks a location which includes the location state
    /// and an item type to give to the player, if applicable
    /// </summary>
    /// <param name="player">The player that tracked a location</param>
    /// <param name="locationId">The id of </param>
    /// <param name="isLocalPlayer"></param>
    /// <returns></returns>
    public PlayerTrackedLocationEventHandlerArgs? PlayerTrackedLocation(MultiplayerPlayerState player, int locationId, bool isLocalPlayer)
    {
        if (TrackerState == null || isLocalPlayer) return null;

        var locationState = TrackerState.LocationStates.FirstOrDefault(x =>
            x.WorldId == player.WorldId && x.LocationId == locationId);
        if (locationState == null || locationState.Autotracked) return null;

        var itemToGive = locationState.ItemWorldId == LocalPlayerId ? locationState.Item : ItemType.Nothing;

        return new PlayerTrackedLocationEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            PhoneticName = player.PhoneticName,
            IsLocalPlayer = isLocalPlayer,
            LocationState = locationState,
            ItemToGive = itemToGive
        };
    }

    /// <summary>
    /// Creates arguments to send to Tracker when a player tracks an item
    /// </summary>
    /// <param name="player">The player that tracked an item</param>
    /// <param name="itemType">The type of item that was tracked</param>
    /// <param name="trackingValue">Thew new tracking value for the item</param>
    /// <param name="isLocalPlayer">If it was the local player tracking the item</param>
    /// <returns></returns>
    public PlayerTrackedItemEventHandlerArgs? PlayerTrackedItem(MultiplayerPlayerState player, ItemType itemType, int trackingValue, bool isLocalPlayer)
    {
        if (TrackerState == null || itemType == ItemType.Nothing || isLocalPlayer) return null;

        var itemState = TrackerState.ItemStates.FirstOrDefault(x => x.WorldId == player.WorldId && x.Type == itemType);
        if (itemState == null || itemState.TrackingState > trackingValue) return null;

        return new PlayerTrackedItemEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            PhoneticName = player.PhoneticName,
            IsLocalPlayer = isLocalPlayer,
            ItemState = itemState,
            TrackingValue = trackingValue,
        };
    }

    /// <summary>
    /// Creates arguments to send to Tracker when a player tracks a boss
    /// </summary>
    /// <param name="player">The player that tracked a boss</param>
    /// <param name="bossType">The boss type that was tracked</param>
    /// <param name="isLocalPlayer">If it was the local player tracking the item</param>
    /// <returns></returns>
    public PlayerTrackedBossEventHandlerArgs? PlayerTrackedBoss(MultiplayerPlayerState player, BossType bossType, bool isLocalPlayer)
    {
        if (TrackerState == null || bossType == BossType.None || isLocalPlayer) return null;

        var bossState = TrackerState.BossStates.FirstOrDefault(x => x.WorldId == player.WorldId && x.Type == bossType);
        if (bossState == null || bossState.AutoTracked) return null;

        return new PlayerTrackedBossEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            PhoneticName = player.PhoneticName,
            IsLocalPlayer = isLocalPlayer,
            BossState = bossState,
        };
    }

    /// <summary>
    /// Creates arguments to send to Tracker when a player tracks a dungeon
    /// </summary>
    /// <param name="player">The player that tracked the dungeon</param>
    /// <param name="dungeonName">The name of the dungeon that was tracked</param>
    /// <param name="isLocalPlayer">If it was the local player tracking the item</param>
    public PlayerTrackedDungeonEventHandlerArgs? PlayerTrackedDungeon(MultiplayerPlayerState player, string dungeonName, bool isLocalPlayer)
    {
        if (TrackerState == null || isLocalPlayer) return null;
        var dungeonState =
            TrackerState.DungeonStates.FirstOrDefault(x => x.WorldId == player.WorldId && x.Name == dungeonName);
        if (dungeonState == null || dungeonState.AutoTracked) return null;

        return new PlayerTrackedDungeonEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            PhoneticName = player.PhoneticName,
            IsLocalPlayer = isLocalPlayer,
            DungeonState = dungeonState,
        };
    }

    /// <summary>
    /// Creates arguments for when receiving a player state from the server by determining what locations, items, etc.
    /// mismatch with the tracker state
    /// </summary>
    /// <param name="player">The player state that is being synced</param>
    /// <param name="previousState">The previous state for the player before this one</param>
    /// <param name="isLocalPlayer">If this is the local player being synced</param>
    /// <returns></returns>
    public PlayerSyncReceivedEventHandlerArgs? PlayerSyncReceived(MultiplayerPlayerState player,
        MultiplayerPlayerState? previousState, bool isLocalPlayer)
    {
        if (TrackerState == null || isLocalPlayer || player.Locations == null || player.Items == null || player.Bosses == null || player.Dungeons == null) return null;

        var ifSendItemsOnComplete = Client.CurrentGameState?.SendItemsOnComplete ?? true;

        // Gather data for locations that have been cleared
        var clearedLocationIds = player.Locations.Where(x => x.Tracked || player.HasForfeited || (ifSendItemsOnComplete && player.HasCompleted)).Select(x => x.LocationId).ToList();
        var updatedLocationStates = TrackerState.LocationStates.Where(x =>
            x.WorldId == player.WorldId && !x.Autotracked && clearedLocationIds.Contains(x.LocationId)).ToList();
        var itemsToGive = updatedLocationStates.Where(x => x.ItemWorldId == LocalPlayerId).Select(x => x.Item).ToList();

        // Gather items that have been updated
        var trackedItems = player.Items.Where(x => x.TrackingValue > 0).Select(x => x.Item).ToList();
        var updatedItemStates = TrackerState.ItemStates.Where(x =>
            x.WorldId == player.WorldId && x.Type != null && trackedItems.Contains(x.Type.Value) &&
            player.GetItem(x.Type.Value)!.TrackingValue > x.TrackingState).Select(x => (x, player.GetItem(x.Type!.Value)!.TrackingValue)).ToList();

        // Gather bosses that have been defeated
        var defeatedBosses = player.Bosses.Where(x => x.Tracked).Select(x => x.Boss).ToList();
        var updatedBossStates = TrackerState.BossStates.Where(x =>
            x.WorldId == player.WorldId && x.Type != BossType.None && !x.AutoTracked && defeatedBosses.Contains(x.Type)).ToList();

        // Gather dungeons that have been cleared
        var clearedDungeons = player.Dungeons.Where(x => x.Tracked).Select(x => x.Dungeon).ToList();
        var updatedDungeonStates = TrackerState.DungeonStates.Where(x =>
            x.WorldId == player.WorldId && !x.AutoTracked && clearedDungeons.Contains(x.Name)).ToList();

        return new PlayerSyncReceivedEventHandlerArgs()
        {
            PlayerId = player.WorldId,
            PlayerName = player.PlayerName,
            PhoneticName = player.PhoneticName,
            IsLocalPlayer = isLocalPlayer,
            UpdatedLocationStates = updatedLocationStates,
            UpdatedItemStates = updatedItemStates,
            UpdatedBossStates = updatedBossStates,
            UpdatedDungeonStates = updatedDungeonStates,
            ItemsToGive = itemsToGive,
            DidForfeit = player.HasForfeited && previousState?.HasForfeited != true,
            DidComplete = player.HasCompleted && previousState?.HasCompleted != true
        };
    }

    /// <summary>
    /// Updates a player state by merging the previous multiplayer state and the current tracker state
    /// </summary>
    /// <param name="state">The player that is being updated</param>
    /// <param name="trackerState">The tracker state for the player</param>
    public MultiplayerWorldState GetPlayerWorldState(MultiplayerPlayerState state, TrackerState trackerState)
    {
        // Grab the tracker state values for the current world, then get the applicable multiplayer state value for
        // that same location, item, etc. Use the greater of the two values to update the state dictionaries to send
        // over to the server
        var locations = trackerState.LocationStates
            .Where(x => x.WorldId == state.WorldId)
            .ToDictionary(x => x.LocationId, x => x.Autotracked);

        var items = trackerState.ItemStates
            .Where(x => x.WorldId == state.WorldId && x.Type != null && x.Type != ItemType.Nothing)
            .ToDictionary(x => x.Type ?? ItemType.Nothing, x => x.TrackingState);

        var bosses = trackerState.BossStates
            .Where(x => x.WorldId == state.WorldId && x.Type != BossType.None)
            .ToDictionary(x => x.Type, x => x.AutoTracked);

        var dungeons = trackerState.DungeonStates
            .Where(x => x.WorldId == state.WorldId)
            .ToDictionary(x => x.Name, x => x.AutoTracked);

        return new MultiplayerWorldState(locations, items, bosses, dungeons);
    }

    /// <summary>
    /// Creates arguments for when a player's game has ended, either by forfeiting or by completing the game
    /// </summary>
    /// <param name="player">The player twhose game was ended</param>
    /// <param name="isLocalPlayer">If it was a local player or not</param>
    /// <param name="didForfeit">If the player forfeited</param>
    /// <param name="didComplete">If the player completed the game</param>
    /// <returns></returns>
    public PlayerEndedGameEventHandlerArgs PlayerEndedGame(MultiplayerPlayerState player, bool isLocalPlayer, bool didForfeit, bool didComplete)
    {
        return new PlayerEndedGameEventHandlerArgs
        {
            PlayerId = player.WorldId,
            PlayerName = player.PlayerName,
            PhoneticName = player.PhoneticName,
            IsLocalPlayer = isLocalPlayer,
            DidForfeit = didForfeit,
            DidComplete = didComplete,
            SendItemsOnComplete = Client.CurrentGameState?.SendItemsOnComplete ?? true
        };
    }

    public abstract void OnTrackingStarted();

    public void OnAutoTrackerConnected()
    {
        if (!RunSync)
        {
            RunSync = true;
            Task.Run(RunPlayerSyncLoop);
        }
    }

    public void Dispose()
    {
        RunSync = false;
        GC.SuppressFinalize(this);
    }

    protected bool RunSync { get; set; }

    private async Task RunPlayerSyncLoop()
    {
        while (RunSync)
        {
            await PlayerSync();
            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }

    protected async Task PlayerSync()
    {
        if (Client.LocalPlayer != null && TrackerState != null)
        {
            var world = GetPlayerWorldState(Client.LocalPlayer, TrackerState);
            await Client.UpdatePlayerWorld(Client.LocalPlayer, world);
        }
    }


}
