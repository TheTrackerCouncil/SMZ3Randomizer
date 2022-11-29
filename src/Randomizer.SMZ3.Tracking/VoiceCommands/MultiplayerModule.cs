using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Randomizer.Multiplayer.Client;
using Randomizer.Shared;
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
    public MultiplayerModule(Tracker tracker, IItemService itemService, IWorldService worldService,
        ILogger<MultiplayerModule> logger, MultiplayerGameService multiplayerGameService)
        : base(tracker, itemService, worldService, logger)
    {
        _multiplayerGameService = multiplayerGameService;

        if (Tracker.Rom is not { MultiplayerGameDetails: null }) return;

        Tracker.LocationCleared += TrackerOnLocationCleared;
        Tracker.BossUpdated += TrackerOnBossUpdated;
        Tracker.ItemTracked += TrackerOnItemTracked;
        Tracker.DungeonUpdated += TrackerOnDungeonUpdated;

        _multiplayerGameService.PlayerTrackedLocation += PlayerTrackedLocation;
        _multiplayerGameService.PlayerTrackedItem += PlayerTrackedItem;
        _multiplayerGameService.PlayerTrackedBoss += PlayerTrackedBoss;
        _multiplayerGameService.PlayerTrackedDungeon += PlayerTrackedDungeon;
        _multiplayerGameService.PlayerSyncReceived += PlayerSyncReceived;

        _multiplayerGameService.SetTrackerState(worldService.World.State!);

        Logger.LogInformation("Multiplayer module initialized");
    }

    private void PlayerSyncReceived(PlayerSyncReceivedEventHandlerArgs args)
    {
        if (args.ItemsToGive == null || args.ItemsToGive.Count == 0) return;
        var items = args.ItemsToGive.Select(x => ItemService.FirstOrDefault(x)).NonNull().ToList();
        Tracker.GameService!.TryGiveItems(items, args.PlayerId);

    }

    private void PlayerTrackedItem(PlayerTrackedItemEventHandlerArgs args)
    {

    }

    private void PlayerTrackedLocation(PlayerTrackedLocationEventHandlerArgs args)
    {
        if (args.ItemToGive == ItemType.Nothing) return;
        var item = ItemService.FirstOrDefault(args.ItemToGive);
        if (item == null)
            throw new InvalidOperationException($"Player retrieved invalid item {args.ItemToGive}");
        Tracker.GameService!.TryGiveItem(item, args.PlayerId);
    }

    private void PlayerTrackedDungeon(PlayerTrackedDungeonEventHandlerArgs args)
    {

    }

    private void PlayerTrackedBoss(PlayerTrackedBossEventHandlerArgs args)
    {

    }

    private async void TrackerOnDungeonUpdated(object? sender, DungeonTrackedEventArgs e)
    {
        if (e.Dungeon == null || !e.AutoTracked) return;
        await _multiplayerGameService.TrackDungeon(e.Dungeon);
    }

    private async void TrackerOnItemTracked(object? sender, ItemTrackedEventArgs e)
    {
        if (e.Item == null || !e.AutoTracked) return;

        if (e.Item.World.Guid == Tracker.World.Guid)
            await _multiplayerGameService.TrackItem(e.Item);
    }

    private async void TrackerOnBossUpdated(object? sender, BossTrackedEventArgs e)
    {
        if (e.Boss == null || !e.AutoTracked) return;
        await _multiplayerGameService.TrackBoss(e.Boss);
    }

    private async void TrackerOnLocationCleared(object? sender, LocationClearedEventArgs e)
    {
        if (!e.AutoTracked) return;
        await _multiplayerGameService.TrackLocation(e.Location);
    }


}
