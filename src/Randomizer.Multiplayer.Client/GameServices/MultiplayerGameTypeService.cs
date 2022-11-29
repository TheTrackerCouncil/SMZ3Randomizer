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

    public MultiplayerGameTypeService(Smz3Randomizer randomizer, MultiplayerClientService client)
    {
        Randomizer = randomizer;
        Client = client;
    }

    protected Smz3Randomizer Randomizer { get; init; }

    protected MultiplayerClientService Client { get; }

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

    public PlayerTrackedLocationEventHandlerArgs PlayerTrackedLocation(MultiplayerPlayerState player, int locationId, bool isLocalPlayer)
    {
        var itemToGive = ItemType.Nothing;

        if (TrackerState != null && !isLocalPlayer)
        {
            var locationState =
                TrackerState.LocationStates.First(x =>
                    x.WorldId == player.WorldId && x.LocationId == locationId);
            locationState.Autotracked = true;
            locationState.Cleared = true;
            if (locationState.ItemWorldId == Client.LocalPlayer!.WorldId)
            {
                itemToGive = locationState.Item;
            }
        }

        return new PlayerTrackedLocationEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            IsLocalPlayer = isLocalPlayer,
            LocationId = locationId,
            ItemToGive = itemToGive
        };
    }

    public PlayerTrackedItemEventHandlerArgs PlayerTrackedItem(MultiplayerPlayerState player, ItemType itemType, int trackingValue, bool isLocalPlayer)
    {
        if (TrackerState != null && itemType != ItemType.Nothing && !isLocalPlayer)
        {
            var itemState = TrackerState.ItemStates.First(x => x.WorldId == player.WorldId && x.Type == itemType);
            itemState.TrackingState = trackingValue;
        }

        return new PlayerTrackedItemEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            IsLocalPlayer = isLocalPlayer,
            ItemType = itemType,
            TrackingValue = trackingValue,
        };
    }

    public PlayerTrackedBossEventHandlerArgs PlayerTrackedBoss(MultiplayerPlayerState player, BossType bossType, bool isLocalPlayer)
    {
        if (TrackerState != null && bossType != BossType.None && !isLocalPlayer)
        {
            var bossState = TrackerState.BossStates.First(x => x.WorldId == player.WorldId && x.Type == bossType);
            bossState.Defeated = true;
        }

        return new PlayerTrackedBossEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            IsLocalPlayer = isLocalPlayer,
            BossType = bossType,
        };
    }

    public PlayerTrackedDungeonEventHandlerArgs PlayerTrackedDungeon(MultiplayerPlayerState player, string dungeonName, bool isLocalPlayer)
    {
        if (TrackerState != null && !isLocalPlayer)
        {
            var dungeonState =
                TrackerState.DungeonStates.FirstOrDefault(x => x.WorldId == player.WorldId && x.Name == dungeonName);
            if (dungeonState != null)
            {
                dungeonState.Cleared = true;
            }
        }

        return new PlayerTrackedDungeonEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            IsLocalPlayer = isLocalPlayer,
            DungeonName = dungeonName,
        };
    }

    public PlayerSyncReceivedEventHandlerArgs PlayerSyncReceived(MultiplayerPlayerState player,
        MultiplayerPlayerState? previousState, bool isLocalPlayer)
    {
        var itemsToGive = new List<ItemType>();

        if (TrackerState != null && !isLocalPlayer)
        {
            foreach (var locationState in TrackerState.LocationStates.Where(x => x.WorldId == player.WorldId && !x.Cleared))
            {
                if (!player.Locations![locationState.LocationId] && !player.HasForfeited) continue;
                locationState.Autotracked = true;
                locationState.Cleared = true;
                if (locationState.ItemWorldId == Client.LocalPlayer!.WorldId)
                {
                    itemsToGive.Add(locationState.Item);
                }
            }
        }

        return new PlayerSyncReceivedEventHandlerArgs()
        {
            PlayerId = player.WorldId!.Value,
            PlayerName = player.PlayerName,
            IsLocalPlayer = isLocalPlayer,
            ItemsToGive = itemsToGive,
            DidForfeit = player.HasForfeited && previousState?.HasForfeited != true
        };
    }
}
