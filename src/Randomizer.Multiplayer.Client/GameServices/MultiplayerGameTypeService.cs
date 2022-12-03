using System.Diagnostics.CodeAnalysis;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client.GameServices;

public abstract class MultiplayerGameTypeService
{

    public MultiplayerGameTypeService(Smz3Randomizer randomizer, Smz3MultiplayerRomGenerator multiplayerRomGenerator, MultiplayerClientService client)
    {
        Randomizer = randomizer;
        Client = client;
        MultiplayerRomGenerator = multiplayerRomGenerator;
    }

    protected Smz3Randomizer Randomizer { get; init; }

    protected MultiplayerClientService Client { get; }

    protected Smz3MultiplayerRomGenerator MultiplayerRomGenerator { get; }

    protected int LocalPlayerId => Client.LocalPlayer?.WorldId ?? 0;

    public TrackerState? TrackerState { get; set; }

    public abstract SeedData? GenerateSeed(string seed, List<MultiplayerPlayerState> players,
        MultiplayerPlayerState localPlayer, out string error);

    public abstract SeedData? RegenerateSeed(string seed, List<MultiplayerPlayerGenerationData> playerGenerationData,
        List<MultiplayerPlayerState> players, MultiplayerPlayerState localPlayer,
        out string error);

    public MultiplayerPlayerState GetPlayerDefaultState(MultiplayerPlayerState state, World world)
    {
        state.Locations = world.Locations.ToDictionary(x => x.Id, _ => false);
        state.Items = world.LocationItems.Select(x => x.Type).Distinct().ToDictionary(x => x, _ => 0);
        state.Bosses = world.GoldenBosses.Select(x => x.Type).ToDictionary(x => x, _ => false);
        state.Dungeons = world.Dungeons.ToDictionary(x => x.DungeonName, _ => false);
        return state;
    }

    protected IEnumerable<Config> GetPlayerConfigs(List<MultiplayerPlayerState> players)
        => players.Select(x => Config.FromConfigString(x.Config!).First());

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

    public async Task TrackLocation(Location location)
    {
        await Client.TrackLocation(location.Id, location.World.Guid);
    }

    public async Task TrackItem(Item item)
    {
        await Client.TrackItem(item.Type, item.State.TrackingState, item.World.Guid);
    }

    public async Task TrackDungeon(IDungeon dungeon)
    {
        await Client.TrackDungeon(dungeon.DungeonName, (dungeon as Region)!.World.Guid);
    }

    public async Task TrackBoss(Boss boss)
    {
        await Client.TrackBoss(boss.Type, boss.World.Guid);
    }

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

    public PlayerSyncReceivedEventHandlerArgs? PlayerSyncReceived(MultiplayerPlayerState player,
        MultiplayerPlayerState? previousState, bool isLocalPlayer)
    {
        if (TrackerState == null || isLocalPlayer || player.Locations == null || player.Items == null || player.Bosses == null || player.Dungeons == null) return null;

        // Gather data for locations that have been cleared
        var clearedLocationIds = player.Locations.Where(x => x.Value || player.HasForfeited || player.HasCompleted).Select(x => x.Key).ToList();
        var updatedLocationStates = TrackerState.LocationStates.Where(x =>
            x.WorldId == player.WorldId && !x.Autotracked && clearedLocationIds.Contains(x.LocationId)).ToList();
        var itemsToGive = updatedLocationStates.Where(x => x.ItemWorldId == LocalPlayerId).Select(x => x.Item).ToList();

        // Gather items that have been updated
        var trackedItems = player.Items.Where(x => x.Value > 0).Select(x => x.Key).ToList();
        var updatedItemStates = TrackerState.ItemStates.Where(x =>
            x.WorldId == player.WorldId && x.Type != null && trackedItems.Contains(x.Type.Value) &&
            player.Items[x.Type.Value] > x.TrackingState).Select(x => (x, player.Items[x.Type!.Value])).ToList();

        // Gather bosses that have been defeated
        var defeatedBosses = player.Bosses.Where(x => x.Value).Select(x => x.Key).ToList();
        var updatedBossStates = TrackerState.BossStates.Where(x =>
            x.WorldId == player.WorldId && x.Type != BossType.None && !x.AutoTracked && defeatedBosses.Contains(x.Type)).ToList();

        // Gather dungeons that have been cleared
        var clearedDungeons = player.Dungeons.Where(x => x.Value).Select(x => x.Key).ToList();
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

    public void UpdatePlayerState(MultiplayerPlayerState state, TrackerState trackerState)
    {
        state.Locations = trackerState.LocationStates.Where(x => x.WorldId == state.WorldId).ToDictionary(x => x.LocationId, x => x.Autotracked);
        state.Items = trackerState.ItemStates.Where(x => x.WorldId == state.WorldId).Where(x => x.Type != null && x.Type != ItemType.Nothing).ToDictionary(x => x.Type!.Value, x => x.TrackingState);
        state.Bosses = trackerState.BossStates.Where(x => x.WorldId == state.WorldId).Where(x => x.Type != BossType.None).ToDictionary(x => x.Type, x => x.Defeated);
        state.Dungeons = trackerState.DungeonStates.Where(x => x.WorldId == state.WorldId).ToDictionary(x => x.Name, x => x.Cleared);
    }

    public PlayerEndedGameEventHandlerArgs PlayerEndedGame(MultiplayerPlayerState player, bool isLocalPlayer, bool didForfeit, bool didComplete)
    {
        return new PlayerEndedGameEventHandlerArgs
        {
            PlayerId = player.WorldId,
            PlayerName = player.PlayerName,
            PhoneticName = player.PhoneticName,
            IsLocalPlayer = isLocalPlayer,
            DidForfeit = didForfeit,
            DidComplete = didComplete
        };
    }

    public abstract void OnTrackingStarted();
}
