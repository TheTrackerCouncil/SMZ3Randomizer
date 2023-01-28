using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Multiplayer.Client;
using Randomizer.Multiplayer.Client.EventHandlers;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3.Tracking.AutoTracking;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands;

public class MultiplayerModule : TrackerModule
{
    private readonly MultiplayerGameService _multiplayerGameService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplayerModule"/>
    /// class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="itemService">Service to get item information</param>
    /// <param name="worldService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    /// <param name="multiplayerGameService">The multiplayer game service</param>
    /// <param name="autoTracker"></param>
    public MultiplayerModule(Tracker tracker, IItemService itemService, IWorldService worldService,
        ILogger<MultiplayerModule> logger, MultiplayerGameService multiplayerGameService, AutoTracker autoTracker)
        : base(tracker, itemService, worldService, logger)
    {
        _multiplayerGameService = multiplayerGameService;

        if (Tracker.Rom is { MultiplayerGameDetails: null }) return;

        Tracker.LocationCleared += TrackerOnLocationCleared;
        Tracker.BossUpdated += TrackerOnBossUpdated;
        Tracker.ItemTracked += TrackerOnItemTracked;
        Tracker.DungeonUpdated += TrackerOnDungeonUpdated;
        Tracker.BeatGame += TrackerOnBeatGame;
        autoTracker.AutoTrackerConnected += AutoTrackerOnAutoTrackerConnected;

        _multiplayerGameService.PlayerTrackedLocation += PlayerTrackedLocation;
        _multiplayerGameService.PlayerTrackedItem += PlayerTrackedItem;
        _multiplayerGameService.PlayerTrackedBoss += PlayerTrackedBoss;
        _multiplayerGameService.PlayerTrackedDungeon += PlayerTrackedDungeon;
        _multiplayerGameService.PlayerSyncReceived += PlayerSyncReceived;
        _multiplayerGameService.PlayerEndedGame += PlayerEndedGame;

        _multiplayerGameService.SetTrackerState(worldService.World.State!);
        _multiplayerGameService.OnTrackingStarted();

        Logger.LogInformation("Multiplayer module initialized");
    }

