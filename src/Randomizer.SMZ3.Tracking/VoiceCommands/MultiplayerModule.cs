using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Multiplayer.Client;
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
            _multiplayerGameService.OnAutoTrackingConnected();
        });
    }

    private void PlayerSyncReceived(PlayerSyncReceivedEventHandlerArgs args)
    {
        // Ignore the sync if auto tracker is not connected as we don't want to lose out on items
        if (Tracker.AutoTracker?.IsConnected != true) return;
        if (args.PlayerId == null || args.ItemsToGive == null || args.ItemsToGive.Count == 0) return;
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
    }

    private void PlayerTrackedItem(PlayerTrackedItemEventHandlerArgs args)
    {
        args.ItemState.TrackingState = args.TrackingValue;
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
        args.LocationState.Cleared = true;
        args.LocationState.Autotracked = true;
    }

    private void PlayerTrackedDungeon(PlayerTrackedDungeonEventHandlerArgs args)
    {
        args.DungeonState.Cleared = true;
        args.DungeonState.AutoTracked = true;
    }

    private void PlayerTrackedBoss(PlayerTrackedBossEventHandlerArgs args)
    {
        args.BossState.Defeated = true;
        args.BossState.AutoTracked = true;
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
    }

}