    private void AutoTrackerOnAutoTrackerConnected(object? sender, EventArgs e)
    {
        // Unfortunately sending items immediately after auto tracker has connected seems to cause the items to get
        // lost, so wait 5 seconds before trying to send the items
        Task.Run(async () =>
        {
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                if (Tracker.AutoTracker?.IsConnected != true) return;
            } while (Tracker.AutoTracker!.CurrentGame == Game.Neither);
            Logger.LogInformation("Requesting player sync");
            await _multiplayerGameService.OnAutoTrackingConnected();
        });
    }

    private void PlayerSyncReceived(PlayerSyncReceivedEventHandlerArgs args)
    {
        // Ignore the sync if auto tracker is not connected as we don't want to lose out on items
        if (Tracker.AutoTracker?.IsConnected != true) return;
        if (args.PlayerId == null || args.ItemsToGive == null || args.ItemsToGive.Count == 0 || args.IsLocalPlayer) return;
        var items = args.ItemsToGive.Select(x => ItemService.FirstOrDefault(x)).NonNull().ToList();
        Tracker.GameService!.TryGiveItems(items, args.PlayerId.Value);

        foreach (var locationState in args.UpdatedLocationStates)
        {
            locationState.Cleared = true;
            locationState.Autotracked = true;
        }

        foreach (var itemState in args.UpdatedItemStates)
        {
            itemState.State.TrackingState = itemState.TrackingValue;
        }

        foreach (var bossState in args.UpdatedBossStates)
        {
            bossState.Defeated = true;
            bossState.AutoTracked = true;
        }

        foreach (var dungeonState in args.UpdatedDungeonStates)
        {
            dungeonState.Cleared = true;
            dungeonState.AutoTracked = true;
        }

    }

    private void PlayerEndedGame(PlayerEndedGameEventHandlerArgs args)
    {
        // Comment on player forfeiting or completing
        if (args.IsLocalPlayer || _multiplayerGameService.LocalPlayer == null ||
            _multiplayerGameService.LocalPlayer.HasCompleted ||
            _multiplayerGameService.LocalPlayer.HasForfeited) return;
        if (args.DidComplete)
        {
            if (args.SendItemsOnComplete)
                Tracker.Say(x => x.Multiplayer.OtherPlayerBeatGame, args.PhoneticName);
            else
                Tracker.Say(x => x.Multiplayer.OtherPlayerBeatGameNoItems, args.PhoneticName);
        }
        else if (args.DidForfeit)
        {
            Tracker.Say(x => x.Multiplayer.OtherPlayerForfeitGame, args.PhoneticName);
        }
    }

    private void PlayerTrackedItem(PlayerTrackedItemEventHandlerArgs args)
    {
        args.ItemState.TrackingState = args.TrackingValue;
        if (args.ItemState.Type == null || args.IsLocalPlayer) return;
        var item = ItemService.FirstOrDefault(args.ItemState.Type.Value);
        if (item == null || item.State.TrackingState >= args.TrackingValue || !item.Progression)
        {
            return;
        }

        Tracker.Say(x => x.Multiplayer.OtherPlayerTrackedItem, args.PhoneticName, item.Metadata.Name,
            item.Metadata.NameWithArticle);
    }

    private void PlayerTrackedLocation(PlayerTrackedLocationEventHandlerArgs args)
    {
        // Ignore the sync if auto tracker is not connected as we don't want to lose out on items
        if (Tracker.AutoTracker?.IsConnected != true) return;
        if (args.ItemToGive == ItemType.Nothing) return;
        var item = ItemService.FirstOrDefault(args.ItemToGive);
        if (item == null)
            throw new InvalidOperationException($"Player retrieved invalid item {args.ItemToGive}");
        Tracker.GameService!.TryGiveItem(item, args.PlayerId);
        if (item.Type.IsPossibleProgression(item.World.Config.ZeldaKeysanity, item.World.Config.MetroidKeysanity))
        {
            Tracker.Say(x => x.Multiplayer.ReceivedUsefulItemFromOtherPlayer, args.PhoneticName, item.Metadata.Name,
                item.Metadata.NameWithArticle);
        }
        else if (item.Type.IsInCategory(ItemCategory.Junk))
        {
            Tracker.Say(x => x.Multiplayer.ReceivedJunkItemFromOtherPlayer, args.PhoneticName, item.Metadata.Name,
                item.Metadata.NameWithArticle);
        }
        args.LocationState.Cleared = true;
        args.LocationState.Autotracked = true;
    }

    private void PlayerTrackedDungeon(PlayerTrackedDungeonEventHandlerArgs args)
    {
        args.DungeonState.Cleared = true;
        args.DungeonState.AutoTracked = true;
        var dungeon = WorldService.GetWorld(args.PlayerId).Dungeons
            .FirstOrDefault(x => x.GetType().Name == args.DungeonState.Name);
        if (dungeon == null) return;
        if (dungeon is { HasReward: true, DungeonReward: { } })
        {
            Tracker.Say(x => x.Multiplayer.OtherPlayerClearedDungeonWithReward, args.PhoneticName,
                dungeon.DungeonMetadata.Name, dungeon.DungeonMetadata.Boss, dungeon.DungeonReward.Metadata.Name,
                dungeon.DungeonReward.Metadata.NameWithArticle);
        }
        else
        {
            Tracker.Say(x => x.Multiplayer.OtherPlayerClearedDungeonWithoutReward, args.PhoneticName,
                dungeon.DungeonMetadata.Name, dungeon.DungeonMetadata.Boss);
        }
    }

    private void PlayerTrackedBoss(PlayerTrackedBossEventHandlerArgs args)
    {
        args.BossState.Defeated = true;
        args.BossState.AutoTracked = true;
        var boss = WorldService.World.GoldenBosses.FirstOrDefault(x => x.Type == args.BossState.Type);
        if (boss == null) return;
        Tracker.Say(x => x.Multiplayer.OtherPlayerDefeatedBoss, args.PhoneticName, boss.Metadata.Name);
    }

    private async void TrackerOnDungeonUpdated(object? sender, DungeonTrackedEventArgs e)
    {
        if (e.Dungeon == null || !e.AutoTracked || !e.Dungeon.DungeonState.Cleared) return;
        await _multiplayerGameService.TrackDungeon(e.Dungeon);
    }

    private async void TrackerOnItemTracked(object? sender, ItemTrackedEventArgs e)
    {
        if (e.Item == null || e.Item.Type == ItemType.Nothing || !e.AutoTracked) return;

        if (e.Item.World.Guid == Tracker.World.Guid)
            await _multiplayerGameService.TrackItem(e.Item);
    }

    private async void TrackerOnBossUpdated(object? sender, BossTrackedEventArgs e)
    {
        if (e.Boss == null || e.Boss.Type == BossType.None || !e.AutoTracked) return;
        await _multiplayerGameService.TrackBoss(e.Boss);
    }

    private async void TrackerOnLocationCleared(object? sender, LocationClearedEventArgs e)
    {
        if (!e.AutoTracked) return;
        await _multiplayerGameService.TrackLocation(e.Location);
        if (e.Location.World == e.Location.Item.World || !e.Location.Item.Progression) return;
        var localItem = ItemService.FirstOrDefault(e.Location.Item.Type);
        if (localItem == null || localItem.State.TrackingState >= e.Location.Item.State.TrackingState) return;
        var otherPlayer = e.Location.Item.World.Config.PhoneticName;
        Tracker.Say(x => x.Multiplayer.GiftedUsefulItemToOtherPlayer, otherPlayer, localItem.Metadata.Name,
            localItem.Metadata.NameWithArticle);
    }

    private async void TrackerOnBeatGame(object? sender, TrackerEventArgs e)
    {
        if (!e.AutoTracked) return;
        await _multiplayerGameService.CompletePlayerGame();
    }

}
